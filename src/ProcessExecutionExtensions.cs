namespace Kenet.SimpleProcess;

public static class ProcessExecutionExtensions
{
    /// <inheritdoc cref="IProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="IProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/param[@name='completionOptions']"/>
    /// <inheritdoc cref="IProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static int RunToCompletion(this IProcessExecution execution, CancellationToken cancellationToken = default) =>
        execution.RunToCompletion(cancellationToken, ProcessCompletionOptions.None);

    /// <inheritdoc cref="IProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="IProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/param[@name='completionOptions']"/>
    /// <inheritdoc cref="IProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static int RunToCompletion(this IProcessExecution execution, ProcessCompletionOptions completionOptions) =>
        execution.RunToCompletion(CancellationToken.None, completionOptions);
}
