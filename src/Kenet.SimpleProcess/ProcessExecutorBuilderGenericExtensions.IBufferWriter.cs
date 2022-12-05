using System.Buffers;

namespace Kenet.SimpleProcess;

public static partial class ProcessExecutorBuilderGenericExtensions
{
    public static T WriteTo<T>(
        this T mutator,
        Func<T, Action<WriteHandler>> readFrom,
        IBufferWriter<byte> bufferWriter)
        where T : IProcessExecutorMutator
    {
        readFrom(mutator)(bufferWriter.Write);
        return mutator;
    }

    public static T WriteTo<T>(
        this T mutator,
        Func<T, Func<WriteHandler, object>> readFrom,
        IBufferWriter<byte> bufferWriter)
        where T : IProcessExecutorMutator
    {
        var writerHandler = readFrom(mutator);
        void WriteHandler(WriteHandler bytes) => _ = writerHandler(bytes);
        return WriteTo(mutator, _ => WriteHandler, bufferWriter);
    }
}
