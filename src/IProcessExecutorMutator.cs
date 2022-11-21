﻿using System.Text;

namespace Kenet.SimpleProcess;

public interface IProcessExecutorMutator
{
    /// <summary>
    /// If used, the process exit code will be checked against <paramref name="validExitCode" /> and throw
    /// <see cref="BadExitCodeException" /> on mismatch.
    /// </summary>
    /// <param name="validExitCode"></param>
    IProcessExecutorMutator WithExitCode(Func<int, bool> validate);

    /// <summary>
    /// Allows the interpretation of the error stream. The interpreted error is only used in case of a bad exit code.
    /// </summary>
    /// <remarks>
    /// The encoding will be initially set or overwritten on successive calls. It is by default <see cref="Encoding.UTF8" />.
    /// </remarks>
    /// <param name="encoding"></param>
    IProcessExecutorMutator WithErrorInterpretation(Encoding? encoding);

    /// <summary>
    /// Adds a writer for the error stream.
    /// </summary>
    /// <param name="writer"></param>
    IProcessExecutorMutator AddErrorWriter(WriteHandler writer);

    /// <summary>
    /// Adds a writer for the output stream.
    /// </summary>
    /// <param name="writer"></param>
    IProcessExecutorMutator AddOutputWriter(WriteHandler writer);

    /// <summary>
    /// Adds another cancellation token.
    /// </summary>
    /// <param name="cancellationTokens"></param>
    IProcessExecutorMutator AddCancellation(IEnumerable<CancellationToken> cancellationTokens);
}