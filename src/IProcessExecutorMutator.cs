using System.Text;

namespace Kenet.SimpleProcess;

/// <summary>
/// Enables you to design a process executor with the most basic features.
/// </summary>
public interface IProcessExecutorMutator
{
    /// <summary>
    /// If used, the process exit code will be checked against <paramref name="validator" /> and throw
    /// <see cref="BadExitCodeException" /> on mismatch.
    /// </summary>
    /// <param name="validator"></param>
    void WithExitCode(Func<int, bool> validator);

    /// <summary>
    /// Allows the interpretation of the error stream. The interpreted error is only used in case of a bad exit code.
    /// </summary>
    /// <remarks>
    /// The encoding will be initially set or overwritten on successive calls. It is by default <see cref="Encoding.UTF8" />.
    /// </remarks>
    /// <param name="encoding"></param>
    void WithErrorInterpretation(Encoding? encoding);

    /// <summary>
    /// Adds a writer for the error stream.
    /// </summary>
    /// <param name="writer"></param>
    void AddErrorWriter(WriteHandler writer);

    /// <summary>
    /// Adds a writer for the output stream.
    /// </summary>
    /// <param name="writer"></param>
    void AddOutputWriter(WriteHandler writer);

    /// <summary>
    /// Adds cancellation tokens, that may cause the process to fall into the cancelling state.
    /// </summary>
    /// <param name="cancellationTokens">
    /// Cancellation tokens, that may cause the process to fall into the cancelling state.
    /// </param>
    void AddCancellation(IEnumerable<CancellationToken> cancellationTokens);
}
