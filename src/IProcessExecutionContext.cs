namespace Kenet.SimpleProcess;

public interface IProcessExecutionContext
{
    /// <summary>
    /// The cancellation token, that gets cancelled when exited. You can use it to
    /// safely register callbacks by using <see cref="CancellationToken.Register(Action)"/>
    /// whose callback gets definitively called even when the process have been canceled before
    /// the start or if the process have been already exited.
    /// </summary>
    CancellationToken Exited { get; }

    /// <summary>
    /// A boolean indicating whether the process has been exited or not.
    /// </summary>
    bool IsExited { get; }
}
