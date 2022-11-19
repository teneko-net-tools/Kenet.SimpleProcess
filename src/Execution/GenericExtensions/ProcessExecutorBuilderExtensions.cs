namespace Kenet.SimpleProcess.Execution.GenericExtensions;

public static partial class ProcessExecutorBuilderExtensions
{
    public static T AddCancellation<T>(this T builder, params CancellationToken[] cancellationTokens)
        where T : IProcessExecutorBuilder
    {
        builder.AddCancellation(cancellationTokens.AsEnumerable());
        return builder;
    }
}
