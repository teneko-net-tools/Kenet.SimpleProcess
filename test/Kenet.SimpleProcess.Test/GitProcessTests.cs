using FluentAssertions;
using Kenet.SimpleProcess.Buffers;

namespace Kenet.SimpleProcess.Test;

public class GitProcessTests
{
    public static IEnumerable<object[]> Git_rev_parse_output_should_be_captured_test_cases()
    {
        yield return new object[]
        {
            ProcessExecutorBuilder.CreateDefault(new SimpleProcessStartInfo("git")
            {
                Arguments = "rev-parse --absolute-git-dir",
                WorkingDirectory = AppContext.BaseDirectory
            })
        };
    }

    [Theory]
    [MemberData(nameof(Git_rev_parse_output_should_be_captured_test_cases))]
    public void Git_rev_parse_output_should_be_captured_synchronously(ProcessExecutorBuilder builder)
    {
        BufferOwner<byte> buffer = default;

        try
        {
            _ = builder
                .WriteTo(b => b.AddOutputWriter, out buffer)
                .RunToCompletion();

            buffer.WrittenCount.Should().NotBe(0);
        }
        finally
        {
            buffer.Dispose();
        }
    }

    [Theory]
    [MemberData(nameof(Git_rev_parse_output_should_be_captured_test_cases))]
    public async Task Git_rev_parse_output_should_be_captured_asynchronously(ProcessExecutorBuilder builder)
    {
        BufferOwner<byte> buffer = default;

        try
        {
            _ = await builder
                .WriteTo(b => b.AddOutputWriter, out buffer)
                .RunToCompletionAsync();

            buffer.WrittenCount.Should().NotBe(0);
        }
        finally
        {
            buffer.Dispose();
        }
    }
}