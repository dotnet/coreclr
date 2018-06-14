// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.Intrinsics;

namespace System.Runtime.Intrinsics.X86
{
    /// <summary>
    /// This class provides access to Intel SSE4.2 hardware instructions via intrinsics
    /// </summary>
    [CLSCompliant(false)]
    public static class Sse42
    {
        public static bool IsSupported { get => IsSupported; }

        /// <summary>
        /// int _mm_cmpistra (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareNoMatchAndRightNotTerminated(Vector128<sbyte> left, Vector128<sbyte> right, StringComparisonMode mode) => CompareNoMatchAndRightNotTerminated(left, right, mode);

        /// <summary>
        /// int _mm_cmpistra (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareNoMatchAndRightNotTerminated(Vector128<byte> left, Vector128<byte> right, StringComparisonMode mode) => CompareNoMatchAndRightNotTerminated(left, right, mode);

        /// <summary>
        /// int _mm_cmpistra (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareNoMatchAndRightNotTerminated(Vector128<short> left, Vector128<short> right, StringComparisonMode mode) => CompareNoMatchAndRightNotTerminated(left, right, mode);

        /// <summary>
        /// int _mm_cmpistra (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareNoMatchAndRightNotTerminated(Vector128<ushort> left, Vector128<ushort> right, StringComparisonMode mode) => CompareNoMatchAndRightNotTerminated(left, right, mode);

        /// <summary>
        /// int _mm_cmpistrc (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareHasMatch(Vector128<sbyte> left, Vector128<sbyte> right, StringComparisonMode mode) => CompareHasMatch(left, right, mode);

        /// <summary>
        /// int _mm_cmpistra (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareHasMatch(Vector128<byte> left, Vector128<byte> right, StringComparisonMode mode) => CompareHasMatch(left, right, mode);

        /// <summary>
        /// int _mm_cmpistra (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareHasMatch(Vector128<short> left, Vector128<short> right, StringComparisonMode mode) => CompareHasMatch(left, right, mode);

        /// <summary>
        /// int _mm_cmpistra (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareHasMatch(Vector128<ushort> left, Vector128<ushort> right, StringComparisonMode mode) => CompareHasMatch(left, right, mode);

        /// <summary>
        /// int _mm_cmpistro (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareReturnFirstResultBit(Vector128<sbyte> left, Vector128<sbyte> right, StringComparisonMode mode) => CompareReturnFirstResultBit(left, right, mode);

        /// <summary>
        /// int _mm_cmpistro (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareReturnFirstResultBit(Vector128<byte> left, Vector128<byte> right, StringComparisonMode mode) => CompareReturnFirstResultBit(left, right, mode);

        /// <summary>
        /// int _mm_cmpistro (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareReturnFirstResultBit(Vector128<short> left, Vector128<short> right, StringComparisonMode mode) => CompareReturnFirstResultBit(left, right, mode);

        /// <summary>
        /// int _mm_cmpistro (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareReturnFirstResultBit(Vector128<ushort> left, Vector128<ushort> right, StringComparisonMode mode) => CompareReturnFirstResultBit(left, right, mode);

        /// <summary>
        /// int _mm_cmpistrs (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareLeftTerminated(Vector128<sbyte> left, Vector128<sbyte> right, StringComparisonMode mode) => CompareLeftTerminated(left, right, mode);

        /// <summary>
        /// int _mm_cmpistrs (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareLeftTerminated(Vector128<byte> left, Vector128<byte> right, StringComparisonMode mode) => CompareLeftTerminated(left, right, mode);

        /// <summary>
        /// int _mm_cmpistrs (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareLeftTerminated(Vector128<short> left, Vector128<short> right, StringComparisonMode mode) => CompareLeftTerminated(left, right, mode);

        /// <summary>
        /// int _mm_cmpistrs (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareLeftTerminated(Vector128<ushort> left, Vector128<ushort> right, StringComparisonMode mode) => CompareLeftTerminated(left, right, mode);

        /// <summary>
        /// int _mm_cmpistrz (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareRightTerminated(Vector128<sbyte> left, Vector128<sbyte> right, StringComparisonMode mode) => CompareRightTerminated(left, right, mode);

