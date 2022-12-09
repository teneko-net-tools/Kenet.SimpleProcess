using Kenet.SimpleProcess.Pipelines;

namespace Kenet.SimpleProcess.Test.Pipelines
{
    public class AsyncLineStreamTests
    {
        private const string N = "\n";
        private const string R = "\r";
        private const string RN = "\r\n";

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
        [InlineData(new object[] { N })]
        [InlineData(new object[] { R })]
        [InlineData(new object[] { RN })]
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
            reader.Write(ConsumedMemoryOwner.Of(R));
            reader.Write(ConsumedMemoryOwner.Of(N));
            reader.Complete();
            reader.WrittenLines.GetConsumingEnumerable().ToList().Should().HaveCount(2);
        }
    }
}
