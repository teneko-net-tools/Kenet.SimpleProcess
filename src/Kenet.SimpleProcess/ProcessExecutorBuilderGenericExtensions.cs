namespace Kenet.SimpleProcess;

public static partial class ProcessExecutorBuilderGenericExtensions
{
    /// <inheritdoc cref="IProcessExecutorMutator.AddCancellation(IEnumerable{CancellationToken})"/>
    public static T AddCancellation<T>(this T mutator, params CancellationToken[] cancellationTokens)
        where T : IProcessExecutorMutator
    {
        mutator.AddCancellation(cancellationTokens.AsEnumerable());
        return mutator;
    }

    /// <summary>
    /// Sets a exit code validator, that only accepts <paramref name="validExitCode"/> as valid exit code.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mutator"></param>
    /// <param name="validExitCode"></param>
    public static T WithExitCode<T>(this T mutator, int validExitCode)
        where T : IProcessExecutorMutator
    {
        bool Validate(int exitCode) => exitCode == validExitCode;
        mutator.WithExitCode(Validate);
        return mutator;
    }

    /// <summary>
    /// Sets a exit code validator, that only accepts <paramref name="validExitCodes"/> as valid exit codes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mutator"></param>
    /// <param name="validExitCodes"></param>
    public static T WithExitCode<T>(this T mutator, params int[] validExitCodes)
        where T : IProcessExecutorMutator
    {
        bool Validate(int exitCode)
        {
            foreach (var validExitCode in validExitCodes) {
                if (validExitCode == exitCode) {
                    return true;
                }
            }

            return false;
        };

        mutator.WithExitCode(Validate);
        return mutator;
    }
}
