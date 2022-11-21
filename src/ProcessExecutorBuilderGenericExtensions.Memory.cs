namespace Kenet.SimpleProcess;

public static partial class ProcessExecutorBuilderGenericExtensions
{
    public static T WriteTo<T>(
        this T mutator,
        Func<T, Action<WriteHandler>> readFrom,
        Memory<byte> memory,
        int startIndex)
        where T : IProcessExecutorMutator
    {
        var writtenBytes = startIndex;

        readFrom(mutator)(bytes => {
            bytes.CopyTo(memory.Span.Slice(writtenBytes, bytes.Length));
            checked { writtenBytes += bytes.Length; }
        });

        return mutator;
    }

    public static T WriteTo<T>(
        this T mutator,
        Func<T, Func<WriteHandler, object>> readFrom,
        Memory<byte> memory,
        int startIndex)
        where T : IProcessExecutorMutator
    {
        var writerHandler = readFrom(mutator);
        void WriteHandler(WriteHandler bytes) => _ = writerHandler(bytes);
        return WriteTo(mutator, _ => WriteHandler, memory, startIndex);
    }

    public static T WriteTo<T>(
        this T mutator,
        Func<T, Action<WriteHandler>> readFrom,
        Memory<byte> memory)
        where T : IProcessExecutorMutator =>
        mutator.WriteTo(readFrom, memory, startIndex: 0);

    public static T WriteTo<T>(
        this T mutator,
        Func<T, Func<WriteHandler, object>> readFrom,
        Memory<byte> memory)
        where T : IProcessExecutorMutator
    {
        var writerHandler = readFrom(mutator);
        void WriteHandler(WriteHandler bytes) => _ = writerHandler(bytes);
        return WriteTo(mutator, _ => WriteHandler, memory);
    }
}
