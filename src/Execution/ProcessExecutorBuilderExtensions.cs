namespace Kenet.SimpleProcess.Execution;

public static class ProcessExecutorBuilderExtensions
{
    public static IProcessExecutor Build(this IProcessExecutorBuilder builder) =>
        new ProcessExecutor(builder.BuildArtifact());

    public static IAsyncProcessExecutor BuildAsync(this IProcessExecutorBuilder builder) =>
        new ProcessExecutor(builder.BuildArtifact());
}
