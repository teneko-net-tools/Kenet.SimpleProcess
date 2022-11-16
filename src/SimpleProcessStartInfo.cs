﻿using System.Diagnostics;

namespace Kenet.SimpleProcess
{
    /// <summary>
    /// The start info of a process.
    /// </summary>
    public record class SimpleProcessStartInfo
    {
        internal static readonly IReadOnlyDictionary<string, string> EmptyEnvironmentVariables = new Dictionary<string, string>();

        /// <summary>
        /// The executable to start.
        /// </summary>
        public string Executable { get; }

        /// <summary>
        /// The process arguments.
        /// </summary>
        public string? Arguments { get; init; }

        /// <summary>
        /// The working directory is not used to find the executable. Instead, its value applies to the process that is started and only has meaning within the context of the new process.
        /// </summary>
        public string? WorkingDirectory { get; init; }

        /// <summary>
        /// The environment variables that are copied once an instance of <see cref="ProcessStartInfo"/> is created.
        /// </summary>
        public IReadOnlyDictionary<string, string> EnvironmentVariables {
            get => _environmentVariables;
            init => _environmentVariables = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IReadOnlyDictionary<string, string> _environmentVariables = EmptyEnvironmentVariables;

        /// <summary>
        /// Creates an instance of type <see cref="SimpleProcessStartInfo" />.
        /// </summary>
        /// <param name="executable"></param>
        public SimpleProcessStartInfo(string executable) =>
            Executable = executable ?? throw new ArgumentNullException(nameof(executable));

        internal ProcessStartInfo CreateProcessStartInfo()
        {
            var executable = Executable;
            var arguments = Arguments;
            var workingDirectory = WorkingDirectory ?? string.Empty;
            ProcessStartInfo processStartInfo;

            if (arguments == null) {
                processStartInfo = new ProcessStartInfo(executable) {
                    WorkingDirectory = workingDirectory
                };
            } else {
                processStartInfo = new ProcessStartInfo(executable, arguments) {
                    WorkingDirectory = workingDirectory
                };
            }

            if (EnvironmentVariables is not null) {
                foreach (var environmentVariable in EnvironmentVariables) {
                    processStartInfo.EnvironmentVariables.Add(environmentVariable.Key, environmentVariable.Value);
                }
            }

            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.CreateNoWindow = true;
            return processStartInfo;
        }
    }
}
