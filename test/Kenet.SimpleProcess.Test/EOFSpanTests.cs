using System.Runtime.InteropServices;

namespace Kenet.SimpleProcess.Test
{
    public class EOFSpanTests
    {
        [Fact]
        public unsafe void Builtin_empty_span_poiner_is_null()
        {
            fixed (byte* builtinEmptyPtr = &MemoryMarshal.GetReference(Span<byte>.Empty)) {
                if (builtinEmptyPtr == null) {
                    return;
                }
            }

            throw new NotSupportedException();
        }

        [Fact]
        public unsafe void Empty_span_poiner_is_not_null()
        {
            var empty = new byte[0];

            fixed (byte* emptyPtr = &MemoryMarshal.GetReference(new Span<byte>(empty))) {
                if (emptyPtr != null) {
                    return;
                }
            }

            throw new NotSupportedException();
        }

        [Fact]
        public unsafe void Two_different_empty_span_pointers_are_unique()
        {
            var empty1 = new byte[0];
            var empty2 = new byte[0];

            fixed (byte* empty1Ptr = &MemoryMarshal.GetReference(new Span<byte>(empty1))) {
                fixed (byte* empty2Ptr = &MemoryMarshal.GetReference(new Span<byte>(empty2))) {
                    if (empty1Ptr != empty2Ptr) {
                        return;
                    }
                }
            }

            throw new NotSupportedException();
        }

        [Fact]
        public unsafe void Same_empty_span_pointers_are_eqal()
        {
            var empty = new byte[0];

            fixed (byte* empty1Ptr = &MemoryMarshal.GetReference(new Span<byte>(empty))) {
                fixed (byte* empty2Ptr = &MemoryMarshal.GetReference(new Span<byte>(empty))) {
                    // We may use this to distinguish between any empty span and unique EOF
                    if (empty1Ptr == empty2Ptr) {
                        return;
                    }
                }
            }

            throw new NotSupportedException();
        }
    }
}
