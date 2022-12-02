using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Kenet.SimpleProcess.Test.Infrastructure
{
    public class WriteCommand : IDisposable
    {
        internal const string WriteProcessName = $"{nameof(Kenet)}.{nameof(Kenet.SimpleProcess)}.{nameof(Write)}";

        private static string GetOSDependentExecutableExtension()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                return ".exe";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                return "";
            }

            throw new NotSupportedException("Unsupported OS plattform");
        }

        internal static SimpleProcessStartInfo CreateWriteStartInfo() =>
            new($"{WriteProcessName}{GetOSDependentExecutableExtension()}");

        internal static void KillRemainingWriteProcesses()
        {
            foreach (var process in Process.GetProcessesByName(WriteProcessName)) {
                process.Kill();
                process.Dispose();
                Thread.Sleep(100);
            }
        }

        public void Dispose() => KillRemainingWriteProcesses();
    }
}
