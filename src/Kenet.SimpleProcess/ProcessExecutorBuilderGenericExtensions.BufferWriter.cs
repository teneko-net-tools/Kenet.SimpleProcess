using System.Buffers;

namespace Kenet.SimpleProcess;

public static partial class ProcessExecutorBuilderGenericExtensions
{
    /// <summary>
    /// Writes to <paramref name="bufferWriter"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mutator"></param>
    /// <param name="writeToHandlerProvider"></param>
    /// <param name="bufferWriter"></param>
    public static T WriteToBufferWriter<T>(
        this T mutator,
        Func<T, Action<WriteHandler>> writeToHandlerProvider,
        IBufferWriter<byte> bufferWriter)
        where T : IProcessExecutorMutator
    {
        writeToHandlerProvider(mutator)(bufferWriter.Write);
        return mutator;
    }

    /// <inheritdoc cref="WriteToBufferWriter{T}(T, Func{T, Action{WriteHandler}}, IBufferWriter{byte})"/>
    public static T WriteToBufferWriter<T>(
        this T mutator,
        Func<T, Func<WriteHandler, object>> writeToHandlerProvider,
        IBufferWriter<byte> bufferWriter)
        where T : IProcessExecutorMutator =>
        WriteToBufferWriter(mutator, mutator => (Action<WriteHandler>)(writeTo => writeToHandlerProvider(mutator)(writeTo)), bufferWriter);
}
