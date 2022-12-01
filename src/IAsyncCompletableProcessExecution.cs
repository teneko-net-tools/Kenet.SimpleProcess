namespace Kenet.SimpleProcess;

/// <summary>
/// Represents an asynchronous process.
/// </summary>
public interface IAsyncCompletableProcessExecution
{
    /// <inheritdoc cref="ICompletableProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)"/>.
    Task<int> RunToCompletionAsync(CancellationToken cancellationToken, ProcessCompletionOptions completionOptions);
}
