using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Kenet.SimpleProcess;

/// <summary>
/// A class around the end-of-stream of an output or error stream.
/// </summary>
public static class EOF
{
    internal static ReadOnlyMemory<byte> Memory = new ReadOnlyMemory<byte>(_eof);

    private static byte[] _eof = new byte[0]; // new keyword indicates uniqueness

    /// <summary>
    /// Makes a special comparison to determine whether the span represents the end-of-stream of an output or error stream.
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe bool IsEndOfStreamCore(ReadOnlySpan<byte> span)
    {
        fixed (byte* eofPointer = &MemoryMarshal.GetReference(Memory.Span)) {
            fixed (byte* spanPointer = &MemoryMarshal.GetReference(span)) {
                return eofPointer == spanPointer;
            }
        }
    }

    /// <inheritdoc cref="IsEndOfStreamCore(ReadOnlySpan{byte})"/>
    public static bool IsEndOfStream(this ReadOnlySpan<byte> span) =>
        IsEndOfStreamCore(span);

    /// <inheritdoc cref="IsEndOfStreamCore(ReadOnlySpan{byte})"/>
    public static bool IsEndOfStream(this Span<byte> span) =>
        IsEndOfStreamCore((ReadOnlySpan<byte>)span);
}
