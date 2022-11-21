namespace Kenet.SimpleProcess;

public interface IProcessExecutorBuilder
{
    /// <summary>
    /// Builds an artifact of the current state of the builder.
    /// </summary>
    /// <returns>An immutable artifact to be used creating a process executor.</returns>
    IProcessExecutorArtifact BuildArtifact();
}
