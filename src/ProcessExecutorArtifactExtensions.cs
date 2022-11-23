namespace Kenet.SimpleProcess;

internal static class ProcessExecutorArtifactExtensions
{
    internal static ProcessExecutorArtifact Clone(this IProcessExecutorArtifact artifact) =>
        new ProcessExecutorArtifact(
        artifact.StartInfo,
        new List<CancellationToken>(artifact.CancellationTokens),
        new List<WriteHandler>(artifact.ErrorWriters),
        new List<WriteHandler>(artifact.OutputWriters),
        artifact.ExitErrorEncoding,
        artifact.ValidateExitCode);
}
