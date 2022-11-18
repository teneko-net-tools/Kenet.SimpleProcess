namespace Kenet.SimpleProcess.Execution;

/// <summary>
/// The execution of the process.
/// </summary>
public sealed class ProcessExecution
{
    /// <summary>
    /// The exit code.
    /// </summary>
    public int ExitCode { get; }

    private readonly SimpleProcess _simpleProcess;

    internal ProcessExecution(SimpleProcess simpleProcess, int exitCode)
    {
        ExitCode = exitCode;
        _simpleProcess = simpleProcess;
    }
}
