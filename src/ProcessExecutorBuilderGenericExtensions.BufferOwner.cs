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
    /// <param name="readFrom"></param>
    /// <param name="bufferOwner"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T WriteTo<T>(
        this T mutator,
        Func<T, Action<WriteHandler>> readFrom,
        out BufferOwner<byte> bufferOwner)
        where T : IProcessExecutorMutator
    {
        var buffer = new ArrayPoolBufferWriter<byte>();
        bufferOwner = new BufferOwner<byte>(buffer);
        readFrom(mutator)(buffer.Write);
        return mutator;
    }

    /// <summary>
    /// Writes to <paramref name="bufferOwner" />. The buffer MUST be disposed on your own!
    /// </summary>
    /// <param name="mutator"></param>
    /// <param name="readFrom"></param>
    /// <param name="bufferOwner"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T WriteTo<T>(
        this T mutator,
        Func<T, Func<WriteHandler, object>> readFrom,
        out BufferOwner<byte> bufferOwner)
        where T : IProcessExecutorMutator
    {
        var writerHandler = readFrom(mutator);
        void WriteHandler(WriteHandler bytes) => _ = writerHandler(bytes);
        return WriteTo(mutator, _ => WriteHandler, out bufferOwner);
    }
}
