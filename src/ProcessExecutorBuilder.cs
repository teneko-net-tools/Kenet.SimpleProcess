using System.Text;

namespace Kenet.SimpleProcess;

/// <summary>
/// Builds a process executor.
/// </summary>
public sealed class ProcessExecutorBuilder :
    IProcessExecutorBuilder,
    IProcessExecutorMutator,
    IRunnable<IProcessExecution>,
    IRunnable<ICompletableProcessExecution>,
    IRunnable<IAsyncProcessExecution>,
    IRunnable<IAsyncCompletableProcessExecution>
{
    /// <summary>
    /// Creates a builder with error interpretation in case of a bad exit code.
    /// </summary>
    /// <param name="startInfo"></param>
    /// <param name="validExitCode"></param>
    /// <returns></returns>
    public static ProcessExecutorBuilder CreateDefault(SimpleProcessStartInfo startInfo, int validExitCode) =>
        new ProcessExecutorBuilder(startInfo)
        .WithExitCode(validExitCode)
        .WithErrorInterpretation();

    /// <summary>
    /// Creates a builder with error interpretation in case <paramref name="validateExitCode"/> returns <see langword="false"/>.
    /// </summary>
    /// <param name="startInfo"></param>
    /// <param name="validateExitCode"></param>
    /// <returns></returns>
    public static ProcessExecutorBuilder CreateDefault(SimpleProcessStartInfo startInfo, Func<int, bool> validateExitCode) =>
        new ProcessExecutorBuilder(startInfo)
        .WithExitCode(validateExitCode)
        .WithErrorInterpretation();

    /// <summary>
    /// Creates a builder with error interpretation in case the exit code is not equals zero.
    /// </summary>
    /// <param name="startInfo"></param>
    /// <returns></returns>
    public static ProcessExecutorBuilder CreateDefault(SimpleProcessStartInfo startInfo) =>
        CreateDefault(startInfo, validExitCode: 0);

    private ProcessExecutorArtifact _artifact;

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    /// <param name="startInfo"></param>
    public ProcessExecutorBuilder(SimpleProcessStartInfo startInfo) =>
        _artifact = new ProcessExecutorArtifact(startInfo);

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    /// <param name="artifact"></param>
    internal ProcessExecutorBuilder(ProcessExecutorArtifact artifact) =>
        _artifact = artifact ?? throw new ArgumentNullException(nameof(artifact));

    /// <inheritdoc cref="IProcessExecutorMutator.WithExitCode(Func{int, bool})"/>
    public ProcessExecutorBuilder WithExitCode(Func<int, bool> validator)
    {
        _artifact.ValidateExitCode = validator;
        return this;
    }

    void IProcessExecutorMutator.WithExitCode(Func<int, bool> validate) =>
        WithExitCode(validate);

    /// <inheritdoc cref="IProcessExecutorMutator.WithErrorInterpretation(Encoding?)"/>
    public ProcessExecutorBuilder WithErrorInterpretation(Encoding? encoding = null)
    {
        _artifact.ExitErrorEncoding ??= encoding ?? Encoding.UTF8;
        return this;
    }

    void IProcessExecutorMutator.WithErrorInterpretation(Encoding? encoding) =>
        WithErrorInterpretation(encoding);

    /// <inheritdoc cref="IProcessExecutorMutator.AddErrorWriter(WriteHandler)"/>
    public ProcessExecutorBuilder AddErrorWriter(WriteHandler writer)
    {
        _artifact.ErrorWriters.Add(writer);
        return this;
    }

    void IProcessExecutorMutator.AddErrorWriter(WriteHandler writer) =>
        AddErrorWriter(writer);

    /// <inheritdoc cref="IProcessExecutorMutator.AddOutputWriter(WriteHandler)"/>
    public ProcessExecutorBuilder AddOutputWriter(WriteHandler writer)
    {
        _artifact.OutputWriters.Add(writer);
        return this;
    }

    void IProcessExecutorMutator.AddOutputWriter(WriteHandler writer) =>
        AddOutputWriter(writer);

    /// <inheritdoc cref="IProcessExecutorMutator.AddCancellation(IEnumerable{CancellationToken})"/>
    public ProcessExecutorBuilder AddCancellation(IEnumerable<CancellationToken> cancellationTokens)
    {
        _artifact.CancellationTokens.AddRange(cancellationTokens);
        return this;
    }

    void IProcessExecutorMutator.AddCancellation(IEnumerable<CancellationToken> cancellationTokens) =>
        AddCancellation(cancellationTokens);

    private ProcessExecutorArtifact BuildArtifact() =>
        _artifact.Clone();

    IProcessExecutorArtifact IProcessExecutorBuilder.BuildArtifact() =>
        BuildArtifact();

    /// <summary>
    /// Builds the process executor.
    /// </summary>
    /// <returns></returns>
    public ProcessExecutor Build() =>
        new ProcessExecutor(BuildArtifact());

    /// <summary>
    /// Runs the process with the current design of a process executor.
    /// </summary>
    /// <returns></returns>
    public ProcessExecution Run()
    {
        var execution = ProcessExecution.Create(BuildArtifact());
        execution.Run();
        return execution;
    }

    IProcessExecution IRunnable<IProcessExecution>.Run() =>
        Run();

    ICompletableProcessExecution IRunnable<ICompletableProcessExecution>.Run() =>
        Run();

    IAsyncProcessExecution IRunnable<IAsyncProcessExecution>.Run() =>
        Run();

    IAsyncCompletableProcessExecution IRunnable<IAsyncCompletableProcessExecution>.Run() =>
        Run();
}
