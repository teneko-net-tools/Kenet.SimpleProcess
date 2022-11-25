using FluentAssertions;
using Kenet.SimpleProcess.Test.Infrastructure;
using static Kenet.SimpleProcess.Test.Infrastructure.SleepCommand;

namespace Kenet.SimpleProcess.Test
{
    [Collection(KillingProcessesCollection.CollectionName)]
    public class NeverEndingProcessCancellationTests
    {
        [Theory]
        [InlineData(new object[] { true })]
        [InlineData(new object[] { false })]
        public async Task Process_should_cancel_completion_after_process_cancellation(bool synchronously)
        {
            using var sleep = new SimpleProcess(CreateSleepStartInfo());
            var runTask = synchronously
                ? Task.Run(() => sleep.RunToCompletion())
                : sleep.RunToCompletionAsync();
            sleep.Cancel();
                await runTask.Awaiting(x => x).Should().ThrowAsync<OperationCanceledException>();
        }
    }
}
