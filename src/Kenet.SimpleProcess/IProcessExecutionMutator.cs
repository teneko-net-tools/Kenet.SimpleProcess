namespace Kenet.SimpleProcess;

public interface IProcessExecutionMutator : IProcessExecutorMutator
{
    /// <summary>
    /// Places a callback, that gets called after the process started.
    /// </summary>
    /// <inheritdoc cref="ProcessExecutor.WithExitCode(Func{int, bool})" path="/*[position()>last()-2]"/>
    public void WhenStarted(Action<IProcessInfo> callback);
}
