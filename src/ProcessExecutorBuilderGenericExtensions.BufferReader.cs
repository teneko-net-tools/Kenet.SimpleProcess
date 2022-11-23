using System.Buffers;
using CommunityToolkit.HighPerformance.Buffers;
using Kenet.SimpleProcess.Buffers;

namespace Kenet.SimpleProcess;

public static partial class ProcessExecutorBuilderGenericExtensions
{
    /// <summary>
    /// Writes to <paramref name="bufferReader"/>. The buffer will be associated with <paramref name="boundary"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mutator"></param>
    /// <param name="readFrom"></param>
    /// <param name="bufferReader"></param>
    /// <param name="boundary"></param>
    /// <returns></returns>
    public static T WriteTo<T>(
        this T mutator,
        Func<T, Func<WriteHandler, object>> readFrom,
        out BufferReader<byte> bufferReader,
        ProcessBoundary boundary)
        where T : IProcessExecutorMutator
    {
        var buffer = new ArrayPoolBufferWriter<byte>();
        bufferReader = new BufferReader<byte>(buffer);
        boundary.Associate(buffer);
        readFrom(mutator)(buffer.Write);
        return mutator;
    }
}
