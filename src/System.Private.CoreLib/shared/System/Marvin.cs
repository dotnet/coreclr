// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Internal.Runtime.CompilerServices;

#if BIT64
using nuint = System.UInt64;
#else
using nuint = System.UInt32;
#endif

namespace System
{
    internal static partial class Marvin
    {
        /// <summary>
        /// Compute a Marvin hash and collapse it into a 32-bit hash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeHash32(ReadOnlySpan<byte> data, ulong seed) => ComputeHash32(ref MemoryMarshal.GetReference(data), data.Length, (uint)seed, (uint)(seed >> 32));

        /// <summary>
        /// Compute a Marvin hash and collapse it into a 32-bit hash.
        /// </summary>
        public static int ComputeHash32(ref byte data, int count, uint p0, uint p1)
        {
            // Control flow of this method generally flows top-to-bottom, trying to
            // minimize the number of branches taken for large (>= 8 bytes, 4 chars) inputs.
            // If small inputs (< 8 bytes, 4 chars) are given, this jumps to a "small inputs"
            // handler at the end of the method.

            if ((uint)count < 8)
            {
                goto InputTooSmallToEnterMainLoop;
            }

            // Main loop - read 8 bytes at a time.
            // The block function is unrolled 2x in this loop.

            uint loopCount = (uint)count / 8;
            Debug.Assert(loopCount > 0, "Shouldn't reach this code path for small inputs.");

            do
            {
                // Most x86 processors have two dispatch ports for reads, so we can read 2x 32-bit
                // values in parallel. We opt for this instead of a single 64-bit read since the
                // typical use case for Marvin32 is computing String hash codes, and the particular
                // layout of String instances means the starting data is never 8-byte aligned when
                // running in a 64-bit process.

                p0 += Unsafe.ReadUnaligned<uint>(ref data);
                uint nextUInt32 = Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref data, 4));

                // One block round for each of the 32-bit integers we just read, 2x rounds total.

                Block(ref p0, ref p1);
                p0 += nextUInt32;
                Block(ref p0, ref p1);

                // Bump the data reference pointer and decrement the loop count.

                // TODO: By decrementing the loop count by 1 every time and comparing against zero,
                // we encourage the JIT to emit "dec test jnz" instead of "add cmp jae". This provides
                // a potential for further optimization: JIT could elide the test instruction entirely
                // since dec sets flags, allowing the x86 architecture to micro-op fuse "dec jnz".

                data = ref Unsafe.AddByteOffset(ref data, 8);
            } while (--loopCount > 0);

            // n.b. We've not been updating the original 'count' parameter, so its actual value is
            // still the original data length. However, we can still rely on its least significant
            // 3 bits to tell us how much data remains (0 .. 7 bytes) after the loop above is
            // completed.

            if ((count & 4) == 0)
            {
                goto DoFinalPartialRead;
            }

        Between4And7BytesRemain:

            // If after finishing the main loop we still have 4 or more leftover bytes, or if we had
            // 4 .. 7 bytes to begin with and couldn't enter the loop in the first place, we need to
            // consume 4 bytes immediately and send them through one round of the block function.

            Debug.Assert((uint)count >= 4, "Only should've gotten here if the original count was >= 4.");

            p0 += Unsafe.ReadUnaligned<uint>(ref data);
            Block(ref p0, ref p1);

        DoFinalPartialRead:

            // Finally, we have 0 .. 3 bytes leftover. Since we know the original data length was at
            // least 4 bytes (smaller lengths are handled at the end of this routine), we can safely
            // read the 4 bytes at the end of the buffer without reading past the beginning of the
            // original buffer. This necessarily means the data we're about to read will overlap with
            // some data we've already processed, but we can handle that below.

            Debug.Assert((uint)count >= 4, "Only should've gotten here if the original count was >= 4.");

            // Read the last 4 bytes of the buffer.

