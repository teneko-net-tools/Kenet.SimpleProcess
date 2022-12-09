using Kenet.SimpleProcess.Test.Infrastructure;
using static Kenet.SimpleProcess.Test.Infrastructure.DummyCommand;

namespace Kenet.SimpleProcess.Test
{
    [Collection(KillingProcessesCollection.CollectionName)]
    public class SleepProcessEOFTests
    {
        [Theory]
        [InlineData(new object[] { true, 0, ProcessCompletionOptions.None })]
        [InlineData(new object[] { false, 0, ProcessCompletionOptions.None })]
        [InlineData(new object[] { true, 30, ProcessCompletionOptions.None })]
        [InlineData(new object[] { false, 30, ProcessCompletionOptions.None })]
        [InlineData(new object[] { true, 0, ProcessCompletionOptions.KillOnCancellation })]
        [InlineData(new object[] { false, 0, ProcessCompletionOptions.KillOnCancellation })]
        [InlineData(new object[] { true, 30, ProcessCompletionOptions.KillTreeOnCancellation })]
        [InlineData(new object[] { false, 30, ProcessCompletionOptions.KillTreeOnCancellation })]
        public async Task Process_writes_eof_when_exiting_after_time(bool synchronously, int cancelledAfterMilliseconds, ProcessCompletionOptions completionOptions)
        {
            CancellationTokenSource? cancellationTokenSource;
            CancellationToken cancellationToken;

            if (cancelledAfterMilliseconds == 0) {
                cancellationTokenSource = null;
                cancellationToken = new CancellationToken(canceled: true);
            } else {
                cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(cancelledAfterMilliseconds));
                cancellationToken = cancellationTokenSource.Token;
            }

            try {
                var receivedEOF = false;

                using var sleep = new SimpleProcess(CreateSleepStartInfo(), cancellationToken) {
                    OutputWriter = _ => receivedEOF = true
                };

                if (synchronously) {
                    sleep.Invoking(x => x.RunToCompletion(completionOptions)).Should().Throw<OperationCanceledException>();
                } else {
                    await sleep.Awaiting(x => x.RunToCompletionAsync(completionOptions)).Should().ThrowAsync<OperationCanceledException>();
                }

                receivedEOF.Should().BeTrue();

                if (completionOptions.HasFlag(ProcessCompletionOptions.KillOnCancellation)) {
                    if (synchronously) {
                        sleep.RunToCompletion(ProcessCompletionOptions.WaitForExit);
                    } else {
                        await sleep.RunToCompletionAsync(ProcessCompletionOptions.WaitForExit);
                    }

                    // Kill was requested
                    sleep.IsExited.Should().BeTrue();
                } else {
                    // Kill was not requested
                    sleep.IsExited.Should().BeFalse();
                }
            } finally {
                cancellationTokenSource?.Dispose();
            }
        }

        [Theory]
        [InlineData(new object[] { true, ProcessCompletionOptions.None })]
        [InlineData(new object[] { false, ProcessCompletionOptions.None })]
        [InlineData(new object[] { true, ProcessCompletionOptions.KillOnCancellation })]
        [InlineData(new object[] { false, ProcessCompletionOptions.KillOnCancellation })]
        public async Task Parallel_processes_write_eof_when_exiting_after_time(bool synchronously, ProcessCompletionOptions completionOptions)
        {
            await Task.WhenAll(Enumerable.Range(0, 15).Select(_ => Process_writes_eof_when_exiting_after_time(synchronously, cancelledAfterMilliseconds: 100, completionOptions)));
        }
    }
}
