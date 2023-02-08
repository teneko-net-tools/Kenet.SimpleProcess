using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using Kenet.SimpleProcess.Threading;

namespace Kenet.SimpleProcess;

/// <summary>
/// A simple process trying to replace the bootstrap code of a native instance of <see cref="Process"/>.
/// </summary>
public sealed partial class SimpleProcess :
    IProcessExecution,
    IAsyncProcessExecution,
    IRunnable<IProcessExecution>,
    IRunnable<IAsyncProcessExecution>
{
    private static readonly TimeSpan s_defaultKillTreeTimeout = TimeSpan.FromSeconds(10);

    private static async Task ReadStreamAsync(
        Stream source,
        WriteHandler writeNextBytes,
        CancellationToken cancellationToken)
    {
        await Task.Yield(); // No ConfigureAwait(bool) available
        var cancellableTaskSource = new TaskCompletionSource<int>();
        var cancellableTask = cancellableTaskSource.Task;

        using var cancelTaskSource = cancellationToken.Register(
            () => _ = cancellableTaskSource.TrySetException(new OperationCanceledException(cancellationToken)),
            useSynchronizationContext: false);

        using var memoryOwner = MemoryPool<byte>.Shared.Rent(1024 * 4);

        @continue:
        try {
            if (cancellationToken.IsCancellationRequested) {
                goto @break;
            }

            // ISSUE: https://github.com/dotnet/runtime/issues/28583
            var lastWrittenBytesCountTask = await Task.WhenAny(
                    source.ReadAsync(memoryOwner.Memory, cancellationToken).AsTask(),
                    cancellableTask)
                .ConfigureAwait(false);

            // Call to GetAwaiter.GetResult() unwraps exception, if any
            var lastWrittenBytesCount = lastWrittenBytesCountTask.GetAwaiter().GetResult();

            if (lastWrittenBytesCount != 0) {
                writeNextBytes(memoryOwner.Memory.Span[..lastWrittenBytesCount]);
                goto @continue;
            }
        } catch (OperationCanceledException error) when (error.CancellationToken.Equals(cancellationToken)) {
            // We cancelled the read task by us, so we ignore because the cancellation gets handled in RunToCompletion[Async] either way
            goto @break;
        } catch {
            // We tried to write the bytes, but an exception, that does not originate from us, occurred, so we continue the reading
            goto @continue;
        }

        @break:
        _ = cancellableTaskSource.TrySetResult(default);
        // We always end with with an EOF, otherwise the consumers may get irritated
        writeNextBytes(EOF.Memory.Span);
    }

    /// <summary>
    /// The original start info.
    /// </summary>
    public SimpleProcessStartInfo StartInfo { get; }

    private readonly CancellationTokenSource _processStartedTokenSource;

    /// <summary>
    /// The buffer the process will write incoming error to.
    /// </summary>
    public WriteHandler? ErrorWriter { private get; init; }

    /// <summary>
    /// The buffer the process will write incoming output to.
    /// </summary>
    public WriteHandler? OutputWriter { private get; init; }

    /// <summary>
    /// The process id.
    /// </summary>
    public int? Id { get; private set; }

    int IExecutingProcess.Id {
        get {
            ThrowIfNotStarted();
            return Id.Value;
        }
    }

    /// <inheritdoc/>
    public CancellationToken Started { get; }

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(_readOutputTask), nameof(_readErrorTask))]
    public bool HasStarted { get; private set; }

    /// <inheritdoc/>
    public CancellationToken Cancelled { get; }

    /// <inheritdoc/>
    public bool IsCancelled => _processCancellationTokenSource.IsCancellationRequested;

    /// <inheritdoc/>
    public CancellationToken Exited { get; }

    /// <inheritdoc/>
    public bool IsExited {
        get => _isExited;
        private set => _isExited = value;
    }

    /// <inheritdoc/>
    public bool IsCompleted => IsExited || IsCancelled;

    /// <inheritdoc/>
    public bool IsDisposed => _isDisposed == 1;

    internal Process? _process;

    private Task? _readOutputTask;
    private Task? _readErrorTask;

    private readonly object _processEndingLock = new();
    private readonly CancellationTokenSource _processCancellationTokenSource;
    private readonly CancellationTokenSource _processExitedTokenSource;
    private readonly object _startProcessLock = new();
    private int? _exitCode;
    private int _isDisposed;
    private volatile bool _isExited;

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    /// <param name="startInfo"></param>
    /// <param name="cancellationToken">
    /// The token, that if cancellation requested, causes the process to fall into the cancelling state.
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public SimpleProcess(SimpleProcessStartInfo startInfo, CancellationToken cancellationToken)
    {
        StartInfo = startInfo ?? throw new ArgumentNullException(nameof(startInfo));

        _processStartedTokenSource = new CancellationTokenSource();
        Started = _processStartedTokenSource.Token;

        /* REMINDER: When the process exited, the readers may not yet have processed all bytes so far,
         * so it may occur, that the readers get canceled before the last bytes are read. Therefore
         * the cancelled token source should not be canceled when exited. */
        _processCancellationTokenSource = cancellationToken.CanBeCanceled
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : new CancellationTokenSource();

        Cancelled = _processCancellationTokenSource.Token;

        // REMINDER: process exit should trigger process cancellation, but the other way around should the process cancellation not
        // influence the process exit.
        _processExitedTokenSource = new CancellationTokenSource();
        Exited = _processExitedTokenSource.Token;
    }

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    /// <param name="startInfo"></param>
    public SimpleProcess(SimpleProcessStartInfo startInfo)
        : this(startInfo, CancellationToken.None)
    {
    }

    [MemberNotNull(nameof(Id))]
    private void ThrowIfNotStarted()
    {
        if (!HasStarted) {
            throw new InvalidOperationException("The process has not been started yet");
        }
    }

    private ProcessStartInfo CreateAdaptedStartInfo()
    {
        var startInfo = StartInfo.CreateProcessStartInfo();
        startInfo.UseShellExecute = false;

        if (OutputWriter is not null) {
            startInfo.RedirectStandardOutput = true;
        }

        if (ErrorWriter is not null) {
            startInfo.RedirectStandardError = true;
        }

        startInfo.CreateNoWindow = true;
        return startInfo;
    }

    [MemberNotNull(nameof(_process))]
    private void CreateProcess(out Process process)
    {
        var currentProcess = _process;

        if (currentProcess is not null) {
            process = currentProcess;
            return;
        }

        var newProcess = new Process { StartInfo = CreateAdaptedStartInfo() };

        if (Interlocked.CompareExchange(ref _process, newProcess, null) == null) {
            process = newProcess;
            return;
        }

        // Someone else was faster, so we dispose new process
        newProcess.Dispose();
        process = currentProcess!;
    }

    private void AnnounceProcessExit()
    {
        lock (_processEndingLock) {
            if (IsExited) {
                return;
            }

            IsExited = true;

            if (IsDisposed) {
                return;
            }

            _processExitedTokenSource.TryCancel(CancellationErrorDiscardHandling.ObjectDisposed);
        }
    }

    [MemberNotNull(nameof(_readOutputTask), nameof(_readErrorTask))]
    private void StartProcess(Process process)
    {
        // Try non-lock version
        if (HasStarted) {
            return;
        }

        lock (_startProcessLock) {
            if (HasStarted) {
                return;
            }

            process.EnableRaisingEvents = true;

            void OnProcessExited(object? sender, EventArgs e) =>
                AnnounceProcessExit();

            process.Exited += OnProcessExited;

            if (!process.Start()) {
                throw new ProcessReuseException("Process reuse is not supported");
            }

            Id = process.Id;
            HasStarted = true;
            _processStartedTokenSource.Cancel();

            /* REMINDER:
             * 1. Exiting token is hitting faster than stream can be read, so don't use it
             * 2. We must perform the read tasks in the thread pool synchronization context, otherwise we face deadlocks when synchronously waiting for them */

            _readOutputTask = OutputWriter is not null
                ? Task.Run(async () => await ReadStreamAsync(process.StandardOutput.BaseStream, OutputWriter, Cancelled).ConfigureAwait(false))
                : Task.CompletedTask;

            _readErrorTask = ErrorWriter is not null
                ? Task.Run(async () => await ReadStreamAsync(process.StandardError.BaseStream, ErrorWriter, Cancelled).ConfigureAwait(false))
                : Task.CompletedTask;
        }
    }

    [MemberNotNull(nameof(_process), nameof(_readOutputTask), nameof(_readErrorTask))]
    private void Run(out Process process)
    {
        CreateProcess(out process);
        StartProcess(process);
    }

    /// <summary>
    /// Runs the process.
    /// </summary>
    [MemberNotNull(nameof(_process))]
    public void Run() =>
        Run(out _);

    IProcessExecution IRunnable<IProcessExecution>.Run()
    {
        Run();
        return this;
    }

    IAsyncProcessExecution IRunnable<IAsyncProcessExecution>.Run()
    {
        Run();
        return this;
    }

    /// <inheritdoc/>
    private void Cancel(bool keepCancellationFromThrowing)
    {
        if (IsDisposed) {
            return;
        }

        lock (_processEndingLock) {
            if (IsCompleted) {
                return;
            }

            if (keepCancellationFromThrowing) {
                _processCancellationTokenSource.TryCancel();
            } else {
                _processCancellationTokenSource.Cancel();
            }
        }
    }

    /// <inheritdoc/>
    public void Cancel() =>
        Cancel(keepCancellationFromThrowing: false);

    private async Task<int> RunToCompletionAsync(
        bool synchronously,
        CancellationToken cancellationToken,
        ProcessCompletionOptions completionOptions)
    {
        var waitForExitOnly = completionOptions.HasFlag(ProcessCompletionOptions.WaitForExit);
        Run(out var process);

        CancellationTokenSource CreateCancellationTokenSource(out CancellationToken newCancellationToken)
        {
            var tokens = completionOptions.HasFlag(ProcessCompletionOptions.WaitForExit)
                ? new CancellationToken[] { cancellationToken, }
                : new CancellationToken[] { cancellationToken, Cancelled };

            var tokenSource = tokens.Length == 0
                ? new CancellationTokenSource()
                : CancellationTokenSource.CreateLinkedTokenSource(tokens);

            newCancellationToken = tokenSource.Token;
            return tokenSource;
        }

        using var cancellationTokenSource = CreateCancellationTokenSource(out var newCancellationToken);
        var recordedException = default(Exception);

        if (synchronously) {
            var promiseOfWaitForExit = Task.Run(async () => await process.WaitForExitAsync(newCancellationToken).ConfigureAwait(false));

            try {
                promiseOfWaitForExit.GetAwaiter().GetResult();
            } catch (Exception error) {
                recordedException = error;
            }
        } else {
            try {
                await process.WaitForExitAsync(newCancellationToken).ConfigureAwait(false);
            } catch (Exception error) {
                recordedException = error;
            }
        }

        // If we are not only waiting for exit, then wait for reading tasks if
        // 1. no exception was thrown, and we expect reading tasks to finish, or
        // 2. the process cancelled and reading tasks are cancelling right now.
        if (!waitForExitOnly && (recordedException == null || IsCancelled)) {
            async ValueTask WaitForReadingTasks()
            {
                if (_readOutputTask.IsCompleted && _readErrorTask.IsCompleted) {
                    return;
                }

                try {
                    if (synchronously) {
                        Task.WaitAll(_readOutputTask, _readErrorTask);
                    } else {
                        await Task.WhenAll(_readOutputTask, _readErrorTask).ConfigureAwait(false);
                    }
                } catch {
                    // Reading tasks should not participate into an exception occurence
                }
            }

            if (synchronously) {
                WaitForReadingTasks().GetAwaiter().GetResult();
            } else {
                await WaitForReadingTasks().ConfigureAwait(false);
            }
        }

        if (recordedException != null) {
            if (recordedException is OperationCanceledException && newCancellationToken.IsCancellationRequested) {
                if (completionOptions.HasFlag(ProcessCompletionOptions.KillTreeOnCancellation)) {
                    KillProcess(entireProcessTree: true, keepKillingFromThrowing: true);
                } else if (completionOptions.HasFlag(ProcessCompletionOptions.KillOnCancellation)) {
                    KillProcess(entireProcessTree: false, keepKillingFromThrowing: true);
                }
            }

            ExceptionDispatchInfo.Capture(recordedException).Throw();
        }

        /* The process exited successfully */
        _exitCode ??= process.ExitCode;
        AnnounceProcessExit();
        return _exitCode.Value;
    }

    /// <inheritdoc/>
    public int RunToCompletion(CancellationToken cancellationToken, ProcessCompletionOptions completionOptions) =>
        RunToCompletionAsync(synchronously: true, cancellationToken, completionOptions).GetAwaiter().GetResult();

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(ICompletableProcess, CancellationToken)"/>
    public int RunToCompletion(CancellationToken cancellationToken) =>
        RunToCompletion(cancellationToken, ProcessCompletionOptions.None);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(ICompletableProcess, CancellationToken)"/>
    public int RunToCompletion() =>
        RunToCompletion(CancellationToken.None, ProcessCompletionOptions.None);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(ICompletableProcess, ProcessCompletionOptions)"/>
    public int RunToCompletion(ProcessCompletionOptions completionOptions) =>
        RunToCompletion(CancellationToken.None, completionOptions);

    /// <inheritdoc/>
    public Task<int> RunToCompletionAsync(CancellationToken cancellationToken, ProcessCompletionOptions completionOptions) =>
        RunToCompletionAsync(synchronously: false, cancellationToken, completionOptions);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletionAsync(IAsyncCompletableProcess, CancellationToken)"/>
    public Task<int> RunToCompletionAsync(CancellationToken cancellationToken) =>
        RunToCompletionAsync(cancellationToken, ProcessCompletionOptions.None);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletionAsync(IAsyncCompletableProcess, CancellationToken)"/>
    public Task<int> RunToCompletionAsync() =>
        RunToCompletionAsync(CancellationToken.None, ProcessCompletionOptions.None);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletionAsync(IAsyncCompletableProcess, ProcessCompletionOptions)"/>
    public Task<int> RunToCompletionAsync(ProcessCompletionOptions completionOptions) =>
        RunToCompletionAsync(CancellationToken.None, completionOptions);

    [MemberNotNull(nameof(_process))]
    private void CheckProcessStarted()
    {
        if (_process is null) {
            throw new InvalidOperationException("The process have not been started yet");
        }
    }

    private void KillProcess(bool entireProcessTree)
    {
        CheckProcessStarted();

        // ISSUE: This acts asynchronous, whereby KillTree acts synchronously
#if NET5_0_OR_GREATER
        _process.Kill(entireProcessTree);
#else
        // https://unix.stackexchange.com/a/124148
        _process.KillTree(s_defaultKillTreeTimeout);
#endif
    }

    private void KillProcess(bool entireProcessTree, bool keepKillingFromThrowing)
    {
        if (keepKillingFromThrowing) {
            try {
                KillProcess(entireProcessTree);
            } catch {
                // Ignore on purpose
            }
        } else {
            KillProcess(entireProcessTree);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The process have not been started yet.</exception>
    public void Kill(bool entireProcessTree) =>
        KillProcess(entireProcessTree);

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The process have not been started yet.</exception>
    public void Kill() =>
        Kill(entireProcessTree: false);

    private void Dispose(bool disposing)
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1) {
            return;
        }

        if (!disposing) {
            return;
        }

        _timer?.Dispose();

        // Cancels all running operations
        Cancel(keepCancellationFromThrowing: true);

        _process?.Dispose();
        _processStartedTokenSource.Dispose();
        _processCancellationTokenSource.Dispose();
        _processExitedTokenSource.Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
