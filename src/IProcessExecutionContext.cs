namespace Kenet.SimpleProcess;

/// <summary>
/// Represents the overall context of a process execution.
/// </summary>
public interface IProcessExecutionContext
{
    /// <summary>
    /// Indicates that the process has been started.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// The cancellation token, that gets cancelled when exited. You can use it to
    /// safely register callbacks by using <see cref="CancellationToken.Register(Action)"/>
    /// whose callback gets called, if the process have been already exited.
    /// </summary>
    CancellationToken Exited { get; }

    /// <summary>
    /// Indicates whether the process has been exited or not.
    /// </summary>
    bool IsExited { get; }

    /// <summary>
    /// The cancellation token, that gets cancelled when the process exited or the
    /// process have been requested to fall into the cancelling state.
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
