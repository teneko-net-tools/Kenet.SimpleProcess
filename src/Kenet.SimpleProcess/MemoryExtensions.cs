using System.Runtime.InteropServices;

namespace Kenet.SimpleProcess;

internal static class MemoryExtensions
{
    /// <summary>
    /// Gets the underlying array of memory.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="memory"></param>
    /// <remarks>Assumes that the memory consists of <b>one single array</b>.</remarks>
    /// <returns>The single array of the memory.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static T[] GetArrayUnsafe<T>(this ReadOnlyMemory<T> memory)
    {
        if (!MemoryMarshal.TryGetArray(memory, out var errorArraySegment)) {
            throw new InvalidOperationException("Memory does not consist of a single array");
        }

        return errorArraySegment.Array!;
    }
}
