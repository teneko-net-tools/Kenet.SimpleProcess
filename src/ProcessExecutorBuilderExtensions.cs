namespace Kenet.SimpleProcess;

/// <summary>
/// Extension methods for <see cref="IProcessExecutorBuilder"/>.
/// </summary>
public static class ProcessExecutorBuilderExtensions
{
    /// <summary>
    /// Builds the process executor.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static ProcessExecutor Build(this IProcessExecutorBuilder builder) =>
        new(builder.BuildArtifact());
}
