using System.Text;

namespace Kenet.SimpleProcess.Execution;

internal class ProcessExecutorBuilderArtifact : IProcessExecutorBuilderArtifact
{
    public IReadOnlyCollection<CancellationToken> CancellationTokens { get; }
    public IReadOnlyCollection<WriteHandler> ErrorWriters { get; }
    public IReadOnlyCollection<WriteHandler> OutputWriters { get; }
    public SimpleProcessStartInfo StartInfo { get; }
    public Encoding? ErrorEncoding { get; }
    public int? ExpectedExitCode { get; }

    public ProcessExecutorBuilderArtifact(
        IReadOnlyCollection<CancellationToken> cancellationTokens,
        IReadOnlyCollection<WriteHandler> errorTracers,
        IReadOnlyCollection<WriteHandler> outputTracers,
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
