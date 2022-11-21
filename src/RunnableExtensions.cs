namespace Kenet.SimpleProcess;

public static class RunnableExtensions
{
    private static readonly ProcessCompletionOptions _blindCompletionOptions =
        ProcessCompletionOptions.KillTreeOnCancellationRequested
        | ProcessCompletionOptions.DisposeOnCompleted
        | ProcessCompletionOptions.DisposeOnFailure;

    /// <inheritdoc cref="IProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)"/>
    public static int RunToCompletion(this IRunnable<IProcessExecution> process, CancellationToken cancellationToken, ProcessCompletionOptions completionOptions) =>
        process.Run().RunToCompletion(cancellationToken, completionOptions);

    /// <summary>
    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(IProcessExecution, CancellationToken)" path="/summary"/>
    /// Because this method hides <see cref="IProcessExecution"/> implictly, you won't be able to dispose it on your own, so we pass
    /// <see cref="ProcessCompletionOptions.DisposeOnCompleted"/> and <see cref="ProcessCompletionOptions.KillTreeOnCancellationRequested"/> by default.
    /// </summary>
    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(IProcessExecution, CancellationToken)"/>
    public static int RunToCompletion(this IRunnable<IProcessExecution> process, CancellationToken cancellationToken = default) =>
        process.Run().RunToCompletion(cancellationToken, _blindCompletionOptions);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(IProcessExecution, ProcessCompletionOptions)"/>
    public static int RunToCompletion(this IRunnable<IProcessExecution> process, ProcessCompletionOptions completionOptions) =>
        process.Run().RunToCompletion(CancellationToken.None, completionOptions);

    /// <inheritdoc cref="IProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)"/>
    public static Task<int> RunToCompletionAsync(this IRunnable<IAsyncProcessExecution> process, CancellationToken cancellationToken, ProcessCompletionOptions completionOptions) =>
        process.Run().RunToCompletionAsync(cancellationToken, completionOptions);

    /// <summary>
    /// <inheritdoc cref="RunToCompletion(IRunnable{IProcessExecution}, CancellationToken)" path="/summary"/>
    /// </summary>
    /// <inheritdoc cref="AsyncProcessExecutionExtensions.RunToCompletionAsync(IAsyncProcessExecution, CancellationToken)" path="/*[not(self::summary)]"/>
    public static Task<int> RunToCompletionAsync(this IRunnable<IAsyncProcessExecution> process, CancellationToken cancellationToken = default) =>
        process.Run().RunToCompletionAsync(cancellationToken, _blindCompletionOptions);

    /// <inheritdoc cref="AsyncProcessExecutionExtensions.RunToCompletionAsync(IAsyncProcessExecution, ProcessCompletionOptions)"/>
    public static Task<int> RunToCompletionAsync(this IRunnable<IAsyncProcessExecution> process, ProcessCompletionOptions completionOptions) =>
        process.Run().RunToCompletionAsync(CancellationToken.None, completionOptions);
}
