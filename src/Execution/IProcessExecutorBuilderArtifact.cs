using System.Text;

namespace Kenet.SimpleProcess.Execution;

public interface IProcessExecutorBuilderArtifact
{
    IReadOnlyList<CancellationToken> CancellationTokens { get; }
    IReadOnlyList<WriteHandler> ErrorWriters { get; }
    IReadOnlyList<WriteHandler> OutputWriters { get; }
    SimpleProcessStartInfo StartInfo { get; }
    Encoding? ErrorEncoding { get; }
    int? ExpectedExitCode { get; }
}
