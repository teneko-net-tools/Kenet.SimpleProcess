using System.Diagnostics.CodeAnalysis;

namespace Kenet.SimpleProcess;

/// <summary>
/// A builder for an instance of <see cref="SimpleProcessStartInfo"/>.
/// </summary>
public class ProcessStartInfoBuilder
{
    private string _executable;
    private string? _arguments;
    private Dictionary<string, string>? _environmentVariables;
    private string? _workingDirectory;

    /// <summary>
    /// Creates an instance of this type with the specified executable.
    /// </summary>
    /// <param name="executable"></param>
    public ProcessStartInfoBuilder(string executable) =>
        _executable = executable;

    /// <summary>
    /// Sets the arguments that are gonna passed 1:1 to the target process.
    /// </summary>
    /// <param name="escapedArguments"></param>
    /// <remarks>
    /// <paramref name="escapedArguments"/> must be properly escaped for the intended OS plattform.
    /// Please consider to use <see cref="ProcessStartInfoBuilderExtensions.WithOSIndependentArguments(ProcessStartInfoBuilder, string[])"/>, which handles the argument escaping for you.
    /// </remarks>
    public ProcessStartInfoBuilder WithArguments(string escapedArguments)
    {
        _arguments = escapedArguments;
        return this;
    }

    /// <summary>
    /// Sets the working directory.
    /// </summary>
    /// <param name="workingDirectory"></param>
    public ProcessStartInfoBuilder WithWorkingDirectory(string workingDirectory)
    {
        _workingDirectory = workingDirectory;
        return this;
    }

    [MemberNotNull(nameof(_environmentVariables))]
    private void EnsureHavingEnvironmentVariableDictionary()
    {
        if (_environmentVariables is null) {
            _environmentVariables = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Adds a single environment variable.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public ProcessStartInfoBuilder AddEnvironmentVariable(string key, string value)
    {
        EnsureHavingEnvironmentVariableDictionary();
        _environmentVariables[key] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple environment variables.
    /// </summary>
    /// <param name="environmentVariables"></param>
    /// <returns></returns>
    public ProcessStartInfoBuilder AddEnvironmentVariables(IReadOnlyDictionary<string, string> environmentVariables)
    {
        EnsureHavingEnvironmentVariableDictionary();

        foreach (var environmentVariable in environmentVariables) {
            _environmentVariables[environmentVariable.Key] = environmentVariable.Value;
        }

        return this;
    }

    /// <summary>
    /// Builds an instance of <see cref="SimpleProcessStartInfo"/>.
    /// </summary>
    public SimpleProcessStartInfo Build() => new SimpleProcessStartInfo(_executable) {
        Arguments = _arguments,
        EnvironmentVariables = _environmentVariables,
        WorkingDirectory = _workingDirectory
    };
}
