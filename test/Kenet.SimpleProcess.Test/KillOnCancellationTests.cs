using FluentAssertions;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Kenet.SimpleProcess.Test
{
    public class KillOnCancellationTests : IDisposable
    {
        private const string SleepProcessName = $"{nameof(Kenet)}.{nameof(Kenet.SimpleProcess)}.{nameof(Sleep)}";

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

        private SimpleProcessStartInfo CreateSleepStartInfo() => new($"{SleepProcessName}{GetOSDependentExecutableExtension()}");

        [Fact]
        public void Run_to_completion_should_not_kill_process_because_cancellation_requested()
        {
            using SimpleProcess process = new(CreateSleepStartInfo());

            process.Invoking(x => x.RunToCompletion(new CancellationToken(true)))
                .Should().Throw<OperationCanceledException>();

            process.IsExited.Should().BeFalse();
            process.Invoking(x => x.Kill()).Should().NotThrow();
        }

        [Fact]
        public void Run_to_completion_should_kill_process_because_cancellation_requested()
        {
            using SimpleProcess process = new(CreateSleepStartInfo());

            process.Invoking(x => x.RunToCompletion(new CancellationToken(true), ProcessCompletionOptions.KillOnCancellationRequested))
                .Should().Throw<OperationCanceledException>();

            // Process kill happens asynchronously
            Thread.Sleep(30);

            // Process kill triggers exit
            process.IsExited.Should().BeTrue();
        }

        public void Dispose()
        {
            foreach (Process process in Process.GetProcessesByName(SleepProcessName))
            {
                process.Kill();
                process.Dispose();
            }
        }
    }
}
