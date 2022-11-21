namespace Kenet.SimpleProcess;

public static class ProcessExecutorBuilderExtensions
{
    public static ProcessExecutor Build(this IProcessExecutorBuilder builder) =>
        new(builder.BuildArtifact());
}
