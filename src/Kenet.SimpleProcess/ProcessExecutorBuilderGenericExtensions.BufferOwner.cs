using System.Buffers;
using CommunityToolkit.HighPerformance.Buffers;
using Kenet.SimpleProcess.Buffers;

namespace Kenet.SimpleProcess;

public static partial class ProcessExecutorBuilderGenericExtensions
{
    /// <summary>
    /// Writes to <paramref name="bufferOwner"/>. The buffer MUST be disposed on your own!
    /// </summary>
    /// <param name="mutator"></param>
    /// <param name="writeToHandlerProvider"></param>
    /// <param name="bufferOwner"></param>
    /// <typeparam name="T"></typeparam>
    public static T WriteToBufferOwner<T>(
        this T mutator,
        Func<T, Action<WriteHandler>> writeToHandlerProvider,
        out BufferOwner<byte> bufferOwner)
        where T : IProcessExecutorMutator
    {
        var buffer = new ArrayPoolBufferWriter<byte>();
        bufferOwner = new BufferOwner<byte>(buffer);
        writeToHandlerProvider(mutator)(buffer.Write);
        return mutator;
    }

    /// <inheritdoc cref="WriteToBufferOwner{T}(T, Func{T, Action{WriteHandler}}, out BufferOwner{byte})"/>
    public static T WriteToBufferOwner<T>(
        this T mutator,
        Func<T, Func<WriteHandler, object>> writeToHandlerProvider,
        out BufferOwner<byte> bufferOwner)
        where T : IProcessExecutorMutator =>
        WriteToBufferOwner(mutator, mutator => writeTo => writeToHandlerProvider(mutator)(writeTo), out bufferOwner);
}
