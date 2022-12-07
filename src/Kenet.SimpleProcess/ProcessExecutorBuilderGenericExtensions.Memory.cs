namespace Kenet.SimpleProcess;

public static partial class ProcessExecutorBuilderGenericExtensions
{
    public static T WriteToMemory<T>(
        this T mutator,
        Func<T, Action<WriteHandler>> writeToHandlerProvider,
        Memory<byte> memory,
        int startIndex)
        where T : IProcessExecutorMutator
    {
        var writtenBytes = startIndex;

        writeToHandlerProvider(mutator)(bytes => {
            bytes.CopyTo(memory.Span.Slice(writtenBytes, bytes.Length));
            checked { writtenBytes += bytes.Length; }
        });

        return mutator;
    }

    public static T WriteToMemory<T>(
        this T mutator,
        Func<T, Func<WriteHandler, object>> writeToHandlerProvider,
        Memory<byte> memory,
        int startIndex)
        where T : IProcessExecutorMutator =>
        WriteToMemory(mutator, mutator => (Action<WriteHandler>)(writeTo => writeToHandlerProvider(mutator)(writeTo)), memory, startIndex);

    public static T WriteToMemory<T>(
        this T mutator,
        Func<T, Action<WriteHandler>> writeToHandlerProvider,
        Memory<byte> memory)
        where T : IProcessExecutorMutator =>
        mutator.WriteToMemory(writeToHandlerProvider, memory, startIndex: 0);

    public static T WriteToMemory<T>(
        this T mutator,
        Func<T, Func<WriteHandler, object>> writeToHandlerProvider,
        Memory<byte> memory)
        where T : IProcessExecutorMutator =>
        WriteToMemory(mutator, mutator => (Action<WriteHandler>)(writeTo => writeToHandlerProvider(mutator)(writeTo)), memory);
}
