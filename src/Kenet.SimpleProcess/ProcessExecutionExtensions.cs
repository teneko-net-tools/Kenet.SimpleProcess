namespace Kenet.SimpleProcess;

/// <summary>
/// Extension methods for <see cref="IProcessExecution"/>.
/// </summary>
public static class ProcessExecutionExtensions
{
    /// <inheritdoc cref="ICompletableProcess.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="ICompletableProcess.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/param[@name='cancellationToken']"/>
    /// <inheritdoc cref="ICompletableProcess.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static int RunToCompletion(this ICompletableProcess execution, CancellationToken cancellationToken) =>
        execution.RunToCompletion(cancellationToken, ProcessCompletionOptions.None);

    /// <inheritdoc cref="ICompletableProcess.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="ICompletableProcess.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static int RunToCompletion(this ICompletableProcess execution) =>
        execution.RunToCompletion(CancellationToken.None, ProcessCompletionOptions.None);

    /// <inheritdoc cref="ICompletableProcess.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="ICompletableProcess.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/param[@name='completionOptions']"/>
    /// <inheritdoc cref="ICompletableProcess.RunToCompletion(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static int RunToCompletion(this ICompletableProcess execution, ProcessCompletionOptions completionOptions) =>
        execution.RunToCompletion(CancellationToken.None, completionOptions);

    /// <inheritdoc cref="IAsyncCompletableProcess.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="IAsyncCompletableProcess.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/param[@name='cancellationToken']"/>
    /// <inheritdoc cref="IAsyncCompletableProcess.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static Task<int> RunToCompletionAsync(this IAsyncCompletableProcess execution, CancellationToken cancellationToken) =>
        execution.RunToCompletionAsync(cancellationToken, ProcessCompletionOptions.None);

    /// <inheritdoc cref="IAsyncCompletableProcess.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="IAsyncCompletableProcess.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static Task<int> RunToCompletionAsync(this IAsyncCompletableProcess execution) =>
        execution.RunToCompletionAsync(CancellationToken.None, ProcessCompletionOptions.None);

    /// <inheritdoc cref="IAsyncCompletableProcess.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/summary"/>
    /// <inheritdoc cref="IAsyncCompletableProcess.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/param[@name='completionOptions']"/>
    /// <inheritdoc cref="IAsyncCompletableProcess.RunToCompletionAsync(CancellationToken, ProcessCompletionOptions)" path="/returns"/>
    public static Task<int> RunToCompletionAsync(this IAsyncCompletableProcess execution, ProcessCompletionOptions completionOptions) =>
        execution.RunToCompletionAsync(CancellationToken.None, completionOptions);
}
