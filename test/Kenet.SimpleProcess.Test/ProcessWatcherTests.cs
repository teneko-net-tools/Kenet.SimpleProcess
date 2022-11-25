using FluentAssertions;
using Kenet.SimpleProcess.Test.Infrastructure;
using static Kenet.SimpleProcess.Test.Infrastructure.SleepCommand;

namespace Kenet.SimpleProcess.Test
{
    [Collection(KillingProcessesCollection.CollectionName)]
    public class ProcessWatcherTests
    {
        [Fact]
        public async void Watching_already_exited_process_throws()
        {
            using var process = new SimpleProcess(CreateSleepStartInfo());
            process.Run();
            process.Kill();
            process.RunToCompletion();

            using var watcher = new ProcessWatcher(process._process);

            try
            {
                watcher.Watch();
                watcher.IsExited.Should().BeTrue();
            }
            catch (ArgumentException)
            {
                // Only valid in .NET 4.8
                watcher.IsExited.Should().BeFalse();
            }
        }

        [Fact]
        public async void Watching_already_exited_process_passes()
        {
            using var process = new SimpleProcess(CreateSleepStartInfo());
            process.Run();
            process.Kill();
            process.RunToCompletion();

            using var watcher = new ProcessWatcher(process._process);
            watcher.Watch(ProcessWatchOptions.ExitedIfNotFound);
            watcher.IsExited.Should().BeTrue();
        }

        [Fact]
        public void Watching_indicates_exited_after_process_exited()
        {
            using var process = new SimpleProcess(CreateSleepStartInfo());
            process.Run();

            using var watcher = new ProcessWatcher(process._process);
            watcher.Watch();

            process.Kill();
            process.RunToCompletion();
            Thread.Sleep(100);
            watcher.IsExited.Should().BeTrue();
        }
    }
}
