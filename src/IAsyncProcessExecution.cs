namespace Kenet.SimpleProcess;

/// <summary>
/// Represents an asynchronous process.
/// </summary>
public interface IAsyncProcessExecution : IRunningProcess
{
    /// <inheritdoc cref="IProcessExecution.RunToCompletion(CancellationToken)"/>.
    Task<int> RunToCompletionAsync(CancellationToken cancellationToken, ProcessCompletionOptions completionOptions);
}
