using Kenet.SimpleProcess.Test.Infrastructure;
using static Kenet.SimpleProcess.Test.Infrastructure.DummyCommand;

namespace Kenet.SimpleProcess.Test
{
    [Collection(KillingProcessesCollection.CollectionName)]
    public class SleepProcessKillTests
    {
        [Theory]
        [InlineData(new object[] { true })]
        [InlineData(new object[] { false })]
        public async Task Run_to_completion_should_wait_for_process_being_killed(bool synchronously)
        {
            using var sleep = new SimpleProcess(CreateSleepStartInfo());
            sleep.Run();
            sleep.Kill();

            if (synchronously) {
                sleep.RunToCompletion();
            } else {
                await sleep.RunToCompletionAsync();
            }

            sleep.IsExited.Should().BeTrue();
        }

        [Fact]
        public async Task Process_should_not_have_been_exited_when_not_waited_for_completion()
        {
            using var sleep = new SimpleProcess(CreateSleepStartInfo());
            sleep.Run();
            sleep.IsExited.Should().BeFalse();
            sleep.Kill();
            await Task.Delay(100);
            sleep.IsExited.Should().BeTrue();
        }
    }
}
