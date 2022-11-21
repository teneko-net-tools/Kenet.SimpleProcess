namespace Kenet.SimpleProcess;

[Flags]
public enum ProcessCompletionOptions
{
    None = 0,
    /// <summary>
    /// If the method associated cancellation token is cancellation requested, then the process is forced to terminate its underlying process.
    /// </summary>
    KillOnCancellationRequested = 1,

    /// <summary>
    /// If the method associated cancellation token is cancellation requested, then the process is forced to terminate its underlying process,
    /// and optionally its child/descendent processes.
    /// </summary>
    KillTreeOnCancellationRequested = KillOnCancellationRequested | 2,

    /// <summary>
    /// Disposes the process when the process completed successfully.
    /// </summary>
    DisposeOnCompleted = 16,

    /// <summary>
    /// Disposes the process when the process completion was not successful.
    /// </summary>
    DisposeOnFailure = 32,
}