        /// <summary>
        /// int _mm_cmpistrz (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareRightTerminated(Vector128<byte> left, Vector128<byte> right, StringComparisonMode mode) => CompareRightTerminated(left, right, mode);

        /// <summary>
        /// int _mm_cmpistrz (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareRightTerminated(Vector128<short> left, Vector128<short> right, StringComparisonMode mode) => CompareRightTerminated(left, right, mode);

        /// <summary>
        /// int _mm_cmpistrz (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareRightTerminated(Vector128<ushort> left, Vector128<ushort> right, StringComparisonMode mode) => CompareRightTerminated(left, right, mode);

        /// <summary>
        /// int _mm_cmpestra (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareNoMatchAndRightNotTerminated(Vector128<sbyte> left, int leftLength, Vector128<sbyte> right, int rightLength, StringComparisonMode mode) => CompareNoMatchAndRightNotTerminated(left, leftLength, right, rightLength, mode);
        
        /// <summary>
        /// int _mm_cmpestra (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareNoMatchAndRightNotTerminated(Vector128<byte> left, int leftLength, Vector128<byte> right, int rightLength, StringComparisonMode mode) => CompareNoMatchAndRightNotTerminated(left, leftLength, right, rightLength, mode);
        
        /// <summary>
        /// int _mm_cmpestra (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareNoMatchAndRightNotTerminated(Vector128<short> left, int leftLength, Vector128<short> right, int rightLength, StringComparisonMode mode) => CompareNoMatchAndRightNotTerminated(left, leftLength, right, rightLength, mode);
        
        /// <summary>
        /// int _mm_cmpestra (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareNoMatchAndRightNotTerminated(Vector128<ushort> left, int leftLength, Vector128<ushort> right, int rightLength, StringComparisonMode mode) => CompareNoMatchAndRightNotTerminated(left, leftLength, right, rightLength, mode);

        /// <summary>
        /// int _mm_cmpestrc (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareHasMatch(Vector128<sbyte> left, int leftLength, Vector128<sbyte> right, int rightLength, StringComparisonMode mode) => CompareHasMatch(left, leftLength, right, rightLength, mode);
        
        /// <summary>
        /// int _mm_cmpestrc (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareHasMatch(Vector128<byte> left, int leftLength, Vector128<byte> right, int rightLength, StringComparisonMode mode) => CompareHasMatch(left, leftLength, right, rightLength, mode);
        
        /// <summary>
        /// int _mm_cmpestrc (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareHasMatch(Vector128<short> left, int leftLength, Vector128<short> right, int rightLength, StringComparisonMode mode) => CompareHasMatch(left, leftLength, right, rightLength, mode);
        
        /// <summary>
        /// int _mm_cmpestrc (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareHasMatch(Vector128<ushort> left, int leftLength, Vector128<ushort> right, int rightLength, StringComparisonMode mode) => CompareHasMatch(left, leftLength, right, rightLength, mode);

        /// <summary>
        /// int _mm_cmpestro (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareReturnFirstResultBit(Vector128<sbyte> left, int leftLength, Vector128<sbyte> right, int rightLength, StringComparisonMode mode) => CompareReturnFirstResultBit(left, leftLength, right, rightLength, mode);
        
        /// <summary>
        /// int _mm_cmpestro (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareReturnFirstResultBit(Vector128<byte> left, int leftLength, Vector128<byte> right, int rightLength, StringComparisonMode mode) => CompareReturnFirstResultBit(left, leftLength, right, rightLength, mode);
        
        /// <summary>
        /// int _mm_cmpestro (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareReturnFirstResultBit(Vector128<short> left, int leftLength, Vector128<short> right, int rightLength, StringComparisonMode mode) => CompareReturnFirstResultBit(left, leftLength, right, rightLength, mode);
        
        /// <summary>
        /// int _mm_cmpestro (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareReturnFirstResultBit(Vector128<ushort> left, int leftLength, Vector128<ushort> right, int rightLength, StringComparisonMode mode) => CompareReturnFirstResultBit(left, leftLength, right, rightLength, mode);

