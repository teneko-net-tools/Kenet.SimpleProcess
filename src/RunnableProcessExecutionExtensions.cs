namespace Kenet.SimpleProcess;

/// <summary>
/// Extension methods for <see cref="IRunnable{T}"/> with T of <see cref="IContextlessProcessExecution"/>
/// and <see cref="IRunnable{T}"/> with T of <see cref="IAsyncContextlessProcessExecution"/>.
/// </summary>
public static class RunnableProcessExecutionExtensions
{
    private static readonly ProcessCompletionOptions _fireAndForgetCompletionOptions =
        ProcessCompletionOptions.KillTreeOnCancellation;

    /// <inheritdoc cref="IContextlessProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)"/>
    public static int RunToCompletion(this IRunnable<IProcessExecution> process, CancellationToken cancellationToken, ProcessCompletionOptions completionOptions)
    {
        using var execution = process.Run();
        return execution.RunToCompletion(cancellationToken, completionOptions);
    }

    /// <summary>
    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(IContextlessProcessExecution, CancellationToken)" path="/summary"/>
    /// Because this method is a shortcut of <see cref="IContextlessProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)"/>,
    /// you won't be able to dispose the instance of type <see cref="IContextlessProcessExecution"/>. Therefore we kill the entire tree and dispose the
    /// process after the completion.
    /// </summary>
    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(IContextlessProcessExecution, CancellationToken)"/>
    public static int RunToCompletion(this IRunnable<IProcessExecution> process, CancellationToken cancellationToken) =>
         RunToCompletion(process, cancellationToken, _fireAndForgetCompletionOptions);

    /// <inheritdoc cref="RunToCompletion(IRunnable{IProcessExecution}, CancellationToken)"/>
    public static int RunToCompletion(this IRunnable<IProcessExecution> process) =>
         RunToCompletion(process, CancellationToken.None, _fireAndForgetCompletionOptions);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(IContextlessProcessExecution, ProcessCompletionOptions)"/>
    public static int RunToCompletion(this IRunnable<IProcessExecution> process, ProcessCompletionOptions completionOptions) =>
        RunToCompletion(process, CancellationToken.None, completionOptions);

    /// <inheritdoc cref="IAsyncContextlessProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)"/>
    public static async Task<int> RunToCompletionAsync(this IRunnable<IAsyncProcessExecution> process, CancellationToken cancellationToken, ProcessCompletionOptions completionOptions)
    {
        using var execution = process.Run();
        return await execution.RunToCompletionAsync(cancellationToken, completionOptions);
    }

    /// <summary>
    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletionAsync(IAsyncContextlessProcessExecution, CancellationToken)" path="/summary"/>
    /// Because this method is a shortcut of <see cref="IAsyncContextlessProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)"/>,
    /// you won't be able to dispose the instance of type <see cref="IAsyncContextlessProcessExecution"/>. Therefore we kill the entire tree and dispose the
    /// process after the completion.
    /// </summary>
    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletionAsync(IAsyncContextlessProcessExecution, CancellationToken)"/>
    public static Task<int> RunToCompletionAsync(this IRunnable<IAsyncProcessExecution> process, CancellationToken cancellationToken) =>
        RunToCompletionAsync(process, cancellationToken, _fireAndForgetCompletionOptions);

    /// <inheritdoc cref="RunToCompletionAsync(IRunnable{IAsyncProcessExecution}, CancellationToken)"/>
    public static Task<int> RunToCompletionAsync(this IRunnable<IAsyncProcessExecution> process) =>
        RunToCompletionAsync(process, CancellationToken.None, _fireAndForgetCompletionOptions);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletionAsync(IAsyncContextlessProcessExecution, ProcessCompletionOptions)"/>
    public static Task<int> RunToCompletionAsync(this IRunnable<IAsyncProcessExecution> process, ProcessCompletionOptions completionOptions) =>
        RunToCompletionAsync(process, CancellationToken.None, completionOptions);
}
