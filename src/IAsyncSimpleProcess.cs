using System.Diagnostics;

namespace Kenet.SimpleProcess;

/// <summary>
/// Represents the asynchronous process.
/// </summary>
public interface IAsyncSimpleProcess
{
    /// <inheritdoc cref="Process.Start()" />
    bool Start();

    /// <summary>
    /// Instructs the process to wait for the associated process to exit.
    /// </summary>
    Task<int> WaitForExitAsync();
}
