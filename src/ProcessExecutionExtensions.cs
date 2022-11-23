namespace Kenet.SimpleProcess;

/// <summary>
/// Extension methods for <see cref="IProcessExecution"/>.
/// </summary>
public static class ProcessExecutionExtensions
{
    /// <inheritdoc cref="IContextlessProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="IContextlessProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/param[@name='completionOptions']"/>
    /// <inheritdoc cref="IContextlessProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static int RunToCompletion(this IContextlessProcessExecution execution, CancellationToken cancellationToken = default) =>
        execution.RunToCompletion(cancellationToken, ProcessCompletionOptions.None);

    /// <inheritdoc cref="IContextlessProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="IContextlessProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/param[@name='completionOptions']"/>
    /// <inheritdoc cref="IContextlessProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static int RunToCompletion(this IContextlessProcessExecution execution, ProcessCompletionOptions completionOptions) =>
        execution.RunToCompletion(CancellationToken.None, completionOptions);

    /// <inheritdoc cref="IAsyncContextlessProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="IAsyncContextlessProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/param[@name='cancellationToken']"/>
    /// <inheritdoc cref="IAsyncContextlessProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static Task<int> RunToCompletionAsync(this IAsyncContextlessProcessExecution execution, CancellationToken cancellationToken = default) =>
        execution.RunToCompletionAsync(cancellationToken, ProcessCompletionOptions.None);

    /// <inheritdoc cref="IAsyncContextlessProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="IAsyncContextlessProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/param[@name='completionOptions']"/>
    /// <inheritdoc cref="IAsyncContextlessProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static Task<int> RunToCompletionAsync(this IAsyncContextlessProcessExecution execution, ProcessCompletionOptions completionOptions) =>
        execution.RunToCompletionAsync(CancellationToken.None, completionOptions);
}
