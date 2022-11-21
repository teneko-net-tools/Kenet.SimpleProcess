namespace Kenet.SimpleProcess;

public static partial class ProcessExecutorBuilderGenericExtensions
{
    public static T AddCancellation<T>(this T mutator, params CancellationToken[] cancellationTokens)
        where T : IProcessExecutorMutator
    {
        mutator.AddCancellation(cancellationTokens.AsEnumerable());
        return mutator;
    }

    public static T WithExitCode<T>(this T mutator, int validExitCode)
        where T : IProcessExecutorMutator
    {
        bool Validate(int exitCode) => exitCode == validExitCode;
        mutator.WithExitCode(Validate);
        return mutator;
    }

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
