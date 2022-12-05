using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Kenet.SimpleProcess.Pipelines;

internal static class ConsumedMemoryOwner
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConsumedMemoryOwner<T> Empty<T>() =>
        ConsumedMemoryOwner<T>.Empty;

    public static ConsumedMemoryOwner<byte> Of(string text, Encoding encoding)
    {
        var charsCount = text.Length;
        var maxBytesCount = encoding.GetMaxByteCount(text.Length);
        var bytesOwner = MemoryPool<byte>.Shared.Rent(encoding.GetMaxByteCount(charsCount));
        int bytesCount;

        unsafe {
            fixed (char* chars = text) {
                fixed (byte* bytes = &MemoryMarshal.GetReference(bytesOwner.Memory.Span)) {
                    bytesCount = encoding.GetBytes(chars, charsCount, bytes, maxBytesCount);
                }
            }
        }

        return new(bytesOwner, bytesCount);
    }

    public static ConsumedMemoryOwner<byte> Of(string text) =>
        Of(text, Encoding.UTF8);
}
