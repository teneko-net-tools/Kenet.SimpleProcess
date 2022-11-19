namespace Kenet.SimpleProcess.Execution;

/// <summary>
/// The execution result of the process.
/// </summary>
public sealed class ProcessExecutionResult
{
    /// <summary>
    /// The exit code.
    /// </summary>
    public int ExitCode { get; }

    private readonly SimpleProcess _simpleProcess;

    internal ProcessExecutionResult(SimpleProcess simpleProcess, int exitCode)
    {
        ExitCode = exitCode;
        _simpleProcess = simpleProcess;
    }
}
