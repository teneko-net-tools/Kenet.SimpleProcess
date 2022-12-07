using System.Buffers;
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
    public CancellationToken Started =>
        _process.Started;

    /// <inheritdoc/>
    public bool HasStarted =>
        _process.HasStarted;

    /// <inheritdoc/>
    public CancellationToken Exited =>
        _process.Exited;

    /// <inheritdoc/>
    public bool IsCancelled =>
        _process.IsCancelled;

    /// <inheritdoc/>
    public bool IsExited =>
        _process.IsExited;

    /// <inheritdoc/>
    public CancellationToken Cancelled =>
        _process.Cancelled;

    /// <inheritdoc/>
    public bool IsDisposed =>
        _process.IsDisposed;

    /// <inheritdoc/>
    public int Id => ((IExecutingProcess)_process).Id;

    int? IProcessInfo.Id => _process.Id;

    private readonly SimpleProcess _process;
    private readonly Func<int, bool>? _validateExitCode;
    private readonly ArrayPoolBufferWriter<byte>? _errorBuffer;
    private readonly Encoding? _exitErrorEncoding;
    private readonly CancellationTokenSource? _cancellationTokenSource;
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

    internal void Run() =>
         _process.Run();

    /// <inheritdoc/>
    public void Cancel() =>
        _process.Cancel();

    /// <inheritdoc/>
    public void CancelAfter(int delayInMilliseconds) =>
        _process.CancelAfter(delayInMilliseconds);

    /// <inheritdoc/>
    public void CancelAfter(TimeSpan delay) =>
        _process.CancelAfter(delay);

    private void CheckExitCode(int exitCode)
    {
        if (_validateExitCode == null) {
            return;
        }

        if (_validateExitCode(exitCode)) {
            return;
        }

        string? errorMessage = null;

        if (_errorBuffer != null) {
            var encoding = _exitErrorEncoding ?? Encoding.UTF8;
            errorMessage = encoding.GetString(_errorBuffer.WrittenMemory.Span, _errorBuffer.WrittenCount);
        }

        throw new BadExitCodeException(errorMessage) { ExitCode = exitCode };
    }

    private Exception GetCapturedOrFallbackError(Exception error)
    {
        void TryInvalidateCache()
        {
            if (IsDisposed) {
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

    private async Task<int> RunToCompletionAsync(bool synchronous, CancellationToken cancellationToken, ProcessCompletionOptions completionOptions)
    {
        try {
            int exitCode;

            if (synchronous) {
                exitCode = _process.RunToCompletion(cancellationToken, completionOptions);
            } else {
                exitCode = await _process.RunToCompletionAsync(cancellationToken, completionOptions).ConfigureAwait(false);
            }

            CheckExitCode(exitCode);
            return exitCode;
        } catch (Exception error) {
            throw GetCapturedOrFallbackError(error);
        }
    }

    /// <inheritdoc />
    public int RunToCompletion(CancellationToken cancellationToken, ProcessCompletionOptions completionOptions) =>
        RunToCompletionAsync(synchronous: true, cancellationToken, completionOptions).GetAwaiter().GetResult();

    /// <inheritdoc />
    public Task<int> RunToCompletionAsync(CancellationToken cancellationToken, ProcessCompletionOptions completionOptions) =>
        RunToCompletionAsync(synchronous: false, cancellationToken, completionOptions);

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

        _process.Dispose();
        _errorBuffer?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}
