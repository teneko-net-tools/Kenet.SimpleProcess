namespace Kenet.SimpleProcess;

/// <summary>
/// Capable to build an artifact for <see cref="ProcessExecutor"/>. You create
/// </summary>
public interface IProcessExecutorBuilder
{
    /// <summary>
    /// Builds an artifact of the current state of the builder.
    /// </summary>
    /// <returns>An immutable artifact to be used creating a process executor.</returns>
    IProcessExecutorArtifact BuildArtifact();
}
