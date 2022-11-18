namespace Kenet.SimpleProcess.Execution;

public interface IAsyncProcessExecutor
{
    /// <summary>
    /// Executes the process asynchronously.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ProcessExecution> ExecuteAsync(CancellationToken cancellationToken);
}
