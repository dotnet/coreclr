// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace System
{
    public static partial class Convert
    {
        private static unsafe void EncodeBase64Avx(byte* input, int inputLength, byte* output, int outputLength)
        {
            // Based on "Base64 encoding with SIMD instructions" article by Wojciech Muła
            // http://0x80.pl/notesen/2016-01-12-sse-base64-encoding.html
            // Encode - https://github.com/WojciechMula/base64simd/blob/master/encode/encode.avx2.cpp
            // Lookup - https://github.com/WojciechMula/base64simd/blob/master/encode/lookup.avx2.cpp (lookup_pshufb_improved)

            byte* outputCurrent = output;
            int stride = 2 * 4 * 3;

            Vector256<byte> shiftLut = Vector256.Create(
                0x47, 0xFC, 0xFC, 0xFC, 0xFC, 0xFC, 0xFC, 0xFC,
                0xFC, 0xFC, 0xFC, 0xED, 0xF0, 0x41, 0x00, 0x00,
                0x47, 0xFC, 0xFC, 0xFC, 0xFC, 0xFC, 0xFC, 0xFC,
                0xFC, 0xFC, 0xFC, 0xED, 0xF0, 0x41, 0x00, 0x00);

            Vector256<byte> asciiToStringMask = Vector256.Create(
                0, 0x80, 1, 0x80, 2, 0x80, 3, 0x80,
                4, 0x80, 5, 0x80, 6, 0x80, 7, 0x80,
                0, 0x80, 1, 0x80, 2, 0x80, 3, 0x80,
                4, 0x80, 5, 0x80, 6, 0x80, 7, 0x80);

            Vector256<byte> shuffleMask = Vector256.Create((byte)
                1, 0, 2, 1, 4, 3, 5, 4, 7, 6, 8, 7, 10, 9, 11, 10,
                1, 0, 2, 1, 4, 3, 5, 4, 7, 6, 8, 7, 10, 9, 11, 10);

            int i = 0;

            for (; i < inputLength - stride - 1; i += stride)
            {
                Vector128<byte> lo = Sse2.LoadVector128(input + i);
                Vector128<byte> hi = Sse2.LoadVector128(input + i + stride / 2);

                Vector256<byte> @in = Avx2.Shuffle(Avx2.InsertVector128(lo.ToVector256(), hi, 1), shuffleMask);

                Vector256<byte>   t0 = Avx2.And(@in, Vector256.Create(0x0fc0fc00).AsByte());
                Vector256<ushort> t1 = Avx2.MultiplyHigh(t0.AsUInt16(), Vector256.Create(0x04000040).AsUInt16());
                Vector256<byte>   t2 = Avx2.And(@in, Vector256.Create(0x003f03f0).AsByte());
                Vector256<ushort> t3 = Avx2.MultiplyLow(t2.AsUInt16(), Vector256.Create(0x01000010).AsUInt16());

                Vector256<ushort> indices = Avx2.Or(t1, t3);
                Vector256<byte> result = LookupPshufb(indices.AsByte(), shiftLut);

                StoreAsTwoByteString(result, asciiToStringMask, outputCurrent);
                outputCurrent += Vector256<byte>.Count * 2;
            }

            // Handle cases when inputLength is not a multiple of 24
            // or it needs '='-paddings (when inputLength % 3 != 0)
            if (i - (inputLength % 3) != inputLength)
            {
                ConvertToBase64Array((char*)outputCurrent, input, i, inputLength - i, false);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<byte> LookupPshufb(Vector256<byte> input, Vector256<byte> shiftLut)
        {
            Vector256<byte> result = Avx2.SubtractSaturate(input, Vector256.Create((byte)51));
            Vector256<sbyte> less =  Avx2.CompareGreaterThan(Vector256.Create((sbyte)26), input.AsSByte());
            result = Avx2.Or(result, Avx2.And(less.AsByte(), Vector256.Create((byte)13)));
            result = Avx2.Shuffle(shiftLut, result.AsByte());
            return Avx2.Add(result, input);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void StoreAsTwoByteString(Vector256<byte> input, Vector256<byte> asciiToStringMask, byte* output)
        {
            // Convert 1-byte string (input) to a 2-byte string
            // E.g. "1,2,3,4,5..." to "1,0,2,0,3,0,4,0,5,0..."
            // I am not sure it's the most efficient way to do it:

            Vector256<byte> permuteLeft = Avx2.Permute4x64(input.AsUInt64(), 0x10 /*_MM_SHUFFLE(0,1,0,0)*/).AsByte();
            Vector256<byte> resultLeft =  Avx2.Shuffle(permuteLeft, asciiToStringMask);

            Vector256<byte> permuteRight = Avx2.Permute4x64(input.AsUInt64(), 0x32 /*_MM_SHUFFLE(0,3,0,2)*/).AsByte();
            Vector256<byte> resultRight =  Avx2.Shuffle(permuteRight, asciiToStringMask);

            Avx.Store(output, resultLeft);
            Avx.Store(output + Vector256<byte>.Count, resultRight);
        }
    }
} 
