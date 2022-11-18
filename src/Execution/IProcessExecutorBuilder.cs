using System.Text;

namespace Kenet.SimpleProcess.Execution;

public interface IProcessExecutorBuilder
{
    /// <summary>
    /// If used, the process exit code will be checked against <paramref name="exitCode" /> and throw
    /// <see cref="BadExitCodeException" /> on mismatch.
    /// </summary>
    /// <param name="exitCode"></param>
    IProcessExecutorBuilder WithExitCode(int exitCode);

    /// <summary>
    /// Allows the interpretation of the error stream. The interpreted error is only used in case of a bad exit code.
    /// </summary>
    /// <remarks>
    /// The encoding will be initially set or overwritten on successive calls. It is by default
    /// <see cref="Encoding.UTF8" />.
    /// </remarks>
    /// <param name="encoding"></param>
    IProcessExecutorBuilder WithErrorInterpretation(Encoding? encoding = null);

    /// <summary>
    /// Adds a writer for the error stream.
    /// </summary>
    /// <param name="writer"></param>
    IProcessExecutorBuilder AddErrorWriter(WriteHandler writer);

    /// <summary>
    /// Adds a writer for the output stream.
    /// </summary>
    /// <param name="writer"></param>
    IProcessExecutorBuilder AddOutputWriter(WriteHandler writer);

    /// <summary>
    /// Adds another cancellation token.
    /// </summary>
    /// <param name="cancellationTokens"></param>
    IProcessExecutorBuilder AddCancellation(IEnumerable<CancellationToken> cancellationTokens);

    /// <summary>
    /// Builds the artifact of current builder.
    /// </summary>
    /// <returns></returns>
    IProcessExecutorBuilderArtifact BuildArtifact();
}
