namespace Kenet.SimpleProcess;

/// <summary>
/// Represents an asynchronous process.
/// </summary>
public interface IAsyncProcess : IProcessExecution, IDisposable
{
    /// <inheritdoc cref="System.Diagnostics.Process.Start()" />
    bool Start();

    /// <summary>
    /// Instructs the process to wait for the associated process to exit.
    /// </summary>
    Task<int> WaitForExitAsync();
}