        /// <summary>
        /// int _mm_cmpestrs (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareLeftTerminated(Vector128<sbyte> left, int leftLength, Vector128<sbyte> right, int rightLength, StringComparisonMode mode) => CompareLeftTerminated(left, leftLength, right, rightLength, mode);
        
        /// <summary>
        /// int _mm_cmpestrs (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareLeftTerminated(Vector128<byte> left, int leftLength, Vector128<byte> right, int rightLength, StringComparisonMode mode) => CompareLeftTerminated(left, leftLength, right, rightLength, mode);
        
        /// <summary>
        /// int _mm_cmpestrs (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareLeftTerminated(Vector128<short> left, int leftLength, Vector128<short> right, int rightLength, StringComparisonMode mode) => CompareLeftTerminated(left, leftLength, right, rightLength, mode);
        
        /// <summary>
        /// int _mm_cmpestrs (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareLeftTerminated(Vector128<ushort> left, int leftLength, Vector128<ushort> right, int rightLength, StringComparisonMode mode) => CompareLeftTerminated(left, leftLength, right, rightLength, mode);

        /// <summary>
        /// int _mm_cmpestrz (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareRightTerminated(Vector128<sbyte> left, int leftLength, Vector128<sbyte> right, int rightLength, StringComparisonMode mode) => CompareRightTerminated(left, leftLength, right, rightLength, mode);
        
        /// <summary>
        /// int _mm_cmpestrz (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareRightTerminated(Vector128<byte> left, int leftLength, Vector128<byte> right, int rightLength, StringComparisonMode mode) => CompareRightTerminated(left, leftLength, right, rightLength, mode);
        
        /// <summary>
        /// int _mm_cmpestrz (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareRightTerminated(Vector128<short> left, int leftLength, Vector128<short> right, int rightLength, StringComparisonMode mode) => CompareRightTerminated(left, leftLength, right, rightLength, mode);
        
        /// <summary>
        /// int _mm_cmpestrz (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static bool CompareRightTerminated(Vector128<ushort> left, int leftLength, Vector128<ushort> right, int rightLength, StringComparisonMode mode) => CompareRightTerminated(left, leftLength, right, rightLength, mode);

        /// <summary>
        /// int _mm_cmpistri (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static int CompareIndex(Vector128<sbyte> left, Vector128<sbyte> right, StringComparisonMode mode) => CompareIndex(left, right, mode);

        /// <summary>
        /// int _mm_cmpistri (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static int CompareIndex(Vector128<byte> left, Vector128<byte> right, StringComparisonMode mode) => CompareIndex(left, right, mode);

        /// <summary>
        /// int _mm_cmpistri (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static int CompareIndex(Vector128<short> left, Vector128<short> right, StringComparisonMode mode) => CompareIndex(left, right, mode);

        /// <summary>
        /// int _mm_cmpistri (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRI xmm, xmm/m128, imm8
        /// </summary>
        public static int CompareIndex(Vector128<ushort> left, Vector128<ushort> right, StringComparisonMode mode) => CompareIndex(left, right, mode);

        /// <summary>
        /// int _mm_cmpestri (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static int CompareIndex(Vector128<sbyte> left, int leftLength, Vector128<sbyte> right, int rightLength, StringComparisonMode mode) => CompareIndex(left, leftLength, right, rightLength, mode);

        /// <summary>
        /// int _mm_cmpestri (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static int CompareIndex(Vector128<byte> left, int leftLength, Vector128<byte> right, int rightLength, StringComparisonMode mode) => CompareIndex(left, leftLength, right, rightLength, mode);

        /// <summary>
        /// int _mm_cmpestri (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static int CompareIndex(Vector128<short> left, int leftLength, Vector128<short> right, int rightLength, StringComparisonMode mode) => CompareIndex(left, leftLength, right, rightLength, mode);

        /// <summary>
        /// int _mm_cmpestri (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRI xmm, xmm/m128, imm8
        /// </summary>
        public static int CompareIndex(Vector128<ushort> left, int leftLength, Vector128<ushort> right, int rightLength, StringComparisonMode mode) => CompareIndex(left, leftLength, right, rightLength, mode);

