using System.Buffers;
using CommunityToolkit.HighPerformance.Buffers;

namespace Kenet.SimpleProcess.Execution.GenericExtensions;

public static partial class ProcessExecutorBuilder
{
    public static T WriteTo<T>(
        this T builder,
        Action<IProcessExecutorBuilder, WriteHandler> readFrom,
        IBufferWriter<byte> bufferWriter)
        where T : IProcessExecutorBuilder
    {
        readFrom(builder, bufferWriter.Write);
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
        Action<IProcessExecutorBuilder, WriteHandler> readFrom,
        out IMemoryOwner<byte> memoryOwner)
        where T : IProcessExecutorBuilder
    {
        var buffer = new ArrayPoolBufferWriter<byte>();
        memoryOwner = buffer;
        readFrom(builder, buffer.Write);
        return builder;
    }
}
