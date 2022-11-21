using System.Diagnostics;

namespace Kenet.SimpleProcess;

public interface IRunningProcess : IProcessExecutionContext, IDisposable
{
    /// <inheritdoc cref="Process.Kill"/>
    void Kill();

    /// <inheritdoc cref="Process.Kill(bool)"/>
    void Kill(bool entireProcessTree);
}
