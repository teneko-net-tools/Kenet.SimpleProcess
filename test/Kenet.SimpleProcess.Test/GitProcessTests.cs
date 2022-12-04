using FluentAssertions;

namespace Kenet.SimpleProcess.Test;

public class GitProcessTests
{
    public static IEnumerable<object[]> Git_rev_parse_output_should_be_captured_test_cases()
    {
        yield return new object[]
        {
            new ProcessStartInfoBuilder("git")
            .WithOSIndependentArguments("rev-parse", "--absolute-git-dir")
            .WithWorkingDirectory(AppContext.BaseDirectory)
            .ToDefaultExecutorBuilder()
        };
    }

    [Theory]
    [MemberData(nameof(Git_rev_parse_output_should_be_captured_test_cases))]
    public void Git_rev_parse_output_should_be_captured_synchronously(ProcessExecutorBuilder builder)
    {
        using var boundary = new ProcessBoundary();

        _ = builder
            .WriteToBuffer(b => b.AddOutputWriter, out var buffer, boundary)
            .RunToCompletion();

        buffer.WrittenCount.Should().NotBe(0);
    }

    [Theory]
    [MemberData(nameof(Git_rev_parse_output_should_be_captured_test_cases))]
    public async Task Git_rev_parse_output_should_be_captured_asynchronously(ProcessExecutorBuilder builder)
    {
        using var boundary = new ProcessBoundary();

        _ = await builder
            .WriteToBuffer(b => b.AddOutputWriter, out var buffer, boundary)
            .RunToCompletionAsync();

        buffer.WrittenCount.Should().NotBe(0);
    }
}
