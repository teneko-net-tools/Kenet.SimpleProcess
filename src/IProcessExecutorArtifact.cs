using System.Text;

namespace Kenet.SimpleProcess;

public interface IProcessExecutorArtifact
{
    /// <summary>
    /// The basic process start info.
    /// </summary>
    SimpleProcessStartInfo StartInfo { get; }
    /// <summary>
    /// A list of cancellation tokens, that lead to
    /// </summary>
    IReadOnlyCollection<CancellationToken> CancellationTokens { get; }
    IReadOnlyCollection<WriteHandler> ErrorWriters { get; }
    IReadOnlyCollection<WriteHandler> OutputWriters { get; }
    Encoding? ExitErrorEncoding { get; }
    Func<int, bool>? ValidateExitCode { get; }
}
