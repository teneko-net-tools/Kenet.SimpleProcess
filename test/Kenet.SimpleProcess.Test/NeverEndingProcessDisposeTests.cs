using FluentAssertions;
using Kenet.SimpleProcess.Test.Infrastructure;
using static Kenet.SimpleProcess.Test.Infrastructure.SleepCommand;

namespace Kenet.SimpleProcess.Test
{
    [Collection(KillingProcessesCollection.CollectionName)]
    public class NeverEndingProcessDisposeTests
    {
        [Theory]
        [InlineData(new object[] { true })]
        [InlineData(new object[] { false })]
        public async Task Process_should_release_run_for_completion_when_disposed(bool synchronously)
        {
            using var sleep = new SimpleProcess(CreateSleepStartInfo());

            _ = Task.Run(async () => {
                await Task.Delay(100);
                sleep.Dispose();
            });

            using var canceller = new CancellationTokenSource(200);

            OperationCanceledException operationCanceledException;

            if (synchronously) {
                operationCanceledException = sleep.Invoking(x => x.RunToCompletion(canceller.Token)).Should().Throw<OperationCanceledException>().And;
            } else {
                operationCanceledException = (await sleep.Awaiting(x => x.RunToCompletionAsync(canceller.Token)).Should().ThrowAsync<OperationCanceledException>()).And;
            }

            operationCanceledException.CancellationToken.Should().NotBe(canceller.Token);
        }
    }
}
