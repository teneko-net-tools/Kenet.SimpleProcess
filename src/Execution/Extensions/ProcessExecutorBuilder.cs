namespace Kenet.SimpleProcess.Execution.Extensions;

/// <summary>
/// Opinionated extension methods for <see cref="IProcessExecutorBuilder" />.
/// </summary>
public static class ProcessExecutorBuilder
{
    public static int Execute(this IProcessExecutorBuilder builder, CancellationToken cancellationToken) =>
        builder.Build().Execute(cancellationToken).ExitCode;

    public static Task<int> ExecuteAsync(this IProcessExecutorBuilder builder, CancellationToken cancellationToken) =>
        builder.BuildAsync().ExecuteAsync(cancellationToken).ContinueWith(
            task => task.Result.ExitCode,
            cancellationToken,
            TaskContinuationOptions.OnlyOnRanToCompletion,
            TaskScheduler.Current);
}
