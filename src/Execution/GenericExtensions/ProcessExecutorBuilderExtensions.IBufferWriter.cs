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
    /// Writes to <param name="memoryOwner" />. The memory MUST be released on your own!
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="readFrom"></param>
    /// <param name="memoryOwner"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T WriteTo<T>(
        this T builder,
        Func<T, Action<WriteHandler>> readFrom,
        out IMemoryOwner<byte> memoryOwner)
        where T : IProcessExecutorBuilder
    {
        var buffer = new ArrayPoolBufferWriter<byte>();
        memoryOwner = buffer;
        readFrom(builder)(buffer.Write);
        return builder;
    }

    /// <summary>
    /// Writes to <param name="memoryOwner" />. The memory MUST be released on your own!
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="readFrom"></param>
    /// <param name="memoryOwner"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T WriteTo<T>(
        this T builder,
        Func<T, Func<WriteHandler, object>> readFrom,
        out IMemoryOwner<byte> memoryOwner)
        where T : IProcessExecutorBuilder
    {
        var writerHandler = readFrom(builder);
        void WriteHandler(WriteHandler bytes) => _ = writerHandler(bytes);
        return WriteTo(builder, _ => WriteHandler, out memoryOwner);
    }
}
