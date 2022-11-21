using System.Text;

namespace Kenet.SimpleProcess;

public interface IProcessExecutorArtifact
{
    SimpleProcessStartInfo StartInfo { get; }
    IReadOnlyCollection<CancellationToken> CancellationTokens { get; }
    IReadOnlyCollection<WriteHandler> ErrorWriters { get; }
    IReadOnlyCollection<WriteHandler> OutputWriters { get; }
    Encoding? ExitErrorEncoding { get; }
    Func<int, bool>? ValidateExitCode { get; }
}
