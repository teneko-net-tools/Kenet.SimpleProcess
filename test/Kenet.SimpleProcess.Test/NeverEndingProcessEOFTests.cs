using System.Text;
using FluentAssertions;
using Kenet.SimpleProcess.Test.Infrastructure;
using static Kenet.SimpleProcess.Test.Infrastructure.WriteCommand;

namespace Kenet.SimpleProcess.Test
{
    [Collection(KillingProcessesCollection.CollectionName)]
    public class NeverEndingProcessEOFTests
    {
        private const int CancelAfter = 1000 * 10;

        [Fact]
        public async Task Process_written_fragments_are_correct()
        {
            var bufferBlocks = new List<string>();
            var numberOfBytes = 0;
            var reachedEOF = false;

            using var sleep = new SimpleProcess(CreateWriteStartInfo()) {
                OutputWriter = bytes => {
                    if (bytes.IsEndOfStream()) {
                        reachedEOF = bytes.IsEndOfStream();
                    } else {
                        numberOfBytes += bytes.Length;
                        bufferBlocks.Add(Encoding.UTF8.GetString(bytes.ToArray()));
                    }
                }
            };

            sleep.CancelAfter(CancelAfter);
            await sleep.RunToCompletionAsync();

            string.Concat(bufferBlocks).Should().Be($"{Environment.NewLine}\0Hello");
            reachedEOF.Should().BeTrue();
        }

        [Fact]
        public async Task Process_written_lines_are_correct()
        {
            var bufferLines = new List<string>();

            using var sleep = new ProcessExecutorBuilder(CreateWriteStartInfo())
                .Build()
                .WriteToAsyncLines(x => x.AddOutputWriter, out var asyncLines)
                .Run();

            sleep.CancelAfter(CancelAfter);

            await foreach (var line in asyncLines) {
                bufferLines.Add(line);
            }

            bufferLines.Count.Should().Be(2);
            bufferLines[0].Should().Be($"");
            bufferLines[1].Should().Be($"\0Hello");
        }
    }
}
