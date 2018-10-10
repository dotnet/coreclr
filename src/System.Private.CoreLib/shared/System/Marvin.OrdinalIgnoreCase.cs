// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
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
        /// Compute a Marvin OrdinalIgnoreCase hash and collapse it into a 32-bit hash.
        /// n.b. <paramref name="count"/> is specified as char count, not byte count.
        /// </summary>
        public static int ComputeHash32OrdinalIgnoreCase(ref char data, int count, uint p0, uint p1)
        {
            nuint ucount = (nuint)count;
            nuint byteOffset = 0;
            uint tempValue;

            while (ucount >= 4)
            {
                tempValue = Unsafe.ReadUnaligned<uint>(ref Unsafe.As<char, byte>(ref Unsafe.AddByteOffset(ref data, byteOffset)));
                if (!Utf16Utility.AllCharsInUInt32AreAscii(tempValue))
                {
                    goto NotAscii;
                }
                p0 += Utf16Utility.ConvertAllAsciiCharsInUInt32ToUppercase(tempValue);
                Block(ref p0, ref p1);

                tempValue = Unsafe.ReadUnaligned<uint>(ref Unsafe.As<char, byte>(ref Unsafe.AddByteOffset(ref data, byteOffset + 4)));
                if (!Utf16Utility.AllCharsInUInt32AreAscii(tempValue))
                {
                    goto NotAsciiSkip2Chars;
                }
                p0 += Utf16Utility.ConvertAllAsciiCharsInUInt32ToUppercase(tempValue);
                Block(ref p0, ref p1);

                byteOffset += 8;
                ucount -= 4;
            }

            switch (ucount)
            {
                case 2:
                    tempValue = Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref Unsafe.As<char, byte>(ref data), byteOffset));
                    if (!Utf16Utility.AllCharsInUInt32AreAscii(tempValue))
                    {
                        goto NotAscii;
                    }
                    p0 += Utf16Utility.ConvertAllAsciiCharsInUInt32ToUppercase(tempValue);
                    Block(ref p0, ref p1);
                    goto case 0;

                case 0:
                    p0 += 0x80u;
                    break;

                case 3:
                    tempValue = Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref Unsafe.As<char, byte>(ref data), byteOffset));
                    if (!Utf16Utility.AllCharsInUInt32AreAscii(tempValue))
                    {
                        goto NotAscii;
                    }
                    p0 += Utf16Utility.ConvertAllAsciiCharsInUInt32ToUppercase(tempValue);
                    byteOffset += 4;
                    Block(ref p0, ref p1);
                    goto case 1;

                case 1:
                    tempValue = Unsafe.AddByteOffset(ref data, byteOffset);
                    if (tempValue > 0x7Fu)
                    {
                        goto NotAscii;
                    }
                    p0 += 0x800000u | Utf16Utility.ConvertAllAsciiCharsInUInt32ToUppercase(tempValue);
                    break;

                default:
                    Debug.Fail("Should not get here.");
                    break;
            }

            Block(ref p0, ref p1);
            Block(ref p0, ref p1);

            return (int)(p1 ^ p0);

        NotAsciiSkip2Chars:
            byteOffset += 4; // in bytes
            ucount -= 2; // in chars

        NotAscii:
            Debug.Assert(0 <= ucount && ucount <= Int32.MaxValue); // this should fit into a signed int
            return ComputeHash32OrdinalIgnoreCaseSlow(ref Unsafe.AddByteOffset(ref data, byteOffset), (int)ucount, p0, p1);
        }

        private static int ComputeHash32OrdinalIgnoreCaseSlow(ref char data, int count, uint p0, uint p1)
        {
            Debug.Assert(count > 0);

            char[] borrowedArr = null;
            Span<char> scratch = (uint)count <= 64 ? stackalloc char[64] : (borrowedArr = ArrayPool<char>.Shared.Rent(count));

            int charsWritten = new ReadOnlySpan<char>(ref data, count).ToUpperInvariant(scratch);
            Debug.Assert(charsWritten == count); // invariant case conversion should involve simple folding; preserve code unit count

            // Slice the array to the size returned by ToUpperInvariant.
            // Multiplication below may overflow, that's fine since it's going to an unsigned integer.
            int hash = ComputeHash32(ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(scratch)), charsWritten * 2, p0, p1);

            // Return the borrowed array if necessary.
            if (borrowedArr != null)
            {
                ArrayPool<char>.Shared.Return(borrowedArr);
            }

            return hash;
        }
    }
}
