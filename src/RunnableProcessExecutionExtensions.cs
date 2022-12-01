namespace Kenet.SimpleProcess;

/// <summary>
/// Extension methods for <see cref="IRunnable{T}"/> with T of <see cref="ICompletableProcessExecution"/>
/// and <see cref="IRunnable{T}"/> with T of <see cref="IAsyncCompletableProcessExecution"/>.
/// </summary>
public static class RunnableProcessExecutionExtensions
{
    private static readonly ProcessCompletionOptions _fireAndForgetCompletionOptions =
        ProcessCompletionOptions.KillTreeOnCancellation;

    /// <inheritdoc cref="ICompletableProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)"/>
    public static int RunToCompletion(this IRunnable<IProcessExecution> process, CancellationToken cancellationToken, ProcessCompletionOptions completionOptions)
    {
        using var execution = process.Run();
        return execution.RunToCompletion(cancellationToken, completionOptions);
    }

    /// <summary>
    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(ICompletableProcessExecution, CancellationToken)" path="/summary"/>
    /// Because this method is a shortcut of <see cref="ICompletableProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)"/>,
    /// you won't be able to dispose the instance of type <see cref="ICompletableProcessExecution"/>. Therefore we kill the entire tree and dispose the
    /// process after the completion.
    /// </summary>
    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(ICompletableProcessExecution, CancellationToken)"/>
    public static int RunToCompletion(this IRunnable<IProcessExecution> process, CancellationToken cancellationToken) =>
         RunToCompletion(process, cancellationToken, _fireAndForgetCompletionOptions);

    /// <inheritdoc cref="RunToCompletion(IRunnable{IProcessExecution}, CancellationToken)"/>
    public static int RunToCompletion(this IRunnable<IProcessExecution> process) =>
         RunToCompletion(process, CancellationToken.None, _fireAndForgetCompletionOptions);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(ICompletableProcessExecution, ProcessCompletionOptions)"/>
    public static int RunToCompletion(this IRunnable<IProcessExecution> process, ProcessCompletionOptions completionOptions) =>
        RunToCompletion(process, CancellationToken.None, completionOptions);

    /// <inheritdoc cref="IAsyncCompletableProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)"/>
    public static async Task<int> RunToCompletionAsync(this IRunnable<IAsyncProcessExecution> process, CancellationToken cancellationToken, ProcessCompletionOptions completionOptions)
    {
        using var execution = process.Run();
        return await execution.RunToCompletionAsync(cancellationToken, completionOptions);
    }

    /// <summary>
    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletionAsync(IAsyncCompletableProcessExecution, CancellationToken)" path="/summary"/>
    /// Because this method is a shortcut of <see cref="IAsyncCompletableProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)"/>,
    /// you won't be able to dispose the instance of type <see cref="IAsyncCompletableProcessExecution"/>. Therefore we kill the entire tree and dispose the
    /// process after the completion.
    /// </summary>
    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletionAsync(IAsyncCompletableProcessExecution, CancellationToken)"/>
    public static Task<int> RunToCompletionAsync(this IRunnable<IAsyncProcessExecution> process, CancellationToken cancellationToken) =>
        RunToCompletionAsync(process, cancellationToken, _fireAndForgetCompletionOptions);

    /// <inheritdoc cref="RunToCompletionAsync(IRunnable{IAsyncProcessExecution}, CancellationToken)"/>
    public static Task<int> RunToCompletionAsync(this IRunnable<IAsyncProcessExecution> process) =>
        RunToCompletionAsync(process, CancellationToken.None, _fireAndForgetCompletionOptions);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletionAsync(IAsyncCompletableProcessExecution, ProcessCompletionOptions)"/>
    public static Task<int> RunToCompletionAsync(this IRunnable<IAsyncProcessExecution> process, ProcessCompletionOptions completionOptions) =>
        RunToCompletionAsync(process, CancellationToken.None, completionOptions);
}
