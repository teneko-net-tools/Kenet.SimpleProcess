using System.Buffers;
using CommunityToolkit.HighPerformance.Buffers;

namespace Kenet.SimpleProcess.Execution.GenericExtensions;

public static partial class ProcessExecutorBuilderExtensions
{
    public static T WriteTo<T>(
        this T builder,
        Func<T, Action<WriteHandler>> readFrom,
        IBufferWriter<byte> bufferWriter)
        where T : IProcessExecutorBuilder
    {
        readFrom(builder)(bufferWriter.Write);
        return builder;
    }

    public static T WriteTo<T>(
        this T builder,
        Func<T, Func<WriteHandler, object>> readFrom,
        IBufferWriter<byte> bufferWriter)
        where T : IProcessExecutorBuilder
    {
        var writerHandler = readFrom(builder);
        void WriteHandler(WriteHandler bytes) => _ = writerHandler(bytes);
        return WriteTo(builder, _ => WriteHandler, bufferWriter);
    }

    /// <summary>
    /// Writes to <param name="bufferOwner" />. The buffer MUST be disposed on your own!
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="readFrom"></param>
    /// <param name="bufferOwner"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T WriteTo<T>(
        this T builder,
        Func<T, Action<WriteHandler>> readFrom,
        out BufferOwner<byte> bufferOwner)
        where T : IProcessExecutorBuilder
    {
        var buffer = new ArrayPoolBufferWriter<byte>();
        bufferOwner = new BufferOwner<byte>(buffer);
        readFrom(builder)(buffer.Write);
        return builder;
    }

    /// <summary>
    /// Writes to <param name="bufferOwner" />. The buffer MUST be disposed on your own!
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="readFrom"></param>
    /// <param name="bufferOwner"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T WriteTo<T>(
        this T builder,
        Func<T, Func<WriteHandler, object>> readFrom,
        out BufferOwner<byte> bufferOwner)
        where T : IProcessExecutorBuilder
    {
        var writerHandler = readFrom(builder);
        void WriteHandler(WriteHandler bytes) => _ = writerHandler(bytes);
        return WriteTo(builder, _ => WriteHandler, out bufferOwner);
    }
}
