using System.Diagnostics;

namespace Kenet.SimpleProcess
{
    /// <summary>
    /// Represents the synchronous process.
    /// </summary>
    public interface ISimpleProcess : IDisposable
    {
        /// <inheritdoc cref="Process.Start()" />
        bool Start();

        /// <summary>
        /// Instructs the process to wait for the associated process to exit.
        /// </summary>
        int WaitForExit();
    }
}
