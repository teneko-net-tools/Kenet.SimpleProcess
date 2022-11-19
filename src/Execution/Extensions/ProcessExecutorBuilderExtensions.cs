namespace Kenet.SimpleProcess.Execution.Extensions;

/// <summary>
/// Opinionated extension methods for <see cref="IProcessExecutorBuilder" />.
/// </summary>
public static class ProcessExecutorBuilderExtensions
{
    public static int Execute(this IProcessExecutorBuilder builder, CancellationToken cancellationToken) =>
        builder.Build().Execute(cancellationToken).ExitCode;

    public static async Task<int> ExecuteAsync(this IProcessExecutorBuilder builder, CancellationToken cancellationToken) =>
        (await builder.BuildAsync().ExecuteAsync(cancellationToken)).ExitCode;
}
