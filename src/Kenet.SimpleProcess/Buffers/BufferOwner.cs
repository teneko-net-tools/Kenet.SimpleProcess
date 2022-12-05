using CommunityToolkit.HighPerformance.Buffers;

namespace Kenet.SimpleProcess.Buffers;

/// <summary>
/// A buffer for which the user bears full responsibility.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct BufferOwner<T> : IDisposable
{
    /// <inheritdoc cref="IBuffer{T}.WrittenMemory" />
    public ReadOnlyMemory<T> WrittenMemory => _bufferWriter?.WrittenMemory ?? ReadOnlyMemory<T>.Empty;

    /// <inheritdoc cref="IBuffer{T}.WrittenSpan" />
    public ReadOnlySpan<T> WrittenSpan => _bufferWriter != null ? _bufferWriter.WrittenSpan : ReadOnlySpan<T>.Empty;

    /// <inheritdoc cref="IBuffer{T}.WrittenCount" />
    public int WrittenCount => _bufferWriter?.WrittenCount ?? 0;

    private readonly ArrayPoolBufferWriter<T> _bufferWriter;

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    /// <param name="bufferWriter"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public BufferOwner(ArrayPoolBufferWriter<T> bufferWriter) =>
        _bufferWriter = bufferWriter ?? throw new ArgumentNullException(nameof(bufferWriter));

    /// <inheritdoc cref="IBuffer{T}.Clear" />
    public void Clear() => _bufferWriter?.Clear();

    /// <inheritdoc />
    public void Dispose() =>
        _bufferWriter?.Dispose();
}
