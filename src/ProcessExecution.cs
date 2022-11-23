using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace Kenet.SimpleProcess;

/// <summary>
/// Represents 
/// </summary>
public sealed class ProcessExecution : IProcessExecution, IAsyncProcessExecution
{
    internal static ProcessExecution Create(IProcessExecutorArtifact artifact)
    {
        var errorBuffer = CreateErrorBuffer();
        var errorWriter = errorBuffer != null ? (WriteHandler)errorBuffer.Write : null;
        var cancellationTokenSource = CreateCancellationTokenSource(out var cancellationToken);

        try {
            var process = CreateProcess(errorWriter, cancellationToken);

            return new ProcessExecution(
                process,
                artifact.ValidateExitCode,
                errorBuffer,
                artifact.ExitErrorEncoding,
                cancellationTokenSource);
        } catch {
            errorBuffer?.Dispose();
            cancellationTokenSource?.Dispose();
            throw;
        }

        SimpleProcess CreateProcess(WriteHandler? additionalErrorWriter, CancellationToken cancellationToken)
        {
            var errorWriters = additionalErrorWriter != null
                ? new List<WriteHandler>(artifact.ErrorWriters) { additionalErrorWriter }
                : artifact.ErrorWriters;

            var errorWriter = errorWriters.Count != 0
                ? (WriteHandler)new CompositeWriter(errorWriters).Write
                : null;

            var outputWriter = artifact.OutputWriters.Count > 1
                ? new CompositeWriter(artifact.OutputWriters).Write
                : artifact.OutputWriters.SingleOrDefault();

            return new SimpleProcess(artifact.StartInfo, cancellationToken) {
                ErrorWriter = errorWriter,
                OutputWriter = outputWriter
            };
        }

        ArrayPoolBufferWriter<byte>? CreateErrorBuffer() =>
            artifact.ExitErrorEncoding != null || artifact.ErrorWriters.Count > 0
                ? new ArrayPoolBufferWriter<byte>()
                : null;

        CancellationTokenSource? CreateCancellationTokenSource(out CancellationToken cancellationToken)
        {
            var cancellationTokenSource = artifact.CancellationTokens.Count > 0
                ? CancellationTokenSource.CreateLinkedTokenSource(artifact.CancellationTokens.ToArray())
                : null;

            cancellationToken = cancellationTokenSource?.Token ?? CancellationToken.None;
            return cancellationTokenSource;
        }
    }

    /// <inheritdoc/>
    public CancellationToken Exited =>
        _process.Exited;

    /// <inheritdoc/>
    public bool IsExited =>
        _process.IsExited;

    private readonly SimpleProcess _process;
    private readonly Func<int, bool>? _validateExitCode;
    private readonly ArrayPoolBufferWriter<byte>? _errorBuffer;
    private readonly Encoding? _exitErrorEncoding;
    private readonly CancellationTokenSource? _cancellationTokenSource;
    private IDisposable? _disposeWhenProcessExited;
    private BadExitCodeException? _capturedException;
    private object _capturedExceptionLock;
    private int _isDisposed;

    private ProcessExecution(
        SimpleProcess process,
        Func<int, bool>? validateExitCode,
        ArrayPoolBufferWriter<byte>? errorBuffer,
        Encoding? exitErrorEncoding,
        CancellationTokenSource? cancellationTokenSource)
    {
        _capturedExceptionLock = new();
        _process = process;
        _validateExitCode = validateExitCode;
        _errorBuffer = errorBuffer;
        _exitErrorEncoding = exitErrorEncoding;
        _cancellationTokenSource = cancellationTokenSource;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsDisposed() => _isDisposed == 1;

    internal void Run() =>
         _process.Run();

    private void CheckExitCode(int exitCode)
    {
        if (_validateExitCode == null) {
            return;
        }

        if (_validateExitCode(exitCode)) {
            return;
        }

        var errorMessage = _errorBuffer != null
            ? (_exitErrorEncoding ?? Encoding.UTF8).GetString(_errorBuffer.WrittenMemory.GetArrayUnsafe(), 0, _errorBuffer.WrittenCount)
            : null;

        throw new BadExitCodeException(errorMessage) { ExitCode = exitCode };
    }

    private Exception GetCapturedOrFallbackError(Exception error)
    {
        void TryInvalidateCache()
        {
            if (IsDisposed()) {
                _capturedException = null;
            }
        }

        if (_capturedException != null) {
            TryInvalidateCache();
        } else {
            lock (_capturedExceptionLock) {
                if (_capturedException != null) {
                    TryInvalidateCache();
                    goto rethrow;
                }

                if (error is not BadExitCodeException badExitCodeError) {
                    goto rethrow;
                }

                _capturedException = badExitCodeError;
            }
        }

        rethrow:
        return _capturedException ?? error;
    }

    private void DisableDisposeOnExit(out bool reenable)
    {
        var previousDisposeOnExit = Interlocked.Exchange(ref _disposeWhenProcessExited, null);
        reenable = previousDisposeOnExit != null;
        ProcessBoundary.DisposeOrFailSilently(previousDisposeOnExit);
    }

    private void EnableDisposeOnExit()
    {
        if (_isDisposed == 1) {
            return;
        }

        if (IsExited) {
            Dispose();
            return;
        }

        var disposeWhenProcessExited = Exited.Register(Dispose);

        if (Interlocked.CompareExchange(ref _disposeWhenProcessExited, disposeWhenProcessExited, null) == null) {
            return; // Everything worked fine
        }

        // We had no luck being first, so we try to dispose
        ProcessBoundary.DisposeOrFailSilently(disposeWhenProcessExited);
    }

    /// <inheritdoc />
    public int RunToCompletion(CancellationToken cancellationToken, ProcessCompletionOptions completionOptions)
    {
        DisableDisposeOnExit(out var reenable);

        try {
            var exitCode = _process.RunToCompletion(cancellationToken, completionOptions);
            CheckExitCode(exitCode);

            if (!reenable && completionOptions.HasFlag(ProcessCompletionOptions.DisposeOnCompleted)) {
                EnableDisposeOnExit();
            }

            return exitCode;
        } catch (Exception error) {
            if (!reenable && completionOptions.HasFlag(ProcessCompletionOptions.DisposeOnFailure)) {
                EnableDisposeOnExit();
            }

            throw GetCapturedOrFallbackError(error);
        } finally {
            if (reenable) {
                EnableDisposeOnExit();
            }
        }
    }

    /// <inheritdoc />
    public async Task<int> RunToCompletionAsync(CancellationToken cancellationToken, ProcessCompletionOptions completionOptions)
    {
        DisableDisposeOnExit(out var reenable);

        try {
            var exitCode = await _process.RunToCompletionAsync(cancellationToken, completionOptions);
            CheckExitCode(exitCode);

            if (!reenable && completionOptions.HasFlag(ProcessCompletionOptions.DisposeOnCompleted)) {
                EnableDisposeOnExit();
            }

            return exitCode;
        } catch (Exception error) {
            if (!reenable && completionOptions.HasFlag(ProcessCompletionOptions.DisposeOnFailure)) {
                EnableDisposeOnExit();
            }

            throw GetCapturedOrFallbackError(error);
        } finally {
            if (reenable) {
                EnableDisposeOnExit();
            }
        }
    }

    /// <inheritdoc/>
    public void Kill() =>
        _process.Kill();

    /// <inheritdoc/>
    public void Kill(bool entireProcessTree) =>
        _process.Kill(entireProcessTree);

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1) {
            return;
        }

        _errorBuffer?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}
