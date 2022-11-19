namespace Kenet.SimpleProcess.Execution;

public interface IProcessExecutor
{
    /// <summary>
    /// Executes the process synchronously.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ProcessExecutionResult Execute(CancellationToken cancellationToken);
}
