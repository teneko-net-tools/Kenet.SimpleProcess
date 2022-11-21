using System.Text;

namespace Kenet.SimpleProcess;

/// <summary>
/// Describes a process to be executed.
/// </summary>
public sealed class ProcessExecutorBuilder : IProcessExecutorBuilder, IProcessExecutorMutator, IRunnable<IProcessExecution>, IRunnable<IAsyncProcessExecution>
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

    private readonly List<CancellationToken> _cancellationTokens = new();
    private readonly List<WriteHandler> _errorTracers = new();
    private readonly List<WriteHandler> _outputTracers = new();
    private readonly SimpleProcessStartInfo _startInfo;
    private Encoding? _exitErrorEncoding;
    private Func<int, bool>? _validateExitCode;

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    /// <param name="startInfo"></param>
    public ProcessExecutorBuilder(SimpleProcessStartInfo startInfo) =>
        _startInfo = startInfo;

    public ProcessExecutorBuilder WithExitCode(Func<int, bool> validator)
    {
        _validateExitCode = validator;
        return this;
    }

    IProcessExecutorMutator IProcessExecutorMutator.WithExitCode(Func<int, bool> validate) =>
        WithExitCode(validate);

    public ProcessExecutorBuilder WithErrorInterpretation(Encoding? encoding = null)
    {
        _exitErrorEncoding ??= encoding ?? Encoding.UTF8;
        return this;
    }

    IProcessExecutorMutator IProcessExecutorMutator.WithErrorInterpretation(Encoding? encoding) =>
        WithErrorInterpretation(encoding);

    public ProcessExecutorBuilder AddErrorWriter(WriteHandler writer)
    {
        _errorTracers.Add(writer);
        return this;
    }

    IProcessExecutorMutator IProcessExecutorMutator.AddErrorWriter(WriteHandler writer) =>
        AddErrorWriter(writer);

    public ProcessExecutorBuilder AddOutputWriter(WriteHandler writer)
    {
        _outputTracers.Add(writer);
        return this;
    }

    IProcessExecutorMutator IProcessExecutorMutator.AddOutputWriter(WriteHandler writer) =>
        AddOutputWriter(writer);

    public ProcessExecutorBuilder AddCancellation(IEnumerable<CancellationToken> cancellationTokens)
    {
        _cancellationTokens.AddRange(cancellationTokens);
        return this;
    }

    IProcessExecutorMutator IProcessExecutorMutator.AddCancellation(IEnumerable<CancellationToken> cancellationTokens) =>
        AddCancellation(cancellationTokens);

    private IProcessExecutorArtifact BuildArtifact() => new ProcessExecutorBuilderArtifact(
        _startInfo with { },
        new List<CancellationToken>(_cancellationTokens),
        new List<WriteHandler>(_errorTracers),
        new List<WriteHandler>(_outputTracers),
        _exitErrorEncoding,
        _validateExitCode);

    IProcessExecutorArtifact IProcessExecutorBuilder.BuildArtifact() =>
        BuildArtifact();

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

    IAsyncProcessExecution IRunnable<IAsyncProcessExecution>.Run() =>
        Run();

    IProcessExecution IRunnable<IProcessExecution>.Run() =>
        Run();
}
