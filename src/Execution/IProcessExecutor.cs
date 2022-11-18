namespace Kenet.SimpleProcess.Execution;

public interface IProcessExecutor
{
    /// <summary>
    /// Executes the process synchronously.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ProcessExecution Execute(CancellationToken cancellationToken);
}
