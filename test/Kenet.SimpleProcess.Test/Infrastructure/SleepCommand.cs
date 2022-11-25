using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Kenet.SimpleProcess.Test.Infrastructure
{
    public class SleepCommand : IDisposable
    {
        internal const string SleepProcessName = $"{nameof(Kenet)}.{nameof(Kenet.SimpleProcess)}.{nameof(Sleep)}";

        private static string GetOSDependentExecutableExtension()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ".exe";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "";
            }

            throw new NotSupportedException("Unsupported OS plattform");
        }

        internal static SimpleProcessStartInfo CreateSleepStartInfo() =>
            new($"{SleepProcessName}{GetOSDependentExecutableExtension()}");

        internal static void KillRemainingSleepProcesses()
        {
            foreach (Process process in Process.GetProcessesByName(SleepProcessName))
            {
                process.Kill();
                process.Dispose();
                Thread.Sleep(100);
            }
        }

        public void Dispose() => KillRemainingSleepProcesses();
    }
}
