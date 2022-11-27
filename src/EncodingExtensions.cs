using System.Runtime.InteropServices;
using System.Text;

namespace Kenet.SimpleProcess;

internal static class EncodingExtensions
{
    public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes, int length)
    {
        fixed (byte* writtenbytes = &MemoryMarshal.GetReference(bytes)) {
            return encoding.GetString(writtenbytes, length);
        }
    }
}
