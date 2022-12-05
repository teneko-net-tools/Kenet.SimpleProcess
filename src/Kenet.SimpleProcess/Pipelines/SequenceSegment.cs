using System.Buffers;
using CommunityToolkit.HighPerformance.Buffers;

namespace Kenet.SimpleProcess.Pipelines
{
    internal sealed class SequenceSegment : ReadOnlySequenceSegment<byte>, IDisposable
    {
        public IMemoryOwner<byte> MemoryOwner { get; }

        public new SequenceSegment? Next {
            get => _next;

            private set {
                base.Next = value;
                _next = value;
            }
        }

        public int Offset { get; private set; }
        public int OffsetRelativeLength { get; private set; }

        private SequenceSegment? _next;
        public readonly int _memoryLength;

        public SequenceSegment(IMemoryOwner<byte> memoryOwner, int memoryLength)
        {
            MemoryOwner = memoryOwner;
            _memoryLength = memoryLength;
            OffsetRelativeLength = memoryLength;
            UpdateMemory();
        }

        public SequenceSegment(IMemoryOwner<byte> memoryOwner)
            : this(memoryOwner, memoryOwner.Memory.Length)
        {
        }

        internal SequenceSegment()
        {
            MemoryOwner = MemoryOwner<byte>.Empty;
            UpdateMemory();
        }

        /// <summary>
        /// Updates the memory with current offset and length.
        /// </summary>
        private void UpdateMemory() =>
            Memory = MemoryOwner.Memory.Slice(Offset, OffsetRelativeLength);

        public ReadOnlySpan<byte> AsSpan(int startIndex, int length)
        {
            var memoryStartIndex = Offset + startIndex;
            var memoryStartEndLength = memoryStartIndex + length;
            return MemoryOwner.Memory.Span.Slice(memoryStartIndex, memoryStartEndLength);
        }

        public ReadOnlySpan<byte> AsSpan(int startIndex) =>
            AsSpan(startIndex, OffsetRelativeLength - startIndex);

        public ReadOnlySpan<byte> AsSpan() =>
            AsSpan(0);

        private void UpdateNextRunningIndexesRecursively()
        {
            var segment = this;
            var nextSegment = Next;

            while (nextSegment != null) {
                nextSegment.RunningIndex = segment.RunningIndex + segment.OffsetRelativeLength;
                segment = nextSegment;
                nextSegment = nextSegment.Next;
            }
        }

        /// <summary>
        /// Sets the new start index and therefore the new <see cref="ReadOnlySequenceSegment{T}.Memory"/> range.
        /// </summary>
        /// <param name="offset">The amount by which <see cref="Offset"/> will be incremented.</param>
        public void IncrementOffset(int offset)
        {
            if (offset == 0) {
                return;
            }

            Offset += offset;
            OffsetRelativeLength -= offset;
            UpdateMemory();
            UpdateNextRunningIndexesRecursively();
        }

        /// <summary>
        /// Sets the new start index and therefore the new <see cref="ReadOnlySequenceSegment{T}.Memory"/> range.
        /// </summary>
        /// <param name="offset">Thw new offset.</param>
        public void UpdateOffset(int offset)
        {
            if (offset.Equals(Offset)) {
                return;
            }

            Offset = offset;
            OffsetRelativeLength = _memoryLength - offset;
            UpdateMemory();
            UpdateNextRunningIndexesRecursively();
        }

        /// <summary>
        /// Sets <paramref name="segment"/> as next segment of THIS segment and updates internal states.
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public void ReplaceNextSegment(SequenceSegment segment)
        {
            Next = segment;
            UpdateNextRunningIndexesRecursively();
        }

        /// <summary>
        /// Disposes until delimiter has been found.
        /// </summary>
        /// <param name="delimiter"></param>
        /// <remarks>
        /// The delimiter itself is not included to be disposed.
        /// </remarks>
        public void DisposeUntil(SequenceSegment? delimiter)
        {
            var segment = this;

            while (delimiter is null || !ReferenceEquals(delimiter, segment)) {
                segment.MemoryOwner.Dispose();
                segment = segment.Next;

                if (segment == null) {
                    break;
                }
            };
        }

        /// <inheritdoc/>
        public void Dispose() =>
            DisposeUntil(delimiter: null);
    }
}
