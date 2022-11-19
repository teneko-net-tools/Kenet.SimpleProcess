using System.Text;

namespace Kenet.SimpleProcess.Execution;

public interface IProcessExecutorBuilderArtifact
{
    IReadOnlyCollection<CancellationToken> CancellationTokens { get; }
    IReadOnlyCollection<WriteHandler> ErrorWriters { get; }
    IReadOnlyCollection<WriteHandler> OutputWriters { get; }
    SimpleProcessStartInfo StartInfo { get; }
    Encoding? ErrorEncoding { get; }
    int? ExpectedExitCode { get; }
}
