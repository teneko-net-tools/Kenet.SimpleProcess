namespace Kenet.SimpleProcess;

/// <summary>
/// Represents an asynchronous process.
/// </summary>
public interface IAsyncCompletableProcess
{
    /// <inheritdoc cref="ICompletableProcess.RunToCompletion(CancellationToken, ProcessCompletionOptions)"/>.
    Task<int> RunToCompletionAsync(CancellationToken cancellationToken, ProcessCompletionOptions completionOptions);
}
