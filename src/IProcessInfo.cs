namespace Kenet.SimpleProcess;

/// <summary>
/// Represents the overall context of a process execution.
/// </summary>
public interface IProcessInfo
{
    /// <summary>
    /// The process id.
    /// </summary>
    /// <remarks>
    /// Available after the process has ran.
    /// </remarks>
    int? Id { get; }

    /// <summary>
    /// Triggered when the process started.
    /// </summary>
    CancellationToken Started { get; }

    /// <summary>
    /// Indicates that the process has been started.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Triggered when the process exited. Use it to safely register callbacks by using
    /// <see cref="CancellationToken.Register(Action)"/> whose callback gets called when the
    /// process exited.
    /// </summary>
    /// <remarks>
    /// There are cases, where the this cancellation token is not always cancelled, for example
    /// when the process has not been ran at all. For getting a definitely end of process, use
    /// <see cref="Cancelled"/>, but keep in mind, that it get called when disposed at the
    /// latest.
    /// </remarks>
    CancellationToken Exited { get; }

    /// <summary>
    /// Indicates whether the process has been exited or not.
    /// </summary>
    bool IsExited { get; }

    /// <summary>
    /// Triggered when the process cancelled or disposed.
    /// </summary>
    /// <remarks>
    /// Even if the process has fallen into the cancelling state, it won't be taken any
    /// proactive action to kill the process. The least what can happen is, that
    /// on-going process completion tasks are gonna canceled, because underlying
    /// output or error stream reader are automatically canceled when the process
    /// has fallen into the cancelling state.
    /// </remarks>
    CancellationToken Cancelled { get; }

    /// <summary>
    /// Indicates whether the process has been disposed or not.
    /// </summary>
    bool IsDisposed { get; }
}
