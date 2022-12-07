namespace Kenet.SimpleProcess;

public interface IProcessExecutionMutator : IProcessExecutorMutator
{
    /// <summary>
    /// Places a callback, that gets called after the process started.
    /// </summary>
    public void NotifyWhenStarted(Action<IProcessInfo> callback);
}
