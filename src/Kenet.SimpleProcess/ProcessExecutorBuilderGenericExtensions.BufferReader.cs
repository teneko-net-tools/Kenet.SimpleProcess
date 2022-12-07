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
    /// <param name="writeToHandlerProvider"></param>
    /// <param name="bufferReader"></param>
    /// <param name="boundary"></param>
    public static T WriteToBufferReader<T>(
        this T mutator,
        Func<T, Action<WriteHandler>> writeToHandlerProvider,
        out BufferReader<byte> bufferReader,
        ProcessBoundary boundary)
        where T : IProcessExecutorMutator
    {
        var buffer = new ArrayPoolBufferWriter<byte>();
        bufferReader = new BufferReader<byte>(buffer);
        boundary.Associate(buffer);
        writeToHandlerProvider(mutator)(buffer.Write);
        return mutator;
    }

    /// <inheritdoc cref="WriteToBufferReader{T}(T, Func{T, Action{WriteHandler}}, out BufferReader{byte}, ProcessBoundary)"/>
    public static T WriteToBufferReader<T>(
        this T mutator,
        Func<T, Func<WriteHandler, object>> writeToHandlerProvider,
        out BufferReader<byte> bufferReader,
        ProcessBoundary boundary)
        where T : IProcessExecutorMutator =>
        WriteToBufferReader(mutator, mutator => writeTo => writeToHandlerProvider(mutator)(writeTo), out bufferReader, boundary);
}
