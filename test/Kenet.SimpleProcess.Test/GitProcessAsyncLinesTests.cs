﻿namespace Kenet.SimpleProcess.Test
{
    public class GitProcessAsyncLinesTests
    {
        private static SimpleProcessStartInfo CreateGitLogStartInfo(int numberOfCommits) => SimpleProcessStartInfo.NewBuilder("git")
            .PasteArguments("log", "--oneline", $"-{numberOfCommits}")
            .WithWorkingDirectory(AppContext.BaseDirectory)
            .Build();

        [Fact]
        public async Task Execution_should_read_lines()
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

        [Fact]
        public async Task Execution_should_read_lines_after_exited()
        {
            var expectedNumberOfCommits = 5;

            _ = await new ProcessExecutorBuilder(CreateGitLogStartInfo(expectedNumberOfCommits))
                .Build()
                .WriteToAsyncLines(x => x.AddOutputWriter, out var asyncLines)
                .RunToCompletionAsync();

            var lines = await asyncLines.ToListAsync();
            lines.Should().HaveCount(expectedNumberOfCommits + 1);
        }
    }
}
