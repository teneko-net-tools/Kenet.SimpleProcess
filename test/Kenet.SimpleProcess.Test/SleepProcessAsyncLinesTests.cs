using Kenet.SimpleProcess.Test.Infrastructure;
using static Kenet.SimpleProcess.Test.Infrastructure.DummyCommand;

namespace Kenet.SimpleProcess.Test
{
    [Collection(KillingProcessesCollection.CollectionName)]
    public partial class AsyncLinesTests
    {
        [Fact]
        public async Task Awaiting_cancelled_async_lines_should_throw()
        {
            var bufferLines = new List<string>();

            using var sleep = new ProcessExecutorBuilder(CreateSleepStartInfo())
                .Build()
                .WriteToAsyncLines(x => x.AddOutputWriter, out var asyncLines)
                .Run();

            sleep.CancelAfter(100);
            await asyncLines.Awaiting(x => x.ToListAsync()).Should().ThrowAsync<OperationCanceledException>();
        }
    }
}
