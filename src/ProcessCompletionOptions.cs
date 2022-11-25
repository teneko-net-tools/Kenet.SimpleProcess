using System.Diagnostics;

namespace Kenet.SimpleProcess;

/// <summary>
/// Tweaks the process completion.
/// </summary>
[Flags]
public enum ProcessCompletionOptions
{
    /// <summary>
    /// The process completes with intended behaviour.
    /// </summary>
    None = 0,

    /// <summary>
    /// Under the sole consideration of the the method associated cancellation token, it is waited for the process exit.
    /// It is equivalent to <see cref="Process.WaitForExit()"/> or <see cref="Process.WaitForExitAsync(CancellationToken)"/>
    /// depending on whether you wait for completion synchronously or asynchronously.
    /// </summary>
    WaitForExit = 1,

    /// <summary>
    /// If the method associated cancellation token is cancellation requested, then the process is forced to terminate its underlying process.
    /// </summary>
    KillOnCancellation = 2,

    /// <summary>
    /// If the method associated cancellation token is cancellation requested, then the process is forced to terminate its underlying process,
    /// and optionally its child/descendent processes.
    /// </summary>
    KillTreeOnCancellation = KillOnCancellation | 4,
}
