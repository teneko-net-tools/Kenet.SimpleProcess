namespace Kenet.SimpleProcess;

/// <summary>
/// Extension methods for <see cref="IRunnable{T}"/> with T of <see cref="IContextlessProcessExecution"/>
/// and <see cref="IRunnable{T}"/> with T of <see cref="IAsyncContextlessProcessExecution"/>.
/// </summary>
public static class RunnableContextlessProcessExecutionExtensions
{
    private static readonly ProcessCompletionOptions _blindCompletionOptions =
        ProcessCompletionOptions.KillTreeOnCancellationRequested
        | ProcessCompletionOptions.DisposeOnCompleted
        | ProcessCompletionOptions.DisposeOnFailure;

    /// <inheritdoc cref="IContextlessProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)"/>
    public static int RunToCompletion(this IRunnable<IContextlessProcessExecution> process, CancellationToken cancellationToken, ProcessCompletionOptions completionOptions) =>
        process.Run().RunToCompletion(cancellationToken, completionOptions);

    /// <summary>
    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(IContextlessProcessExecution, CancellationToken)" path="/summary"/>
    /// Because this method is a shortcut of <see cref="IContextlessProcessExecution.RunToCompletion(CancellationToken, ProcessCompletionOptions)"/>,
    /// you won't be able to dispose the instance of type <see cref="IContextlessProcessExecution"/>, so we pass <see cref="ProcessCompletionOptions.DisposeOnCompleted"/>,
    /// <see cref="ProcessCompletionOptions.DisposeOnFailure"/> and <see cref="ProcessCompletionOptions.KillTreeOnCancellationRequested"/>
    /// by default.
    /// </summary>
    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(IContextlessProcessExecution, CancellationToken)"/>
    public static int RunToCompletion(this IRunnable<IContextlessProcessExecution> process, CancellationToken cancellationToken = default) =>
        process.Run().RunToCompletion(cancellationToken, _blindCompletionOptions);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletion(IContextlessProcessExecution, ProcessCompletionOptions)"/>
    public static int RunToCompletion(this IRunnable<IContextlessProcessExecution> process, ProcessCompletionOptions completionOptions) =>
        process.Run().RunToCompletion(CancellationToken.None, completionOptions);

    /// <inheritdoc cref="IAsyncContextlessProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)"/>
    public static Task<int> RunToCompletionAsync(this IRunnable<IAsyncContextlessProcessExecution> process, CancellationToken cancellationToken, ProcessCompletionOptions completionOptions) =>
        process.Run().RunToCompletionAsync(cancellationToken, completionOptions);

    /// <summary>
    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletionAsync(IAsyncContextlessProcessExecution, CancellationToken)" path="/summary"/>
    /// Because this method is a shortcut of <see cref="IAsyncContextlessProcessExecution.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)"/>,
    /// you won't be able to dispose the instance of type <see cref="IAsyncContextlessProcessExecution"/>, so we pass <see cref="ProcessCompletionOptions.DisposeOnCompleted"/>,
    /// <see cref="ProcessCompletionOptions.DisposeOnFailure"/> and <see cref="ProcessCompletionOptions.KillTreeOnCancellationRequested"/>
    /// by default.
    /// </summary>
    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletionAsync(IAsyncContextlessProcessExecution, CancellationToken)"/>
    public static Task<int> RunToCompletionAsync(this IRunnable<IAsyncContextlessProcessExecution> process, CancellationToken cancellationToken = default) =>
        process.Run().RunToCompletionAsync(cancellationToken, _blindCompletionOptions);

    /// <inheritdoc cref="ProcessExecutionExtensions.RunToCompletionAsync(IAsyncContextlessProcessExecution, ProcessCompletionOptions)"/>
    public static Task<int> RunToCompletionAsync(this IRunnable<IAsyncContextlessProcessExecution> process, ProcessCompletionOptions completionOptions) =>
        process.Run().RunToCompletionAsync(CancellationToken.None, completionOptions);
}
