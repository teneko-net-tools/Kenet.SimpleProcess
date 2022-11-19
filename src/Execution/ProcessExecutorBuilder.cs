using System.Text;

namespace Kenet.SimpleProcess.Execution;

/// <summary>
/// Describes a process to be executed.
/// </summary>
public sealed class ProcessExecutorBuilder : IProcessExecutorBuilder
{
    /// <summary>
    /// Creates a builder with error interpretation in case of a bad exit code.
    /// </summary>
    /// <param name="startInfo"></param>
    /// <param name="exitCode"></param>
    /// <returns></returns>
    public static IProcessExecutorBuilder CreateDefault(SimpleProcessStartInfo startInfo, int exitCode) =>
        new ProcessExecutorBuilder(startInfo).WithExitCode(exitCode).WithErrorInterpretation();

    /// <summary>
    /// Creates a builder with error interpretation in case the exit code is not equals zero.
    /// </summary>
    /// <param name="startInfo"></param>
    /// <returns></returns>
    public static IProcessExecutorBuilder CreateDefault(SimpleProcessStartInfo startInfo) =>
        CreateDefault(startInfo, exitCode: 0);

    private readonly List<CancellationToken> _cancellationTokens = new();
    private readonly List<WriteHandler> _errorTracers = new();
    private readonly List<WriteHandler> _outputTracers = new();
    private readonly SimpleProcessStartInfo _startInfo;
    private Encoding? _errorEncoding;
    private int? _expectedExitCode;

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    /// <param name="startInfo"></param>
    public ProcessExecutorBuilder(SimpleProcessStartInfo startInfo) =>
        _startInfo = startInfo;

    public IProcessExecutorBuilder WithExitCode(int exitCode)
    {
        _expectedExitCode = exitCode;
        return this;
    }

    public IProcessExecutorBuilder WithErrorInterpretation(Encoding? encoding = null)
    {
        _errorEncoding ??= encoding ?? Encoding.UTF8;
        return this;
    }

    public IProcessExecutorBuilder AddErrorWriter(WriteHandler writer)
    {
        _errorTracers.Add(writer);
        return this;
    }

    public IProcessExecutorBuilder AddOutputWriter(WriteHandler writer)
    {
        _outputTracers.Add(writer);
        return this;
    }

    public IProcessExecutorBuilder AddCancellation(IEnumerable<CancellationToken> cancellationTokens)
    {
        _cancellationTokens.AddRange(cancellationTokens);
        return this;
    }

    public IProcessExecutorBuilderArtifact BuildArtifact() => new ProcessExecutorBuilderArtifact(
        new List<CancellationToken>(_cancellationTokens),
        new List<WriteHandler>(_errorTracers),
        new List<WriteHandler>(_outputTracers),
        _startInfo with { },
        _errorEncoding,
        _expectedExitCode);
}
