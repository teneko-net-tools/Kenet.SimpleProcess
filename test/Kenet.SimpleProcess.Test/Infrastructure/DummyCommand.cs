using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Kenet.SimpleProcess.Test.Infrastructure
{
    public class DummyCommand : IDisposable
    {
        internal const string DummyProcessName = $"{nameof(Kenet)}.{nameof(Kenet.SimpleProcess)}.{nameof(Dummy)}";

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

        internal static SimpleProcessStartInfo CreateDummyStartInfo() =>
            new(DummyProcessName + GetOSDependentExecutableExtension());

        internal static SimpleProcessStartInfo CreateSleepStartInfo() =>
            CreateDummyStartInfo() with { Arguments = "sleep" };

        internal static SimpleProcessStartInfo CreateWriteStartInfo() =>
            CreateDummyStartInfo() with { Arguments = "write" };

        internal static SimpleProcessStartInfo CreateErrorStartInfo() =>
            CreateDummyStartInfo() with { Arguments = "error" };

        internal static void KillRemainingSleepProcesses()
        {
            foreach (var process in Process.GetProcessesByName(DummyProcessName)) {
                process.Kill();
                process.Dispose();
                Thread.Sleep(100);
            }
        }

        public void Dispose() => KillRemainingSleepProcesses();
    }
}
