using FluentAssertions;
using Kenet.SimpleProcess.Test.Infrastructure;
using static Kenet.SimpleProcess.Test.Infrastructure.SleepCommand;

namespace Kenet.SimpleProcess.Test
{
    [Collection(KillingProcessesCollection.CollectionName)]
    public class NeverEndingProcessKillTests
    {
        [Theory]
        [InlineData(new object[] { true })]
        [InlineData(new object[] { false })]
        public async Task Run_to_completion_should_wait_for_process_being_killedAsync(bool synchronously)
        {
            using var sleep = new SimpleProcess(CreateSleepStartInfo());
            sleep.Run();
            sleep.Kill();

            if (synchronously) {
                await sleep.RunToCompletionAsync();
            } else {
                await sleep.RunToCompletionAsync();
            }

            sleep.IsExited.Should().BeTrue();
        }

        [Fact]
        public void Process_should_not_have_been_exited_when_not_waited_for_completion()
        {
            using var sleep = new SimpleProcess(CreateSleepStartInfo());
            sleep.Run();
            sleep.Kill();
            sleep.IsExited.Should().BeFalse();
        }
    }
}
