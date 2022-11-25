namespace Kenet.SimpleProcess;

/// <summary>
/// Represents an asynchronous process.
/// </summary>
public interface IAsyncContextlessProcessExecution : IExecutingProcess
{
    /// <inheritdoc cref="IContextlessProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)"/>.
    Task<int> RunToCompletionAsync(CancellationToken cancellationToken, ProcessCompletionOptions completionOptions);
}
