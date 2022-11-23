namespace Kenet.SimpleProcess;

/// <summary>
/// Extension methods for <see cref="IProcessExecution"/>.
/// </summary>
public static class ProcessExecutionGenericExtensions
{
    /// <summary>
    /// Associates <paramref name="execution"/> with <paramref name="boundary"/>,
    /// so <paramref name="execution"/> gets automatically disposed,
    /// if <paramref name="boundary"/> gets disposed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="execution"></param>
    /// <param name="boundary"></param>
    /// <returns></returns>
    public static T Associate<T>(this T execution, ProcessBoundary boundary)
        where T : IDisposableProcess
    {
        boundary.Associate(execution);
        return execution;
    }
}
