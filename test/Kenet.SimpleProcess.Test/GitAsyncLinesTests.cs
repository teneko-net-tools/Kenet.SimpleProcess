using FluentAssertions;

namespace Kenet.SimpleProcess.Test
{
    public class GitAsyncLinesTests
    {
        private static SimpleProcessStartInfo CreateGitLogStartInfo(int numberOfCommits) =>
            new("git")
            {
                Arguments = $"log --oneline -{numberOfCommits}",
                WorkingDirectory = AppContext.BaseDirectory
            };

        [Fact]
        public async Task Executor_should_read_lines()
        {
            var expectedNumberOfCommits = 5;

            using var execution = new ProcessExecutorBuilder(CreateGitLogStartInfo(expectedNumberOfCommits))
                .Build()
                .WriteToAsyncLines(x => x.AddOutputWriter, out var lines)
                .Run();

            var actualNumberOfCommits = 0;

            await foreach (var line in lines)
            {
                actualNumberOfCommits++;
            }

            actualNumberOfCommits.Should().Be(expectedNumberOfCommits);
            execution.IsExited.Should().BeTrue();
        }
    }
}
