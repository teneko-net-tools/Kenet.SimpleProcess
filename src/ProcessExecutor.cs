namespace Kenet.SimpleProcess;

/// <summary>
/// A repeatable process executor.
/// </summary>
public sealed class ProcessExecutor : IRunnable<IProcessExecution>, IRunnable<IAsyncProcessExecution>
{
    private readonly IProcessExecutorArtifact _artifact;

    public ProcessExecutor(IProcessExecutorArtifact artifact) =>
        _artifact = artifact ?? throw new ArgumentNullException(nameof(artifact));

    public ProcessExecution Run()
    {
        var execution = ProcessExecution.Create(_artifact);
        execution.Run();
        return execution;
    }

    IProcessExecution IRunnable<IProcessExecution>.Run() =>
        Run();

    IAsyncProcessExecution IRunnable<IAsyncProcessExecution>.Run() =>
        Run();
}
