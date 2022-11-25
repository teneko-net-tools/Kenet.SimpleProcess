using System.Text;

namespace Kenet.SimpleProcess;

/// <summary>
/// Represents an immutable artifact with the most basic features to create e.g. an instance of <see cref="ProcessExecutor"/>.
/// </summary>
public interface IProcessExecutorArtifact
{
    /// <summary>
    /// The basic process start info.
    /// </summary>
    SimpleProcessStartInfo StartInfo { get; }

    /// <summary>
    /// A list of cancelling state tokens, that may cause the process to fall into the cancelling state.
    /// </summary>
    IReadOnlyCollection<CancellationToken> CancellationTokens { get; }

    /// <summary>
    /// A list of error writers where incoming bytes are written to.
    /// </summary>
    IReadOnlyCollection<WriteHandler> ErrorWriters { get; }

    /// <summary>
    /// A list of error writers where incoming bytes are written to.
    /// </summary>
    IReadOnlyCollection<WriteHandler> OutputWriters { get; }

    /// <summary>
    /// In case of a bad exit code, the encoding is used to interpret the incoming error bytes.
    /// </summary>
    Encoding? ExitErrorEncoding { get; }

    /// <summary>
    /// The delegate is used to validate the exit code.
    /// </summary>
    Func<int, bool>? ValidateExitCode { get; }
}
