﻿using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace Kenet.SimpleProcess;

/// <summary>
/// A simple process trying to replace the bootstrap code of a native instance of <see cref="Process"/>.
/// </summary>
public sealed class SimpleProcess :
    IProcessExecution,
    IAsyncProcessExecution,
    IRunnable<IProcessExecution>,
    IRunnable<IAsyncProcessExecution>
{
    private static readonly TimeSpan _defaultKillTreeTimeout = TimeSpan.FromSeconds(10);

    private static async Task ReadStreamAsync(
        Stream source,
        WriteHandler writeNextBytes,
        CancellationToken cancellationToken)
    {
        await Task.Yield(); // No ConfigureAwait(bool) available
        var cancellableTaskSource = new TaskCompletionSource<int>();
        var cancellableTask = cancellableTaskSource.Task;
        using var cancelTaskSource = cancellationToken.Register(() => cancellableTaskSource.SetException(new OperationCanceledException(cancellationToken)));
        Task<int>? lastWrittenBytesCountTask = null;
        using var memoryOwner = MemoryPool<byte>.Shared.Rent(1024 * 4);

        try {
            tryReadNext:

            if (cancellationToken.IsCancellationRequested) {
                return;
            }

            // ISSUE: https://github.com/dotnet/runtime/issues/28583
            lastWrittenBytesCountTask = await Task.WhenAny(
                    source.ReadAsync(memoryOwner.Memory, cancellationToken).AsTask(),
                    cancellableTask)
                .ConfigureAwait(false);

            // Call to GetAwaiter.GetResult() unwraps exception, if any
            var lastWrittenBytesCount = lastWrittenBytesCountTask.GetAwaiter().GetResult();

            if (lastWrittenBytesCount != 0) {
                writeNextBytes(memoryOwner.Memory.Span[..lastWrittenBytesCount]);
                goto tryReadNext;
            }
        } catch (OperationCanceledException error) when (
            error.CancellationToken.Equals(cancellationToken)) {
        } finally {
            writeNextBytes(EOF.Memory.Span);
        }
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
    public bool IsExited { get; private set; }

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

    private ProcessStartInfo CreateAdaptiveStartInfo()
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

        var newProcess = new Process { StartInfo = CreateAdaptiveStartInfo() };

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

            _processExitedTokenSource.TryCancel();
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
    public void Cancel()
    {
        if (IsDisposed) {
            return;
        }

        lock (_processEndingLock) {
            if (IsCancelled) {
                return;
            }

            _processCancellationTokenSource.Cancel();
        }
    }

    /// <inheritdoc/>
    public void CancelAfter(int delayInMilliseconds)
    {
        if (IsDisposed) {
            return;
        }

        lock (_processEndingLock) {
            if (IsCancelled) {
                return;
            }

            _processCancellationTokenSource.CancelAfter(delayInMilliseconds);
        }
    }

    /// <inheritdoc/>
    public void CancelAfter(TimeSpan delay)
    {
        if (IsDisposed) {
            return;
        }

        lock (_processEndingLock) {
            if (IsCancelled) {
                return;
            }

            _processCancellationTokenSource.CancelAfter(delay);
        }
    }

    private async Task<int> RunToCompletionAsync(
        bool synchronously,
        CancellationToken cancellationToken,
        ProcessCompletionOptions completionOptions)
    {
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

        try {
            /* We want to enable multiple calls to RunToCompletion[Async] e.g. to wait for exit after call to asynchronous Kill([bool]) */

            // 1. If the read task has been completed, we should not await it again, as it could be fauled or canceled.
            // [2. When the process has fallen into cancelling state, then on-going read should already have been canceled.]
            // 3. If the user only wants to wait for the process exit, we ignore reader tasks, because they may get faulted or canceled.
            Task PrepareReadTask(Task readTask) => readTask.IsCompleted || completionOptions.HasFlag(ProcessCompletionOptions.WaitForExit)
                ? Task.CompletedTask
                : readTask.ContinueWithCallbackOnFailure(cancellationTokenSource.Cancel);

            var readOutputTask = PrepareReadTask(_readOutputTask);
            var readErrorTask = PrepareReadTask(_readErrorTask);

            if (synchronously) {
                // This produces potentially a never ending task, but should end if the process exits/got killed or is going to be disposed.
                var waitForExitTask = Task.Run(process.WaitForExit, newCancellationToken);

                // This makes the assumption, that every await uses ConfigureAwait(continueOnCapturedContext: false)
                //
                // We must pass cancellation token, otherwise all tasks are awaited.
                var recordedException = default(Exception);

                try {
                    Task.WaitAll(
                        new[] {
                            waitForExitTask,
                            readOutputTask,
                            readErrorTask,
                        },
                        newCancellationToken);
                } catch (AggregateException error) {
                    // Let's unwrap the aggregated exception
                    recordedException = error.InnerException;
                } catch (Exception error) {
                    recordedException = error;
                }

                // When the cancellation was not requested from the method-scoped cancellation token, then we can assume:
                // 1. The process has fallen into cancelling state and the cancelled token turned cancellation requested.
                // 2. This instance disposed and the [disposed token -> cancelled token] turned cancelled requested.
                if (!cancellationToken.IsCancellationRequested) {
                    try {
                        Task.WaitAll(readOutputTask, readErrorTask);
                    } catch (AggregateException) {
                        ; // We ignore on purpose
                    }
                }

                if (recordedException != null) {
                    ExceptionDispatchInfo.Capture(recordedException).Throw();
                }
            } else {
                var cancellationTaskSource = new TaskCompletionSource<object>();
                using var cancelTaskSource = newCancellationToken.Register(() => cancellationTaskSource.SetException(new OperationCanceledException(newCancellationToken)));

                var whenAllTask = Task.WhenAll(
                    process.WaitForExitAsync(newCancellationToken),
                    readOutputTask,
                    readErrorTask);

                // See above
                var whenAnyTask = await Task.WhenAny(
                        whenAllTask,
                        cancellationTaskSource.Task)
                    .ConfigureAwait(false);

                // See above.
                if (!cancellationToken.IsCancellationRequested) {
                    // This will not throw in case of cancellation of any task
                    await Task.WhenAll(readOutputTask, readErrorTask).ConfigureAwait(false);
                }

                whenAnyTask.GetAwaiter().GetResult();
            }

            /* The process exited successfully */
            _exitCode ??= process.ExitCode;
            AnnounceProcessExit();
        } catch (Exception error) {
            if (completionOptions != ProcessCompletionOptions.None
                && error is OperationCanceledException
                && newCancellationToken.IsCancellationRequested) {
                if (completionOptions.HasFlag(ProcessCompletionOptions.KillTreeOnCancellation)) {
                    Kill(entireProcessTree: true);
                } else if (completionOptions.HasFlag(ProcessCompletionOptions.KillOnCancellation)) {
                    Kill();
                }
            }

            throw;
        }

        return _exitCode.Value;
    }

    /// <inheritdoc/>
    public int RunToCompletion(CancellationToken cancellationToken, ProcessCompletionOptions completionOptions) =>
        RunToCompletionAsync(synchronously: true, cancellationToken, completionOptions).GetAwaiter().GetResult();

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(ICompletableProcessExecution, CancellationToken)"/>
    public int RunToCompletion(CancellationToken cancellationToken) =>
        RunToCompletion(cancellationToken, ProcessCompletionOptions.None);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(ICompletableProcessExecution, CancellationToken)"/>
    public int RunToCompletion() =>
        RunToCompletion(CancellationToken.None, ProcessCompletionOptions.None);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(ICompletableProcessExecution, ProcessCompletionOptions)"/>
    public int RunToCompletion(ProcessCompletionOptions completionOptions) =>
        RunToCompletion(CancellationToken.None, completionOptions);

    /// <inheritdoc/>
    public Task<int> RunToCompletionAsync(CancellationToken cancellationToken, ProcessCompletionOptions completionOptions) =>
        RunToCompletionAsync(synchronously: false, cancellationToken, completionOptions);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletionAsync(IAsyncCompletableProcessExecution, CancellationToken)"/>
    public Task<int> RunToCompletionAsync(CancellationToken cancellationToken) =>
        RunToCompletionAsync(cancellationToken, ProcessCompletionOptions.None);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletionAsync(IAsyncCompletableProcessExecution, CancellationToken)"/>
    public Task<int> RunToCompletionAsync() =>
        RunToCompletionAsync(CancellationToken.None, ProcessCompletionOptions.None);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletionAsync(IAsyncCompletableProcessExecution, ProcessCompletionOptions)"/>
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

        // Cancels all running operations
        Cancel();

        _process?.Dispose();
        _processStartedTokenSource.Dispose();

        // It can happen, that someone is in Cancel()
        lock (_processEndingLock) {
            if (!IsExited)
                _processCancellationTokenSource.Dispose();
        }

        _processExitedTokenSource.Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}