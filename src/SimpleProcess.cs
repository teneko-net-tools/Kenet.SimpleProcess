using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Kenet.SimpleProcess;

/// <summary>
/// A simple process trying to replace the bootstrap code of a native instance of <see cref="Process"/>.
/// </summary>
public sealed class SimpleProcess :
    IProcessExecution,
    IAsyncProcessExecution,
    IRunnable<IProcessExecution>,
    IRunnable<IContextlessProcessExecution>,
    IRunnable<IAsyncProcessExecution>,
    IRunnable<IAsyncContextlessProcessExecution>
{
    private static readonly TimeSpan _defaultKillTreeTimeout = TimeSpan.FromSeconds(10);

    private static async Task ReadStreamAsync(
        Stream source,
        WriteHandler writeNextBytes,
        CancellationToken cancellationToken)
    {
        try {
            tryReadNext:

            if (cancellationToken.IsCancellationRequested) {
                WriteEOF();
                return;
            }

            {
                using var memoryOwner = MemoryPool<byte>.Shared.Rent(1024 * 4);
                var lastWrittenBytesCount = await source.ReadAsync(memoryOwner.Memory, cancellationToken).ConfigureAwait(false);

                if (lastWrittenBytesCount != 0) {
                    writeNextBytes(memoryOwner.Memory.Span[..lastWrittenBytesCount]);
                    goto tryReadNext;
                }

                WriteEOF();
            }
        } catch (OperationCanceledException error) when (error.CancellationToken.Equals(cancellationToken)) {
            WriteEOF(); // We canceled ReadAsync ourselves, so we safely can write EOF
        }

        void WriteEOF() => writeNextBytes(ReadOnlySpan<byte>.Empty);
    }

    public SimpleProcessStartInfo StartInfo { get; }

    /// <summary>
    /// The buffer the process will write incoming error to.
    /// </summary>
    public WriteHandler? ErrorWriter { get; init; }

    /// <summary>
    /// The buffer the process will write incoming output to.
    /// </summary>
    public WriteHandler? OutputWriter { get; init; }

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(_readOutputTask), nameof(_readErrorTask))]
    public bool IsProcessStarted { get; private set; }

    /// <inheritdoc/>
    public CancellationToken Exited { get; }

    /// <inheritdoc/>
    public bool IsExited { get; private set; }

    private readonly CancellationToken _userCancellationToken;
    private Task? _readOutputTask;
    private Task? _readErrorTask;

    //private readonly IDisposable? _killWhenUserCancellationRequested;
    private readonly CancellationTokenSource _processExitedSource;
    private readonly object _startProcessLock = new();
    private Process? _process;
    private int _exitCode;
    private int _isDisposed;

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    /// <param name="startInfo"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public SimpleProcess(SimpleProcessStartInfo startInfo, CancellationToken cancellationToken)
    {
        _userCancellationToken = cancellationToken;
        StartInfo = startInfo ?? throw new ArgumentNullException(nameof(startInfo));

        if (_userCancellationToken.CanBeCanceled) {
            // ISSUE: User cancellation token leads to cancellation of _processExitedSource but
            // how should the process been treated (before/after run)? Kill?
            _processExitedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            // TODO: Make user cancellation token useful
            //_killWhenUserCancellationRequested = cancellationToken.Register(() => Kill(entireProcessTree: true));
        } else {
            _processExitedSource = new CancellationTokenSource();
        }

        Exited = _processExitedSource.Token;
    }

    public SimpleProcess(SimpleProcessStartInfo startInfo)
        : this(startInfo, CancellationToken.None)
    {
    }

    internal void CreateProcess(out Process process)
    {
        var currentProcess = _process;

        if (currentProcess is not null) {
            process = currentProcess;
            return;
        }

        var newProcess = new Process { StartInfo = StartInfo.CreateProcessStartInfo() };

        if (Interlocked.CompareExchange(ref _process, newProcess, null) == null) {
            process = newProcess;
            return;
        }

        // Someone else was faster, so we dispose new process
        newProcess.Dispose();
        process = currentProcess!;
    }

    [MemberNotNull(nameof(_readOutputTask), nameof(_readErrorTask))]
    private void StartProcess(Process process)
    {
        // Try non-lock version
        if (IsProcessStarted) {
            return;
        }

        lock (_startProcessLock) {
            if (IsProcessStarted) {
                return;
            }

            process.EnableRaisingEvents = true;

            void OnProcessExited(object? sender, EventArgs e)
            {
                process.Exited -= OnProcessExited;
                IsExited = true;
                _exitCode = process.ExitCode;
                _processExitedSource.Cancel();
            }

            process.Exited += OnProcessExited;

            if (!process.Start()) {
                throw new ProcessReuseException("Process reuse is not supported");
            }

            IsProcessStarted = true;

            _readOutputTask = OutputWriter is not null
                ? ReadStreamAsync(process.StandardOutput.BaseStream, OutputWriter, Exited)
                : Task.CompletedTask;

            _readErrorTask = ErrorWriter is not null
                ? ReadStreamAsync(process.StandardError.BaseStream, ErrorWriter, Exited)
                : Task.CompletedTask;
        }
    }

    [MemberNotNull(nameof(_readOutputTask), nameof(_readErrorTask))]
    private void Run(out Process process)
    {
        CreateProcess(out process);
        StartProcess(process);
    }

    /// <summary>
    /// Runs the process.
    /// </summary>
    public void Run() =>
        Run(out _);

    IProcessExecution IRunnable<IProcessExecution>.Run()
    {
        Run();
        return this;
    }

    IContextlessProcessExecution IRunnable<IContextlessProcessExecution>.Run()
    {
        Run();
        return this;
    }

    IAsyncProcessExecution IRunnable<IAsyncProcessExecution>.Run()
    {
        Run();
        return this;
    }

    IAsyncContextlessProcessExecution IRunnable<IAsyncContextlessProcessExecution>.Run()
    {
        Run();
        return this;
    }

    private CancellationTokenSource? CreateUserCancellationTokenSource(CancellationToken additionalCancellationToken, out CancellationToken cancellationToken)
    {
        if (!additionalCancellationToken.CanBeCanceled) {
            cancellationToken = _userCancellationToken;
            return null;
        }

        var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_userCancellationToken, additionalCancellationToken);
        cancellationToken = cancellationTokenSource.Token;
        return cancellationTokenSource;
    }

    private static bool CancellationTokenIsAssociatedWithError(Exception error, CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled) {
            return false;
        }

        if (error is not OperationCanceledException operationCanceledError) {
            return false;
        }

        var isTokenWithErrorAssociated = false;
        ProcessBoundary.DisposeOrFailSilently(cancellationToken.Register(() => isTokenWithErrorAssociated = true));
        return isTokenWithErrorAssociated;
    }

    private void HandleRunToCompletionSuccess(ProcessCompletionOptions completionOptions)
    {
        if (completionOptions.HasFlag(ProcessCompletionOptions.DisposeOnCompleted)) {
            Dispose();
        }
    }

    private void HandleRunToCompletionFailure(
        ref Exception error,
        CancellationToken methodScopedCancellationToken,
        ProcessCompletionOptions completionOptions)
    {
        if (error is AggregateException aggregateError && aggregateError.InnerExceptions.Count == 1) {
            error = aggregateError.InnerExceptions[0];
        }

        if (completionOptions != ProcessCompletionOptions.None && CancellationTokenIsAssociatedWithError(error, methodScopedCancellationToken)) {
            if (completionOptions.HasFlag(ProcessCompletionOptions.KillTreeOnCancellationRequested)) {
                Kill(entireProcessTree: true);
            } else if (completionOptions.HasFlag(ProcessCompletionOptions.KillOnCancellationRequested)) {
                Kill();
            }
        }

        if (completionOptions.HasFlag(ProcessCompletionOptions.DisposeOnFailure)) {
            Exited.Register(Dispose);
        }
    }

    /// <inheritdoc/>
    public int RunToCompletion(CancellationToken cancellationToken, ProcessCompletionOptions completionOptions)
    {
        Run(out var process);
        var userCancellationTokenSource = CreateUserCancellationTokenSource(cancellationToken, out var userCancellationToken);

        try {
            // This produces potentially a never ending task, but should
            // end if the process exited.
            var waitForExitTask = Task.Run(process.WaitForExit, userCancellationToken);

            // This makes the assumption, that every await uses
            // ConfigureAwait(continueOnCapturedContext: false)
            Task.WaitAll(new[] { waitForExitTask, _readErrorTask, _readErrorTask }, userCancellationToken);
            HandleRunToCompletionSuccess(completionOptions);
        } catch (Exception error) {
            HandleRunToCompletionFailure(ref error, cancellationToken, completionOptions);
            throw;
        } finally {
            userCancellationTokenSource?.Dispose();
        }

        // REMINDER: Do not kill the process if user has requested the
        // cancellation, because this is not scope of this method
        return _exitCode;
    }

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(IContextlessProcessExecution, CancellationToken)"/>
    public int RunToCompletion(CancellationToken cancellationToken = default) =>
        RunToCompletion(cancellationToken, ProcessCompletionOptions.None);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(IContextlessProcessExecution, ProcessCompletionOptions)"/>
    public int RunToCompletion(ProcessCompletionOptions completionOptions) =>
        RunToCompletion(CancellationToken.None, completionOptions);

    /// <inheritdoc/>
    public async Task<int> RunToCompletionAsync(CancellationToken cancellationToken, ProcessCompletionOptions completionOptions)
    {
        Run(out var process);
        var userCancellationTokenSource = CreateUserCancellationTokenSource(cancellationToken, out var userCancellationToken);

        try {
            await Task.WhenAll(
                    process.WaitForExitAsync(userCancellationToken),
                    _readOutputTask,
                    _readErrorTask)
                .ConfigureAwait(false);

            HandleRunToCompletionSuccess(completionOptions);
        } catch (Exception error) {
            HandleRunToCompletionFailure(ref error, cancellationToken, completionOptions);
            throw;
        } finally {
            userCancellationTokenSource?.Dispose();
        }

        return _exitCode;
    }

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletionAsync(IAsyncContextlessProcessExecution, CancellationToken)"/>
    public Task<int> RunToCompletionAsync(CancellationToken cancellationToken = default) =>
        RunToCompletionAsync(cancellationToken, ProcessCompletionOptions.None);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletionAsync(IAsyncContextlessProcessExecution, ProcessCompletionOptions)"/>
    public Task<int> RunToCompletionAsync(ProcessCompletionOptions completionOptions) =>
        RunToCompletionAsync(CancellationToken.None, completionOptions);

    [MemberNotNull(nameof(_process))]
    private void CheckProcessStarted()
    {
        if (_process is null) {
            throw new InvalidOperationException("The process have not been started yet");
        }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The process have not been started yet.</exception>
    public void Kill()
    {
        CheckProcessStarted();
        _process.Kill();
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The process have not been started yet.</exception>
    public void Kill(bool entireProcessTree)
    {
        CheckProcessStarted();
#if NET5_0_OR_GREATER
        // ISSUE: This acts asynchronous, whereby KillTree acts synchronously
        _process.Kill(entireProcessTree);
#else
        // https://unix.stackexchange.com/a/124148
        _process.KillTree(_defaultKillTreeTimeout);
#endif
    }

    private void Dispose(bool disposing)
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1) {
            return;
        }

        if (!disposing) {
            return;
        }

        _process?.Dispose();
        _processExitedSource.Dispose();
        //_killWhenUserCancellationRequested?.Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