            uint partialResult = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref Unsafe.AddByteOffset(ref data, (nuint)(uint)count & 7), -4));

            // The 'partialResult' local above contains any data we have yet to read, plus some number
            // of bytes which we've already read from the buffer. An example of this is given below
            // for little-endian architectures. In this table, AA BB CC are the bytes which we still
            // need to consume, and ## are bytes which we want to throw away since we've already
            // consumed them as part of a previous read.
            //
            //                                                    (partialResult contains)  (we want it to contain)
            // count mod 4 = 0 -> [ ## ## ## ## |             ] -> 0x########             -> 0x00000080
            // count mod 4 = 1 -> [ ## ## ## ## | AA          ] -> 0xAA######             -> 0x000080AA
            // count mod 4 = 2 -> [ ## ## ## ## | AA BB       ] -> 0xBBAA####             -> 0x0080BBAA
            // count mod 4 = 3 -> [ ## ## ## ## | AA BB CC    ] -> 0xCCBBAA##             -> 0x80CCBBAA

            if (BitConverter.IsLittleEndian)
            {
                partialResult >>= 8; // make some room for the 0x80 byte
                partialResult |= 0x80000000u; // put the 0x80 byte at the beginning
                partialResult >>= (~count << 3) & 0x1F; // shift out all previously consumed bytes
            }
            else
            {
                partialResult <<= 8; // make some room for the 0x80 byte
                partialResult |= 0x80u; // put the 0x80 byte at the end
                partialResult <<= (~count << 3) & 0x1F; // shift out all previously consumed bytes
            }

        DoFinalRoundsAndReturn:

            // Now that we've computed the final partial result, merge it in and run two rounds of
            // the block function to finish out the Marvin algorithm.

            p0 += partialResult;
            Block(ref p0, ref p1);
            Block(ref p0, ref p1);

            return (int)(p1 ^ p0);

        InputTooSmallToEnterMainLoop:

            // We can't run the main loop, but we might still have 4 or more bytes available to us.
            // If so, jump to the 4 .. 7 bytes logic immediately after the main loop.

            if ((count & 4) != 0)
            {
                goto Between4And7BytesRemain;
            }

            // We had only 0 .. 3 bytes to begin with, so we can't perform any 32-bit reads.
            // This means that we're going to be building up the final result right away and
            // will only ever run two rounds total of the block function. Let's initialize
            // the partial result to "no data".

            if (BitConverter.IsLittleEndian)
            {
                partialResult = 0x80u;
            }
            else
            {
                partialResult = 0x80000000u;
            }

            if ((count & 1) != 0)
            {
                // If the buffer is 1 or 3 bytes in length, let's read a single byte now
                // and merge it into our partial result. This will result in partialResult
                // having one of the two values below, where AA BB CC are the buffer bytes.
                // 
                //                  (little-endian / big-endian)
                // [ AA          ]  -> 0x000080AA / 0xAA800000;
                // [ AA BB CC    ]  -> 0x000080CC / 0xCC800000;

                partialResult = Unsafe.AddByteOffset(ref data, (nuint)(uint)count & 2);

                if (BitConverter.IsLittleEndian)
                {
                    partialResult |= 0x8000;
                }
                else
                {
                    partialResult <<= 24;
                    partialResult |= 0x800000u;
                }
            }

            if ((count & 2) != 0)
            {
                // If the buffer is 2 or 3 bytes in length, let's read a single ushort now
                // and merge it into the partial result. This will result in partialResult
                // having one of the two values below, where AA BB CC are the buffer bytes.
                // 
                //                  (little-endian / big-endian)
                // [ AA BB       ]  -> 0x0080BBAA / 0xAABB8000;
                // [ AA BB CC    ]  -> 0x80CCBBAA / 0xAABBCC80; (carried over from above)

                if (BitConverter.IsLittleEndian)
                {
                    partialResult <<= 16;
                    partialResult |= (uint)Unsafe.ReadUnaligned<ushort>(ref data);
                }
                else
                {
                    partialResult |= (uint)Unsafe.ReadUnaligned<ushort>(ref data);
                    partialResult = BitOps.RotateLeft(partialResult, 16);
                }
            }

            // Everything is consumed! Go perform the final rounds and return.

            goto DoFinalRoundsAndReturn;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Block(ref uint rp0, ref uint rp1)
        {
            uint p0 = rp0;
            uint p1 = rp1;

            p1 ^= p0;
            p0 = BitOps.RotateLeft(p0, 20);

            p0 += p1;
            p1 = BitOps.RotateLeft(p1, 9);

            p1 ^= p0;
            p0 = BitOps.RotateLeft(p0, 27);

            p0 += p1;
            p1 = BitOps.RotateLeft(p1, 19);

            rp0 = p0;
            rp1 = p1;
        }

        public static ulong DefaultSeed { get; } = GenerateSeed();

        private static unsafe ulong GenerateSeed()
        {
            ulong seed;
            Interop.GetRandomBytes((byte*)&seed, sizeof(ulong));
            return seed;
        }
    }
}
