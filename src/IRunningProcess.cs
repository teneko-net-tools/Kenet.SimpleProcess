using System.Diagnostics;

namespace Kenet.SimpleProcess;

/// <summary>
/// Represents the running process.
/// </summary>
public interface IRunningProcess : IProcessExecutionContext
{
    /// <inheritdoc cref="Process.Kill()"/>
    void Kill();

    /// Immediatelly stops the associated process, and optionally its child/descendant processes.
    /// <param name="entireProcessTree">
    /// <see langword="true"/> to kill the associated process and its descendants;
    /// <see langword="false"/> to kill only the associated process.
    /// </param>
    void Kill(bool entireProcessTree);
}
