namespace Kenet.SimpleProcess;

public static class AsyncProcessExecutionExtensions
{
    /// <inheritdoc cref="IProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="IProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/param[@name='cancellationToken']"/>
    /// <inheritdoc cref="IProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static Task<int> RunToCompletionAsync(this IAsyncProcessExecution execution, CancellationToken cancellationToken = default) =>
        execution.RunToCompletionAsync(cancellationToken, ProcessCompletionOptions.None);

    /// <inheritdoc cref="IProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="IProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/param[@name='completionOptions']"/>
    /// <inheritdoc cref="IProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static Task<int> RunToCompletionAsync(this IAsyncProcessExecution execution, ProcessCompletionOptions completionOptions) =>
        execution.RunToCompletionAsync(CancellationToken.None, completionOptions);
}
