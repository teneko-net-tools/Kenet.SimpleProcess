﻿using System.Diagnostics;

namespace Kenet.SimpleProcess;

/// <summary>
/// Represents the running process.
/// </summary>
public interface IRunningProcess : IProcessExecutionContext
{
    /// <summary>
    /// Cancels the current process.
    /// </summary>
    /// <remarks>
    /// It does not kill the process.
    /// </remarks>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="AggregateException"/>
    void Cancel();

    /// <summary>
    /// Cancels the current process after given milliseconds.
    /// </summary>
    /// <remarks>
    /// It does not kill the process.
    /// </remarks>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    void CancelAfter(int delayInMilliseconds);

    /// <summary>
    /// Cancels the current process after the delay.
    /// </summary>
    /// <remarks>
    /// It does not kill the process.
    /// </remarks>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    void CancelAfter(TimeSpan delay);

    /// <inheritdoc cref="Process.Kill()"/>
    void Kill();

    /// Immediatelly stops the associated process, and optionally its child/descendant processes.
    /// <param name="entireProcessTree">
    /// <see langword="true"/> to kill the associated process and its descendants;
    /// <see langword="false"/> to kill only the associated process.
    /// </param>
    void Kill(bool entireProcessTree);
}
