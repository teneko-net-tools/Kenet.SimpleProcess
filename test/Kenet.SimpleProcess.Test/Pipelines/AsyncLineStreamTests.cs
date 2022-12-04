using FluentAssertions;
using Kenet.SimpleProcess.Pipelines;

namespace Kenet.SimpleProcess.Test.Pipelines
{
    public class AsyncLineStreamTests
    {
        private const string _n = "\n";
        private const string _r = "\r";
        private const string _rn = "\r\n";

        [Fact]
        public void No_writes_should_result_into_no_lines()
        {
            var reader = new AsyncLineStream();
            reader.Complete();
            reader.WrittenLines.GetConsumingEnumerable().ToList().Should().BeEmpty();
        }

        [Fact]
        public void Writing_empty_string_should_result_into_empty_line()
        {
            var reader = new AsyncLineStream();
            reader.Write(ConsumedMemoryOwner.Of(""));
            reader.Complete();
            reader.WrittenLines.GetConsumingEnumerable().ToList().Should().BeEmpty();
        }

        [Theory]
        [InlineData(new object[] { _n })]
        [InlineData(new object[] { _r })]
        [InlineData(new object[] { _rn })]
        public void Writing_single_newline_should_result_into_two_empty_lines(string newline)
        {
            var reader = new AsyncLineStream();
            reader.Write(ConsumedMemoryOwner.Of(newline));
            reader.Complete();
            reader.WrittenLines.GetConsumingEnumerable().ToList().Should().HaveCount(2);
        }

        [Fact]
        public void Writing_windows_newline_parted_should_result_into_two_empty_lines()
        {
            var reader = new AsyncLineStream();
            reader.Write(ConsumedMemoryOwner.Of(_r));
            reader.Write(ConsumedMemoryOwner.Of(_n));
            reader.Complete();
            reader.WrittenLines.GetConsumingEnumerable().ToList().Should().HaveCount(2);
        }
    }
}
