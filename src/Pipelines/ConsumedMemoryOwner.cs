using System.Buffers;
using CommunityToolkit.HighPerformance.Buffers;

namespace Kenet.SimpleProcess.Pipelines;

internal readonly struct ConsumedMemoryOwner<T> : IMemoryOwner<T>
{
    public static readonly ConsumedMemoryOwner<T> Empty = new ConsumedMemoryOwner<T>(MemoryOwner<T>.Empty, 0);

    public Memory<T> Memory =>
        _memoryOwner.Memory;

    public Memory<T> ConsumedMemory =>
        _memoryOwner.Memory.Slice(0, Consumed);

    /// <summary>
    /// How much has been consumed.
    /// </summary>
    public int Consumed { get; }

    private readonly IMemoryOwner<T> _memoryOwner;

    public ConsumedMemoryOwner(IMemoryOwner<T> memoryOwner, int consumed)
    {
        _memoryOwner = memoryOwner ?? throw new ArgumentNullException(nameof(memoryOwner));

        if (consumed < 0) {
            throw new ArgumentOutOfRangeException("Consumed amount cannot be lesser than zero");
        } else if (consumed > memoryOwner.Memory.Length) {
            throw new ArgumentOutOfRangeException("Consumed amount cannot be greater than the memory is long");
        }

        Consumed = consumed;
    }
    public ConsumedMemoryOwner(IMemoryOwner<T> memoryOwner)
        : this(memoryOwner, memoryOwner.Memory.Length)
    {
    }

    public void Dispose() =>
        _memoryOwner.Dispose();
}