        /// <summary>
        /// __m128i _mm_cmpistrm (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRM xmm, xmm/m128, imm8
        /// </summary>
        public static Vector128<sbyte> CompareMask(Vector128<sbyte> left, Vector128<sbyte> right, StringComparisonMode mode) => CompareMask(left, right, mode);

        /// <summary>
        /// __m128i _mm_cmpistrm (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRM xmm, xmm/m128, imm8
        /// </summary>
        public static Vector128<byte> CompareMask(Vector128<byte> left, Vector128<byte> right, StringComparisonMode mode) => CompareMask(left, right, mode);

        /// <summary>
        /// __m128i _mm_cmpistrm (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRM xmm, xmm/m128, imm8
        /// </summary>
        public static Vector128<short> CompareMask(Vector128<short> left, Vector128<short> right, StringComparisonMode mode) => CompareMask(left, right, mode);

        /// <summary>
        /// __m128i _mm_cmpistrm (__m128i a, __m128i b, const int imm8)
        ///   PCMPISTRM xmm, xmm/m128, imm8
        /// </summary>
        public static Vector128<ushort> CompareMask(Vector128<ushort> left, Vector128<ushort> right, StringComparisonMode mode) => CompareMask(left, right, mode);

        /// <summary>
        /// __m128i _mm_cmpestrm (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRM xmm, xmm/m128, imm8
        /// </summary>
        public static Vector128<sbyte> CompareMask(Vector128<sbyte> left, int leftLength, Vector128<sbyte> right, int rightLength, StringComparisonMode mode) => CompareMask(left, leftLength, right, rightLength, mode);

        /// <summary>
        /// __m128i _mm_cmpestrm (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRM xmm, xmm/m128, imm8
        /// </summary>
        public static Vector128<byte> CompareMask(Vector128<byte> left, int leftLength, Vector128<byte> right, int rightLength, StringComparisonMode mode) => CompareMask(left, leftLength, right, rightLength, mode);

        /// <summary>
        /// __m128i _mm_cmpestrm (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRM xmm, xmm/m128, imm8
        /// </summary>
        public static Vector128<short> CompareMask(Vector128<short> left, int leftLength, Vector128<short> right, int rightLength, StringComparisonMode mode) => CompareMask(left, leftLength, right, rightLength, mode);

        /// <summary>
        /// __m128i _mm_cmpestrm (__m128i a, int la, __m128i b, int lb, const int imm8)
        ///   PCMPESTRM xmm, xmm/m128, imm8
        /// </summary>
        public static Vector128<ushort> CompareMask(Vector128<ushort> left, int leftLength, Vector128<ushort> right, int rightLength, StringComparisonMode mode) => CompareMask(left, leftLength, right, rightLength, mode);

        /// <summary>
        /// __m128i _mm_cmpgt_epi64 (__m128i a, __m128i b)
        ///   PCMPGTQ xmm, xmm/m128
        /// </summary>
        public static Vector128<long> CompareGreaterThan(Vector128<long> left, Vector128<long> right) => CompareGreaterThan(left, right);

        /// <summary>
        /// unsigned int _mm_crc32_u8 (unsigned int crc, unsigned char v)
        ///   CRC32 reg, reg/m8
        /// </summary>
        public static uint Crc32(uint crc, byte data) => Crc32(crc, data);
        /// <summary>
        /// unsigned int _mm_crc32_u16 (unsigned int crc, unsigned short v)
        ///   CRC32 reg, reg/m16
        /// </summary>
        public static uint Crc32(uint crc, ushort data) => Crc32(crc, data);
        /// <summary>
        /// unsigned int _mm_crc32_u32 (unsigned int crc, unsigned int v)
        ///   CRC32 reg, reg/m32
        /// </summary>
        public static uint Crc32(uint crc, uint data) => Crc32(crc, data);
        /// <summary>
        /// unsigned __int64 _mm_crc32_u64 (unsigned __int64 crc, unsigned __int64 v)
        ///   CRC32 reg, reg/m64
        /// </summary>
        public static ulong Crc32(ulong crc, ulong data) => Crc32(crc, data);
    }
}
