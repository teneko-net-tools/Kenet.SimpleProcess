namespace Kenet.SimpleProcess;

/// <summary>
/// Extension methods for <see cref="ProcessExecutor"/>.
/// </summary>
public static class ProcessExecutorExtensions
{
    /// <inheritdoc cref="ProcessExecutor.NotifyWhenStarted(Action{ProcessExecution})"/>
    /// <typeparam name="T">The return type. When the callback gets called, its return value will be discarded and not used.</typeparam>
    public static ProcessExecutor OnRun<T>(this ProcessExecutor executor, Func<ProcessExecution, T> callback) =>
        executor.NotifyWhenStarted((execution) => callback(execution));
}
