using System.Buffers;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;

namespace Kenet.SimpleProcess.Execution;

internal class ProcessExecutor : IProcessExecutor, IAsyncProcessExecutor
{
    private readonly IProcessExecutorBuilderArtifact _artifact;

    public ProcessExecutor(IProcessExecutorBuilderArtifact artifact) =>
        _artifact = artifact ?? throw new ArgumentNullException(nameof(artifact));

    private SimpleProcess CreateProcess(
        WriteHandler? additionalErrorWriter,
        CancellationToken cancellationToken)
    {
        var errorWriters = additionalErrorWriter != null
            ? new List<WriteHandler>(_artifact.ErrorWriters) { additionalErrorWriter }
            : _artifact.ErrorWriters;

        var errorWriter = errorWriters.Count != 0
            ? (WriteHandler)new CompositeWriter(errorWriters).Write
            : null;

        var outputWriter = _artifact.OutputWriters.Count > 1
            ? new CompositeWriter(_artifact.OutputWriters).Write
            : _artifact.OutputWriters.SingleOrDefault();

        return new SimpleProcess(_artifact.StartInfo, outputWriter, errorWriter, cancellationToken);
    }

    private ArrayPoolBufferWriter<byte>? CreateErrorBuffer() =>
        _artifact.ErrorEncoding != null || _artifact.ErrorWriters.Count > 0
            ? new ArrayPoolBufferWriter<byte>()
            : null;

    private CancellationTokenSource? CreateCancellationTokenSource(
        CancellationToken additionalCancellationToken,
        out CancellationToken cancellationToken)
    {
        var cancellationTokenSource = _artifact.CancellationTokens.Count > 0
            ? CancellationTokenSource.CreateLinkedTokenSource(_artifact.CancellationTokens
                .Append(additionalCancellationToken).ToArray())
            : null;

        cancellationToken = cancellationTokenSource?.Token ?? additionalCancellationToken;
        return cancellationTokenSource;
    }

    private void CheckExitCode(int exitCode, IBuffer<byte>? errorBuffer)
    {
        if (!_artifact.ExpectedExitCode.HasValue) {
            return;
        }

        if (_artifact.ExpectedExitCode == exitCode) {
            return;
        }

        var errorMessage = errorBuffer != null
            ? (_artifact.ErrorEncoding ?? Encoding.UTF8).GetString(errorBuffer.WrittenMemory.GetArrayUnsafe())
            : null;

        throw new BadExitCodeException(errorMessage) { ExitCode = exitCode };
    }

    /// <inheritdoc />
    public async Task<ProcessExecutionResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        var errorBuffer = CreateErrorBuffer();
        var errorWriter = errorBuffer != null ? (WriteHandler)errorBuffer.Write : null;
        var cancellationTokenSource = CreateCancellationTokenSource(cancellationToken, out cancellationToken);

        try {
            using var process = CreateProcess(errorWriter, cancellationToken);
            var exitCode = await process.WaitForExitAsync();
            CheckExitCode(exitCode, errorBuffer);
            return new ProcessExecutionResult(process, exitCode);
        } finally {
            errorBuffer?.Dispose();
            cancellationTokenSource?.Dispose();
        }
    }

    /// <inheritdoc />
    public ProcessExecutionResult Execute(CancellationToken cancellationToken)
    {
        var errorBuffer = CreateErrorBuffer();
        var errorWriter = errorBuffer != null ? (WriteHandler)errorBuffer.Write : null;
        var cancellationTokenSource = CreateCancellationTokenSource(cancellationToken, out cancellationToken);

        try {
            using var process = CreateProcess(errorWriter, cancellationToken);
            var exitCode = process.WaitForExit();
            CheckExitCode(exitCode, errorBuffer);
            return new ProcessExecutionResult(process, exitCode);
        } finally {
            errorBuffer?.Dispose();
            cancellationTokenSource?.Dispose();
        }
    }
}
