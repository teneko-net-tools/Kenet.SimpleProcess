namespace Kenet.SimpleProcess;

/// <summary>
/// Represents an asynchronous process execution.
/// </summary>
public interface IAsyncProcessExecution : IExecutingProcess, IAsyncCompletableProcessExecution, IDisposableProcess
{
}
