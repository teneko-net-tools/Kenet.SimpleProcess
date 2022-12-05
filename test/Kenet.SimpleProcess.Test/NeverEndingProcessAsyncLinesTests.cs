using FluentAssertions;
using Kenet.SimpleProcess.Test.Infrastructure;
using static Kenet.SimpleProcess.Test.Infrastructure.SleepCommand;

namespace Kenet.SimpleProcess.Test
{
    [Collection(KillingProcessesCollection.CollectionName)]
    public class NeverEndingProcessAsyncLinesTests
    {
        [Fact]
        public async Task Awaiting_cancelled_async_lines_should_throw()
        {
            var bufferLines = new List<string>();

            using var sleep = new ProcessExecutorBuilder(CreateSleepStartInfo())
                .Build()
                .WriteToAsyncLines(x => x.AddOutputWriter, out var asyncLines)
                .Run();

            sleep.CancelAfter(50);
            await asyncLines.Awaiting(x => x.ToListAsync()).Should().ThrowAsync<OperationCanceledException>();
        }
    }
}
