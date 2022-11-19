namespace Kenet.SimpleProcess;

/// <summary>
/// Represents a synchronous process.
/// </summary>
public interface IProcess : IProcessExecution, IDisposable
{
    /// <inheritdoc cref="System.Diagnostics.Process.Start()" />
    bool Start();

    /// <summary>
    /// Instructs the process to wait for the associated process to exit.
    /// </summary>
    int WaitForExit();
}
