using System.Text;

namespace Kenet.SimpleProcess.Execution;

internal class ProcessExecutorBuilderArtifact : IProcessExecutorBuilderArtifact
{
    public IReadOnlyList<CancellationToken> CancellationTokens { get; }
    public IReadOnlyList<WriteHandler> ErrorWriters { get; }
    public IReadOnlyList<WriteHandler> OutputWriters { get; }
    public SimpleProcessStartInfo StartInfo { get; }
    public Encoding? ErrorEncoding { get; }
    public int? ExpectedExitCode { get; }

    public ProcessExecutorBuilderArtifact(
        IReadOnlyList<CancellationToken> cancellationTokens,
        IReadOnlyList<WriteHandler> errorTracers,
        IReadOnlyList<WriteHandler> outputTracers,
        SimpleProcessStartInfo startInfo,
        Encoding? errorEncoding,
        int? expectedExitCode)
    {
        CancellationTokens = cancellationTokens ?? throw new ArgumentNullException(nameof(cancellationTokens));
        ErrorWriters = errorTracers ?? throw new ArgumentNullException(nameof(errorTracers));
        OutputWriters = outputTracers ?? throw new ArgumentNullException(nameof(outputTracers));
        StartInfo = startInfo ?? throw new ArgumentNullException(nameof(startInfo));
        ErrorEncoding = errorEncoding;
        ExpectedExitCode = expectedExitCode;
    }
}
