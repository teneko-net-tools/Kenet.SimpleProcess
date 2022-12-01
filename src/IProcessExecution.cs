namespace Kenet.SimpleProcess;

/// <summary>
/// Represents a synchronous process execution.
/// </summary>
public interface IProcessExecution : IExecutingProcess, ICompletableProcessExecution, IDisposableProcess
{
}
