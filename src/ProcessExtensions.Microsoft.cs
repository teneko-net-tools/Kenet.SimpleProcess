// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// https://raw.githubusercontent.com/dotnet/cli/master/test/Microsoft.DotNet.Tools.Tests.Utilities/Extensions/ProcessExtensions.cs

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Kenet.SimpleProcess;

internal partial class ProcessExtensions
{
    private static readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    internal static void KillTree(this Process process, TimeSpan timeout)
    {
        if (_isWindows) {
            RunProcessAndWaitForExit(
                "taskkill",
                $"/T /F /PID {process.Id}",
                timeout,
                out _);
        } else {
            var children = new HashSet<int>();
            GetAllChildIdsUnix(process.Id, children, timeout);

            foreach (var childId in children) {
                KillProcessUnix(childId, timeout);
            }

            KillProcessUnix(process.Id, timeout);
        }
    }

    private static void GetAllChildIdsUnix(int parentId, ISet<int> children, TimeSpan timeout)
    {
        var exitCode = RunProcessAndWaitForExit(
            "pgrep",
            $"-P {parentId}",
            timeout,
            out var stdout);

        if (exitCode == 0 && !string.IsNullOrEmpty(stdout)) {
            using (var reader = new StringReader(stdout)) {
                while (true) {
                    var text = reader.ReadLine();
                    if (text == null) {
                        return;
                    }

                    int id;
                    if (int.TryParse(text, out id)) {
                        children.Add(id);
                        // Recursively get the children
                        GetAllChildIdsUnix(id, children, timeout);
                    }
                }
            }
        }
    }

    private static void KillProcessUnix(int processId, TimeSpan timeout)
    {
        RunProcessAndWaitForExit(
            "kill",
            $"-TERM {processId}",
            timeout,
            out _);
    }

    private static int RunProcessAndWaitForExit(string fileName, string arguments, TimeSpan timeout, out string? stdout)
    {
        var startInfo = new ProcessStartInfo {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Started process should not be null");

        if (process.WaitForExit((int)timeout.TotalMilliseconds)) {
            stdout = process.StandardOutput.ReadToEnd();
        } else {
            process.Kill();
            stdout = null;
        }

        return process.ExitCode;
    }
}
