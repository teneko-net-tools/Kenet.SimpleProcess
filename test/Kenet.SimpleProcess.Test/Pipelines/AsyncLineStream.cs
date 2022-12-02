using System.Buffers;
using System.Text;
using FluentAssertions;
using Kenet.SimpleProcess.Pipelines;

namespace Kenet.SimpleProcess.Test.Pipelines
{
    public class AsyncLineStreamTests
    {
        [Fact]
        public void No_writes_should_result_into_no_lines()
        {
            var reader = new AsyncLineStream();
            reader.Complete();
            reader.WrittenLines.GetConsumingEnumerable().ToList().Should().BeEmpty();
        }

        [Fact]
        public void Written_empty_should_result_into_empty_line()
        {
            var reader = new AsyncLineStream();
            reader.Write(new ConsumedMemoryOwner<byte>(StringMemoryOwner.Of("")));
            reader.Complete();
            reader.WrittenLines.GetConsumingEnumerable().ToList().Should().BeEmpty();
        }

        [Fact]
        public void Written_newline_should_result_into_two_empty_lines()
        {
            var reader = new AsyncLineStream();
            reader.Write(new ConsumedMemoryOwner<byte>(StringMemoryOwner.Of(Environment.NewLine)));
            reader.Complete();
            reader.WrittenLines.GetConsumingEnumerable().ToList().Should().HaveCount(2);
        }

        private class StringMemoryOwner : IMemoryOwner<byte>
        {
            public static StringMemoryOwner Of(string text) =>
                new(text);

            public Memory<byte> Memory { get; }

            public StringMemoryOwner(string text) =>
                Memory = new Memory<byte>(Encoding.UTF8.GetBytes(text));

            public void Dispose()
            {
            }
        }
    }
}
