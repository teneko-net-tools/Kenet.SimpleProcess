using FluentAssertions;

namespace Kenet.SimpleProcess.Test
{
    public class GitAsyncLinesTests
    {
        private static SimpleProcessStartInfo CreateGitLogStartInfo(int numberOfCommits) =>
            new("git") {
                Arguments = $"log --oneline -{numberOfCommits}",
                WorkingDirectory = AppContext.BaseDirectory
            };

        [Fact]
        public async Task Execution_should_read_linesAsync()
        {
            const byte lineFeed = 10; // \n
            var expectedNumberOfCommits = 5;
            byte lastByte = 0;

            using var execution = new ProcessExecutorBuilder(CreateGitLogStartInfo(expectedNumberOfCommits))
                .Build()
                .WriteToAsyncLines(x => x.AddOutputWriter, out var asyncLines)
                .AddOutputWriter(bytes => {
                    if (bytes.Length != 0) {
                        lastByte = bytes[^1];
                    }
                })
                .Run();

            var commits = new List<string>();

            await foreach (var line in asyncLines) {
                commits.Add(line);
            }

            commits.Count.Should().Be(expectedNumberOfCommits + 1);
            lastByte.Should().Be(lineFeed);
        }
    }
}
