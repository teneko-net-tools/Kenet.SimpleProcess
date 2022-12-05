namespace Kenet.SimpleProcess;

/// <summary>
/// Extension methods for <see cref="IProcessExecution"/>.
/// </summary>
public static class ProcessExecutionExtensions
{
    /// <inheritdoc cref="ICompletableProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="ICompletableProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/param[@name='cancellationToken']"/>
    /// <inheritdoc cref="ICompletableProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static int RunToCompletion(this ICompletableProcessExecution execution, CancellationToken cancellationToken) =>
        execution.RunToCompletion(cancellationToken, ProcessCompletionOptions.None);

    /// <inheritdoc cref="ICompletableProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="ICompletableProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static int RunToCompletion(this ICompletableProcessExecution execution) =>
        execution.RunToCompletion(CancellationToken.None, ProcessCompletionOptions.None);

    /// <inheritdoc cref="ICompletableProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="ICompletableProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/param[@name='completionOptions']"/>
    /// <inheritdoc cref="ICompletableProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static int RunToCompletion(this ICompletableProcessExecution execution, ProcessCompletionOptions completionOptions) =>
        execution.RunToCompletion(CancellationToken.None, completionOptions);

    /// <inheritdoc cref="IAsyncCompletableProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="IAsyncCompletableProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/param[@name='cancellationToken']"/>
    /// <inheritdoc cref="IAsyncCompletableProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static Task<int> RunToCompletionAsync(this IAsyncCompletableProcessExecution execution, CancellationToken cancellationToken) =>
        execution.RunToCompletionAsync(cancellationToken, ProcessCompletionOptions.None);

    /// <inheritdoc cref="IAsyncCompletableProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="IAsyncCompletableProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static Task<int> RunToCompletionAsync(this IAsyncCompletableProcessExecution execution) =>
        execution.RunToCompletionAsync(CancellationToken.None, ProcessCompletionOptions.None);

    /// <inheritdoc cref="IAsyncCompletableProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="IAsyncCompletableProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/param[@name='completionOptions']"/>
    /// <inheritdoc cref="IAsyncCompletableProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static Task<int> RunToCompletionAsync(this IAsyncCompletableProcessExecution execution, ProcessCompletionOptions completionOptions) =>
        execution.RunToCompletionAsync(CancellationToken.None, completionOptions);
}
