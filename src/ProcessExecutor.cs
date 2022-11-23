namespace Kenet.SimpleProcess;

/// <summary>
/// A repeatable process executor.
/// </summary>
public sealed class ProcessExecutor :
    IRunnable<IProcessExecution>,
    IRunnable<IContextlessProcessExecution>,
    IRunnable<IAsyncProcessExecution>,
    IRunnable<IAsyncContextlessProcessExecution>
{
    private readonly IProcessExecutorArtifact _artifact;

    /// <summary>
    /// Creates an instance of this type by providing an artifact.
    /// </summary>
    /// <param name="artifact">
    /// The artifact to be used to spawn one or more process executions.
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public ProcessExecutor(IProcessExecutorArtifact artifact) =>
        _artifact = artifact ?? throw new ArgumentNullException(nameof(artifact));

    /// <summary>
    /// Makes a copy of the internal process executor state and passes it to a new builder instance.
    /// </summary>
    public ProcessExecutorBuilder ToBuilder() =>
        new ProcessExecutorBuilder(_artifact.Clone());

    /// <inheritdoc cref="SimpleProcess.Run()"/>
    public ProcessExecution Run()
    {
        var execution = ProcessExecution.Create(_artifact);
        execution.Run();
        return execution;
    }

    IProcessExecution IRunnable<IProcessExecution>.Run() =>
        Run();

    IContextlessProcessExecution IRunnable<IContextlessProcessExecution>.Run() =>
        Run();

    IAsyncProcessExecution IRunnable<IAsyncProcessExecution>.Run() =>
        Run();

    IAsyncContextlessProcessExecution IRunnable<IAsyncContextlessProcessExecution>.Run() =>
        Run();
}
