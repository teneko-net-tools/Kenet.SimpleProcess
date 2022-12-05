using System.Runtime.Serialization;

namespace Kenet.SimpleProcess;

/// <summary>
/// An exception that indicates a bad exit code.
/// </summary>
public class BadExitCodeException : Exception
{
    internal const string DefaultErrorMessage = "The process exited with a bad exit code";

    /// <summary>
    /// The bad exit code.
    /// </summary>
    public int? ExitCode { get; init; }

    /// <summary>
    /// The process output.
    /// </summary>
    public string? ProcessOutput =>
        _havingProcessOutput ? base.Message : null;

    /// <summary>
    /// Either the process output message, if not <see langword="null"/> or empty, otherwise a default message as fallback without mentioning of the exit code.
    /// </summary>
    public string FallbackMessage =>
        ProcessOutput == null
        ? DefaultErrorMessage + " and outputted \"\" (<null>)"
        : (ProcessOutput.Length == 0
            ? DefaultErrorMessage + " and outputted \"\" (<empty>)"
            : ProcessOutput);

    private readonly bool _havingProcessOutput;

    /// <summary>
    /// The original or fallback message with possible mentioning of the exit code.
    /// </summary>
    public override string Message =>
        FallbackMessage + (ExitCode != null ? $"{Environment.NewLine}Exit Code = {ExitCode}" : string.Empty);

    /// <inheritdoc />
    protected BadExitCodeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    /// <summary>
    /// Creates a bad exit code without process output.
    /// </summary>
    public BadExitCodeException()
    {
    }

    /// <inheritdoc />
    public BadExitCodeException(string? processOutput) : base(processOutput) =>
        _havingProcessOutput = processOutput != null;

    /// <inheritdoc />
    public BadExitCodeException(string? processOutput, Exception? innerException) : base(processOutput, innerException) =>
        _havingProcessOutput = processOutput != null;
}
