// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.Intrinsics;

namespace System.Runtime.Intrinsics.X86
{
    /// <summary>
    /// This class provides access to Intel SSE2 hardware instructions via intrinsics
    /// </summary>
    [CLSCompliant(false)]
    public static class Sse2
    {
        public static bool IsSupported { get => IsSupported; }
        
        /// <summary>
        /// __m128i _mm_add_epi8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<byte> Add(Vector128<byte> left, Vector128<byte> right) => Add(left, right);
        /// <summary>
        /// __m128i _mm_add_epi8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<sbyte> Add(Vector128<sbyte> left, Vector128<sbyte> right) => Add(left, right);
        /// <summary>
        /// __m128i _mm_add_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> Add(Vector128<short> left, Vector128<short> right) => Add(left, right);
        /// <summary>
        /// __m128i _mm_add_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ushort> Add(Vector128<ushort> left, Vector128<ushort> right) => Add(left, right);
        /// <summary>
        /// __m128i _mm_add_epi32 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<int> Add(Vector128<int> left, Vector128<int> right) => Add(left, right);
        /// <summary>
        /// __m128i _mm_add_epi32 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<uint> Add(Vector128<uint> left, Vector128<uint> right) => Add(left, right);
        /// <summary>
        /// __m128i _mm_add_epi64 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<long> Add(Vector128<long> left, Vector128<long> right) => Add(left, right);
        /// <summary>
        /// __m128i _mm_add_epi64 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ulong> Add(Vector128<ulong> left, Vector128<ulong> right) => Add(left, right);
        /// <summary>
        /// __m128d _mm_add_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> Add(Vector128<double> left, Vector128<double> right) => Add(left, right);

        /// <summary>
        /// __m128i _mm_adds_epi8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<sbyte> AddSaturate(Vector128<sbyte> left, Vector128<sbyte> right) => AddSaturate(left, right);
        /// <summary>
        /// __m128i _mm_adds_epu8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<byte> AddSaturate(Vector128<byte> left, Vector128<byte> right) => AddSaturate(left, right);
        /// <summary>
        /// __m128i _mm_adds_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> AddSaturate(Vector128<short> left, Vector128<short> right) => AddSaturate(left, right);
        /// <summary>
        /// __m128i _mm_adds_epu16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ushort> AddSaturate(Vector128<ushort> left, Vector128<ushort> right) => AddSaturate(left, right);

        /// <summary>
        /// __m128i _mm_and_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<byte> And(Vector128<byte> left, Vector128<byte> right) => And(left, right);
        /// <summary>
        /// __m128i _mm_and_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<sbyte> And(Vector128<sbyte> left, Vector128<sbyte> right) => And(left, right);
        /// <summary>
        /// __m128i _mm_and_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> And(Vector128<short> left, Vector128<short> right) => And(left, right);
        /// <summary>
        /// __m128i _mm_and_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ushort> And(Vector128<ushort> left, Vector128<ushort> right) => And(left, right);
        /// <summary>
        /// __m128i _mm_and_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<int> And(Vector128<int> left, Vector128<int> right) => And(left, right);
        /// <summary>
        /// __m128i _mm_and_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<uint> And(Vector128<uint> left, Vector128<uint> right) => And(left, right);
        /// <summary>
        /// __m128i _mm_and_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<long> And(Vector128<long> left, Vector128<long> right) => And(left, right);
        /// <summary>
        /// __m128i _mm_and_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ulong> And(Vector128<ulong> left, Vector128<ulong> right) => And(left, right);
        /// <summary>
        /// __m128d _mm_and_pd (__m128d a, __m128d b)
        /// </summary>
        public static Vector128<double> And(Vector128<double> left, Vector128<double> right) => And(left, right);

        /// <summary>
        /// __m128i _mm_andnot_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<byte> AndNot(Vector128<byte> left, Vector128<byte> right) => AndNot(left, right);
        /// <summary>
        /// __m128i _mm_andnot_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<sbyte> AndNot(Vector128<sbyte> left, Vector128<sbyte> right) => AndNot(left, right);
        /// <summary>
        /// __m128i _mm_andnot_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> AndNot(Vector128<short> left, Vector128<short> right) => AndNot(left, right);
        /// <summary>
        /// __m128i _mm_andnot_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ushort> AndNot(Vector128<ushort> left, Vector128<ushort> right) => AndNot(left, right);
        /// <summary>
        /// __m128i _mm_andnot_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<int> AndNot(Vector128<int> left, Vector128<int> right) => AndNot(left, right);
        /// <summary>
        /// __m128i _mm_andnot_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<uint> AndNot(Vector128<uint> left, Vector128<uint> right) => AndNot(left, right);
        /// <summary>
        /// __m128i _mm_andnot_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<long> AndNot(Vector128<long> left, Vector128<long> right) => AndNot(left, right);
        /// <summary>
        /// __m128i _mm_andnot_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ulong> AndNot(Vector128<ulong> left, Vector128<ulong> right) => AndNot(left, right);
        /// <summary>
        /// __m128d _mm_andnot_pd (__m128d a, __m128d b)
        /// </summary>
        public static Vector128<double> AndNot(Vector128<double> left, Vector128<double> right) => AndNot(left, right);

        /// <summary>
        /// __m128i _mm_avg_epu8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<byte> Average(Vector128<byte> left, Vector128<byte> right) => Average(left, right);
        /// <summary>
        /// __m128i _mm_avg_epu16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ushort> Average(Vector128<ushort> left, Vector128<ushort> right) => Average(left, right);

        /// <summary>
        /// __m128i _mm_cmpeq_epi8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<sbyte> CompareEqual(Vector128<sbyte> left, Vector128<sbyte> right) => CompareEqual(left, right);
        /// <summary>
        /// __m128i _mm_cmpeq_epi8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<byte> CompareEqual(Vector128<byte> left, Vector128<byte> right) => CompareEqual(left, right);
        /// <summary>
        /// __m128i _mm_cmpeq_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> CompareEqual(Vector128<short> left, Vector128<short> right) => CompareEqual(left, right);
        /// <summary>
        /// __m128i _mm_cmpeq_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ushort> CompareEqual(Vector128<ushort> left, Vector128<ushort> right) => CompareEqual(left, right);
        /// <summary>
        /// __m128i _mm_cmpeq_epi32 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<int> CompareEqual(Vector128<int> left, Vector128<int> right) => CompareEqual(left, right);
        /// <summary>
        /// __m128i _mm_cmpeq_epi32 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<uint> CompareEqual(Vector128<uint> left, Vector128<uint> right) => CompareEqual(left, right);
        /// <summary>
        /// __m128d _mm_cmpeq_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> CompareEqual(Vector128<double> left, Vector128<double> right) => CompareEqual(left, right);

        /// <summary>
        /// __m128i _mm_cmpgt_epi8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<sbyte> CompareGreaterThan(Vector128<sbyte> left, Vector128<sbyte> right) => CompareGreaterThan(left, right);
        /// <summary>
        /// __m128i _mm_cmpgt_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> CompareGreaterThan(Vector128<short> left, Vector128<short> right) => CompareGreaterThan(left, right);
        /// <summary>
        /// __m128i _mm_cmpgt_epi32 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<int> CompareGreaterThan(Vector128<int> left, Vector128<int> right) => CompareGreaterThan(left, right);
        /// <summary>
        /// __m128d _mm_cmpgt_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> CompareGreaterThan(Vector128<double> left, Vector128<double> right) => CompareGreaterThan(left, right);

        /// <summary>
        /// __m128d _mm_cmpge_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> CompareGreaterThanOrEqual(Vector128<double> left, Vector128<double> right) => CompareGreaterThanOrEqual(left, right);

        /// <summary>
        /// __m128i _mm_cmplt_epi8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<sbyte> CompareLessThan(Vector128<sbyte> left, Vector128<sbyte> right) => CompareLessThan(left, right);
        /// <summary>
        /// __m128i _mm_cmplt_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> CompareLessThan(Vector128<short> left, Vector128<short> right) => CompareLessThan(left, right);
        /// <summary>
        /// __m128i _mm_cmplt_epi32 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<int> CompareLessThan(Vector128<int> left, Vector128<int> right) => CompareLessThan(left, right);
        /// <summary>
        /// __m128d _mm_cmplt_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> CompareLessThan(Vector128<double> left, Vector128<double> right) => CompareLessThan(left, right);

        /// <summary>
        /// __m128d _mm_cmple_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> CompareLessThanOrEqual(Vector128<double> left, Vector128<double> right) => CompareLessThanOrEqual(left, right);

        /// <summary>
        /// __m128d _mm_cmpneq_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> CompareNotEqual(Vector128<double> left, Vector128<double> right) => CompareNotEqual(left, right);

        /// <summary>
        /// __m128d _mm_cmpngt_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> CompareNotGreaterThan(Vector128<double> left, Vector128<double> right) => CompareNotGreaterThan(left, right);

        /// <summary>
        /// __m128d _mm_cmpnge_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> CompareNotGreaterThanOrEqual(Vector128<double> left, Vector128<double> right) => CompareNotGreaterThanOrEqual(left, right);

        /// <summary>
        /// __m128d _mm_cmpnlt_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> CompareNotLessThan(Vector128<double> left, Vector128<double> right) => CompareNotLessThan(left, right);

        /// <summary>
        /// __m128d _mm_cmpnle_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> CompareNotLessThanOrEqual(Vector128<double> left, Vector128<double> right) => CompareNotLessThanOrEqual(left, right);

        /// <summary>
        /// __m128d _mm_cmpord_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> CompareOrdered(Vector128<double> left, Vector128<double> right) => CompareOrdered(left, right);

        /// <summary>
        /// __m128d _mm_cmpunord_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> CompareUnordered(Vector128<double> left, Vector128<double> right) => CompareUnordered(left, right);

        /// <summary>
        /// __m128i _mm_cvtps_epi32 (__m128 a)
        /// </summary>
        public static Vector128<int> ConvertToInt(Vector128<float> value) => ConvertToInt(value);
        /// <summary>
        /// __m128i _mm_cvtpd_epi32 (__m128d a)
        /// </summary>
        public static Vector128<int> ConvertToInt(Vector128<double> value) => ConvertToInt(value);
        /// <summary>
        /// __m128 _mm_cvtepi32_ps (__m128i a)
        /// </summary>
        public static Vector128<float> ConvertToFloat(Vector128<int> value) => ConvertToFloat(value);
        /// <summary>
        /// __m128 _mm_cvtpd_ps (__m128d a)
        /// </summary>
        public static Vector128<float> ConvertToFloat(Vector128<double> value) => ConvertToFloat(value);
        /// <summary>
        /// __m128d _mm_cvtepi32_pd (__m128i a)
        /// </summary>
        public static Vector128<double> ConvertToDouble(Vector128<int> value) => ConvertToDouble(value);
        /// <summary>
        /// __m128d _mm_cvtps_pd (__m128 a)
        /// </summary>
        public static Vector128<double> ConvertToDouble(Vector128<float> value) => ConvertToDouble(value);

        /// <summary>
        /// __m128i _mm_cvttps_epi32 (__m128 a)
        /// </summary>
        public static Vector128<int> ConvertToIntWithTruncation(Vector128<float> value) => ConvertToIntWithTruncation(value);
        /// <summary>
        /// __m128i _mm_cvttpd_epi32 (__m128d a)
        /// </summary>
        public static Vector128<int> ConvertToIntWithTruncation(Vector128<double> value) => ConvertToIntWithTruncation(value);

        /// <summary>
        /// __m128d _mm_div_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> Divide(Vector128<double> left, Vector128<double> right) => Divide(left, right);

        /// <summary>
        /// int _mm_extract_epi16 (__m128i a,  int immediate)
        /// </summary>
        public static short ExtractShort<T>(Vector128<T> value, byte index) where T : struct
        {
            ThrowHelper.ThrowNotSupportedExceptionIfNonNumericType<T>();
            return ExtractShort<T>(value, index);
        }
        /// <summary>
        /// int _mm_extract_epi16 (__m128i a,  int immediate)
        /// </summary>
        public static ushort ExtractUshort<T>(Vector128<T> value, byte index) where T : struct
        {
            ThrowHelper.ThrowNotSupportedExceptionIfNonNumericType<T>();
            return ExtractUshort<T>(value, index);
        }
        
        /// <summary>
        /// __m128i _mm_insert_epi16 (__m128i a,  int i, int immediate)
        /// </summary>
        public static Vector128<T> InsertShort<T>(Vector128<T> value, short data, byte index) where T : struct
        {
            ThrowHelper.ThrowNotSupportedExceptionIfNonNumericType<T>();
            return InsertShort<T>(value, data, index);
        }
        /// <summary>
        /// __m128i _mm_insert_epi16 (__m128i a,  int i, int immediate)
        /// </summary>
        public static Vector128<T> InsertUshort<T>(Vector128<T> value, ushort data, byte index) where T : struct
        {
            ThrowHelper.ThrowNotSupportedExceptionIfNonNumericType<T>();
            return InsertUshort<T>(value, data, index);
        }
        
        /// <summary>
        /// __m128i _mm_loadu_si128 (__m128i const* mem_address)
        /// </summary>
        public static unsafe Vector128<sbyte> Load(sbyte* address) => Load(address);
        /// <summary>
        /// __m128i _mm_loadu_si128 (__m128i const* mem_address)
        /// </summary>
        public static unsafe Vector128<byte> Load(byte* address) => Load(address);
        /// <summary>
        /// __m128i _mm_loadu_si128 (__m128i const* mem_address)
        /// </summary>
        public static unsafe Vector128<short> Load(short* address) => Load(address);
        /// <summary>
        /// __m128i _mm_loadu_si128 (__m128i const* mem_address)
        /// </summary>
        public static unsafe Vector128<ushort> Load(ushort* address) => Load(address);
        /// <summary>
        /// __m128i _mm_loadu_si128 (__m128i const* mem_address)
        /// </summary>
        public static unsafe Vector128<int> Load(int* address) => Load(address);
        /// <summary>
        /// __m128i _mm_loadu_si128 (__m128i const* mem_address)
        /// </summary>
        public static unsafe Vector128<uint> Load(uint* address) => Load(address);
        /// <summary>
        /// __m128i _mm_loadu_si128 (__m128i const* mem_address)
        /// </summary>
        public static unsafe Vector128<long> Load(long* address) => Load(address);
        /// <summary>
        /// __m128i _mm_loadu_si128 (__m128i const* mem_address)
        /// </summary>
        public static unsafe Vector128<ulong> Load(ulong* address) => Load(address);
        /// <summary>
        /// __m128d _mm_loadu_pd (double const* mem_address)
        /// </summary>
        public static unsafe Vector128<double> Load(double* address) => Load(address);

        /// <summary>
        /// __m128i _mm_load_si128 (__m128i const* mem_address)
        /// </summary>
        public static unsafe Vector128<sbyte> LoadAligned(sbyte* address) => LoadAligned(address);
        /// <summary>
        /// __m128i _mm_load_si128 (__m128i const* mem_address)
        /// </summary>
        public static unsafe Vector128<byte> LoadAligned(byte* address) => LoadAligned(address);
        /// <summary>
        /// __m128i _mm_load_si128 (__m128i const* mem_address)
        /// </summary>
        public static unsafe Vector128<short> LoadAligned(short* address) => LoadAligned(address);
        /// <summary>
        /// __m128i _mm_load_si128 (__m128i const* mem_address)
        /// </summary>
        public static unsafe Vector128<ushort> LoadAligned(ushort* address) => LoadAligned(address);
        /// <summary>
        /// __m128i _mm_load_si128 (__m128i const* mem_address)
        /// </summary>
        public static unsafe Vector128<int> LoadAligned(int* address) => LoadAligned(address);
        /// <summary>
        /// __m128i _mm_load_si128 (__m128i const* mem_address)
        /// </summary>
        public static unsafe Vector128<uint> LoadAligned(uint* address) => LoadAligned(address);
        /// <summary>
        /// __m128i _mm_load_si128 (__m128i const* mem_address)
        /// </summary>
        public static unsafe Vector128<long> LoadAligned(long* address) => LoadAligned(address);
        /// <summary>
        /// __m128i _mm_load_si128 (__m128i const* mem_address)
        /// </summary>
        public static unsafe Vector128<ulong> LoadAligned(ulong* address) => LoadAligned(address);
        /// <summary>
        /// __m128d _mm_load_pd (double const* mem_address)
        /// </summary>
        public static unsafe Vector128<double> LoadAligned(double* address) => LoadAligned(address);

        /// <summary>
        /// void _mm_maskmoveu_si128 (__m128i a,  __m128i mask, char* mem_address)
        /// </summary>
        public static unsafe void MaskMove(Vector128<sbyte> source, Vector128<sbyte> mask, sbyte* address) => MaskMove(source, mask, address);
        /// <summary>
        /// void _mm_maskmoveu_si128 (__m128i a,  __m128i mask, char* mem_address)
        /// </summary>
        public static unsafe void MaskMove(Vector128<byte> source, Vector128<byte> mask, byte* address) => MaskMove(source, mask, address);

        /// <summary>
        /// __m128i _mm_max_epu8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<byte> Max(Vector128<byte> left, Vector128<byte> right) => Max(left, right);
        /// <summary>
        /// __m128i _mm_max_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> Max(Vector128<short> left, Vector128<short> right) => Max(left, right);
        /// <summary>
        /// __m128d _mm_max_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> Max(Vector128<double> left, Vector128<double> right) => Max(left, right);

        /// <summary>
        /// __m128i _mm_min_epu8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<byte> Min(Vector128<byte> left, Vector128<byte> right) => Min(left, right);
        /// <summary>
        /// __m128i _mm_min_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> Min(Vector128<short> left, Vector128<short> right) => Min(left, right);
        /// <summary>
        /// __m128d _mm_min_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> Min(Vector128<double> left, Vector128<double> right) => Min(left, right);

        /// <summary>
        /// int _mm_movemask_epi8 (__m128i a)
        /// </summary>
        public static int MoveMask(Vector128<sbyte> value) => MoveMask(value);
        /// <summary>
        /// int _mm_movemask_pd (__m128d a)
        /// </summary>
        public static int MoveMask(Vector128<double> value) => MoveMask(value);

        /// <summary>
        /// __m128i _mm_mul_epu32 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ulong> Multiply(Vector128<uint> left, Vector128<uint> right) => Multiply(left, right);
        /// <summary>
        /// __m128d _mm_mul_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> Multiply(Vector128<double> left, Vector128<double> right) => Multiply(left, right);

        /// <summary>
        /// __m128i _mm_mulhi_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> MultiplyHi(Vector128<short> left, Vector128<short> right) => MultiplyHi(left, right);
        /// <summary>
        /// __m128i _mm_mulhi_epu16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ushort> MultiplyHi(Vector128<ushort> left, Vector128<ushort> right) => MultiplyHi(left, right);

        /// <summary>
        /// __m128i _mm_madd_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<int> MultiplyHorizontalAdd(Vector128<short> left, Vector128<short> right) => MultiplyHorizontalAdd(left, right);

        /// <summary>
        /// __m128i _mm_mullo_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> MultiplyLow(Vector128<short> left, Vector128<short> right) => MultiplyLow(left, right);

        /// <summary>
        /// __m128i _mm_or_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<byte> Or(Vector128<byte> left, Vector128<byte> right) => Or(left, right);
        /// <summary>
        /// __m128i _mm_or_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<sbyte> Or(Vector128<sbyte> left, Vector128<sbyte> right) => Or(left, right);
        /// <summary>
        /// __m128i _mm_or_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> Or(Vector128<short> left, Vector128<short> right) => Or(left, right);
        /// <summary>
        /// __m128i _mm_or_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ushort> Or(Vector128<ushort> left, Vector128<ushort> right) => Or(left, right);
        /// <summary>
        /// __m128i _mm_or_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<int> Or(Vector128<int> left, Vector128<int> right) => Or(left, right);
        /// <summary>
        /// __m128i _mm_or_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<uint> Or(Vector128<uint> left, Vector128<uint> right) => Or(left, right);
        /// <summary>
        /// __m128i _mm_or_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<long> Or(Vector128<long> left, Vector128<long> right) => Or(left, right);
        /// <summary>
        /// __m128i _mm_or_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ulong> Or(Vector128<ulong> left, Vector128<ulong> right) => Or(left, right);
        /// <summary>
        /// __m128d _mm_or_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> Or(Vector128<double> left, Vector128<double> right) => Or(left, right);

        /// <summary>
        /// __m128i _mm_packs_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<sbyte> PackSignedSaturate(Vector128<short> left, Vector128<short> right) => PackSignedSaturate(left, right);
        /// <summary>
        /// __m128i _mm_packs_epi32 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> PackSignedSaturate(Vector128<int> left, Vector128<int> right) => PackSignedSaturate(left, right);

        /// <summary>
        /// __m128i _mm_packus_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<byte> PackUnsignedSaturate(Vector128<short> left, Vector128<short> right) => PackUnsignedSaturate(left, right);

        /// <summary>
        /// ___m128i _mm_set_epi8 (char e15, char e14, char e13, char e12, char e11, char e10, char e9, char e8, char e7, char e6, char e5, char e4, char e3, char e2, char e1, char e0)
        /// </summary>
        public static Vector128<sbyte> Set(sbyte e15, sbyte e14, sbyte e13, sbyte e12, sbyte e11, sbyte e10, sbyte e9, sbyte e8, sbyte e7, sbyte e6, sbyte e5, sbyte e4, sbyte e3, sbyte e2, sbyte e1, sbyte e0)  => Set(e15, e14, e13, e12, e11, e10, e9, e8, e7, e6, e5, e4, e3, e2, e1, e0);
        /// <summary>
        /// ___m128i _mm_set_epi8 (char e15, char e14, char e13, char e12, char e11, char e10, char e9, char e8, char e7, char e6, char e5, char e4, char e3, char e2, char e1, char e0)
        /// </summary>
        public static Vector128<byte> Set(byte e15, byte e14, byte e13, byte e12, byte e11, byte e10, byte e9, byte e8, byte e7, byte e6, byte e5, byte e4, byte e3, byte e2, byte e1, byte e0)  => Set(e15, e14, e13, e12, e11, e10, e9, e8, e7, e6, e5, e4, e3, e2, e1, e0);
        /// <summary>
        /// __m128i _mm_set_epi16 (short e7, short e6, short e5, short e4, short e3, short e2, short e1, short e0)
        /// </summary>
        public static Vector128<short> Set(short e7, short e6, short e5, short e4, short e3, short e2, short e1, short e0) => Set(e7, e6, e5, e4, e3, e2, e1, e0);
        /// <summary>
        /// __m128i _mm_set_epi16 (short e7, short e6, short e5, short e4, short e3, short e2, short e1, short e0)
        /// </summary>
        public static Vector128<ushort> Set(ushort e7, ushort e6, ushort e5, ushort e4, ushort e3, ushort e2, ushort e1, ushort e0) => Set(e7, e6, e5, e4, e3, e2, e1, e0);
        /// <summary>
        /// __m128i _mm_set_epi32 (int e3, int e2, int e1, int e0)
        /// </summary>
        public static Vector128<int> Set(int e3, int e2, int e1, int e0) => Set(e3, e2, e1, e0);
        /// <summary>
        /// __m128i _mm_set_epi32 (int e3, int e2, int e1, int e0)
        /// </summary>
        public static Vector128<uint> Set(uint e3, uint e2, uint e1, uint e0) => Set(e3, e2, e1, e0);
        /// <summary>
        /// __m128i _mm_set_epi64x (__int64 e1, __int64 e0)
        /// </summary>
        public static Vector128<long> Set(long e1, long e0) => Set(e1, e0);
        /// <summary>
        /// __m128i _mm_set_epi64x (__int64 e1, __int64 e0)
        /// </summary>
        public static Vector128<ulong> Set(ulong e1, ulong e0) => Set(e1, e0);
        /// <summary>
        /// __m128d _mm_set_pd (double e1, double e0)
        /// </summary>
        public static Vector128<double> Set(double e1, double e0) => Set(e1, e0);
        
        /// <summary>
        /// __m128i _mm_set1_epi8 (char a)
        /// </summary>
        public static Vector128<byte> Set1(byte value) => Set1(value);
        /// <summary>
        /// __m128i _mm_set1_epi8 (char a)
        /// </summary>
        public static Vector128<sbyte> Set1(sbyte value) => Set1(value);
        /// <summary>
        /// __m128i _mm_set1_epi16 (short a)
        /// </summary>
        public static Vector128<short> Set1(short value) => Set1(value);
        /// <summary>
        /// __m128i _mm_set1_epi16 (short a)
        /// </summary>
        public static Vector128<ushort> Set1(ushort value) => Set1(value);
        /// <summary>
        /// __m128i _mm_set1_epi32 (int a)
        /// </summary>
        public static Vector128<int> Set1(int value) => Set1(value);
        /// <summary>
        /// __m128i _mm_set1_epi32 (int a)
        /// </summary>
        public static Vector128<uint> Set1(uint value) => Set1(value);
        /// <summary>
        /// __m128i _mm_set1_epi64x (long long a)
        /// </summary>
        public static Vector128<long> Set1(long value) => Set1(value);
        /// <summary>
        /// __m128i _mm_set1_epi64x (long long a)
        /// </summary>
        public static Vector128<ulong> Set1(ulong value) => Set1(value);
        /// <summary>
        /// __m128d _mm_set1_pd (double a)
        /// </summary>
        public static Vector128<double> Set1(double value) => Set1(value);

        /// <summary>
        /// __m128i _mm_setzero_si128 ()
        /// __m128d _mm_setzero_pd (void)
        /// </summary>
        public static Vector128<T> SetZero<T>() where T : struct
        {
            ThrowHelper.ThrowNotSupportedExceptionIfNonNumericType<T>();
            return SetZero<T>();
        }

        /// <summary>
        /// __m128i _mm_sad_epu8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<long> SumAbsoluteDifferences(Vector128<byte> left, Vector128<byte> right) => SumAbsoluteDifferences(left, right);

        /// <summary>
        /// __m128i _mm_shuffle_epi32 (__m128i a,  int immediate)
        /// </summary>
        public static Vector128<int> Shuffle(Vector128<int> value, byte control) => Shuffle(value, control);
        /// <summary>
        /// __m128i _mm_shuffle_epi32 (__m128i a,  int immediate)
        /// </summary>
        public static Vector128<uint> Shuffle(Vector128<uint> value, byte control) => Shuffle(value, control);
        /// <summary>
        /// __m128d _mm_shuffle_pd (__m128d a,  __m128d b, int immediate)
        /// </summary>
        public static Vector128<double> Shuffle(Vector128<double> left, Vector128<double> right, byte control) => Shuffle(left, right, control);
        
        /// <summary>
        /// __m128i _mm_shufflehi_epi16 (__m128i a,  int immediate)
        /// </summary>
        public static Vector128<short> ShuffleHigh(Vector128<short> value, byte control) => ShuffleHigh(value, control);
        /// <summary>
        /// __m128i _mm_shufflehi_epi16 (__m128i a,  int control)
        /// </summary>
        public static Vector128<ushort> ShuffleHigh(Vector128<ushort> value, byte control) => ShuffleHigh(value, control);
        
        /// <summary>
        /// __m128i _mm_shufflelo_epi16 (__m128i a,  int control)
        /// </summary>
        public static Vector128<short> ShuffleLow(Vector128<short> value, byte control) => ShuffleLow(value, control);
        /// <summary>
        /// __m128i _mm_shufflelo_epi16 (__m128i a,  int control)
        /// </summary>
        public static Vector128<ushort> ShuffleLow(Vector128<ushort> value, byte control) => ShuffleLow(value, control);
        
        /// <summary>
        /// __m128i _mm_slli_epi16 (__m128i a,  int immediate)
        /// </summary>
        public static Vector128<short> ShiftLeftLogical(Vector128<short> value, byte count) => ShiftLeftLogical(value, count);
        /// <summary>
        /// __m128i _mm_slli_epi16 (__m128i a,  int immediate)
        /// </summary>
        public static Vector128<ushort> ShiftLeftLogical(Vector128<ushort> value, byte count) => ShiftLeftLogical(value, count);
        /// <summary>
        /// __m128i _mm_slli_epi32 (__m128i a,  int immediate)
        /// </summary>
        public static Vector128<int> ShiftLeftLogical(Vector128<int> value, byte count) => ShiftLeftLogical(value, count);
        /// <summary>
        /// __m128i _mm_slli_epi32 (__m128i a,  int immediate)
        /// </summary>
        public static Vector128<uint> ShiftLeftLogical(Vector128<uint> value, byte count) => ShiftLeftLogical(value, count);
        /// <summary>
        /// __m128i _mm_slli_epi64 (__m128i a,  int immediate)
        /// </summary>
        public static Vector128<long> ShiftLeftLogical(Vector128<long> value, byte count) => ShiftLeftLogical(value, count);
        /// <summary>
        /// __m128i _mm_slli_epi64 (__m128i a,  int immediate)
        /// </summary>
        public static Vector128<ulong> ShiftLeftLogical(Vector128<ulong> value, byte count) => ShiftLeftLogical(value, count);
        
        /// <summary>
        /// __m128i _mm_bslli_si128 (__m128i a, int imm8)
        /// </summary>
        public static Vector128<sbyte> ShiftLeftLogical128BitLane(Vector128<sbyte> value, byte numBytes) => ShiftLeftLogical128BitLane(value, numBytes);
        /// <summary>
        /// __m128i _mm_bslli_si128 (__m128i a, int imm8)
        /// </summary>
        public static Vector128<byte> ShiftLeftLogical128BitLane(Vector128<byte> value, byte numBytes) => ShiftLeftLogical128BitLane(value, numBytes);
        /// <summary>
        /// __m128i _mm_bslli_si128 (__m128i a, int imm8)
        /// </summary>
        public static Vector128<short> ShiftLeftLogical128BitLane(Vector128<short> value, byte numBytes) => ShiftLeftLogical128BitLane(value, numBytes);
        /// <summary>
        /// __m128i _mm_bslli_si128 (__m128i a, int imm8)
        /// </summary>
        public static Vector128<ushort> ShiftLeftLogical128BitLane(Vector128<ushort> value, byte numBytes) => ShiftLeftLogical128BitLane(value, numBytes);
        /// <summary>
        /// __m128i _mm_bslli_si128 (__m128i a, int imm8)
        /// </summary>
        public static Vector128<int> ShiftLeftLogical128BitLane(Vector128<int> value, byte numBytes) => ShiftLeftLogical128BitLane(value, numBytes);
        /// <summary>
        /// __m128i _mm_bslli_si128 (__m128i a, int imm8)
        /// </summary>
        public static Vector128<uint> ShiftLeftLogical128BitLane(Vector128<uint> value, byte numBytes) => ShiftLeftLogical128BitLane(value, numBytes);
        /// <summary>
        /// __m128i _mm_bslli_si128 (__m128i a, int imm8)
        /// </summary>
        public static Vector128<long> ShiftLeftLogical128BitLane(Vector128<long> value, byte numBytes) => ShiftLeftLogical128BitLane(value, numBytes);
        /// <summary>
        /// __m128i _mm_bslli_si128 (__m128i a, int imm8)
        /// </summary>
        public static Vector128<ulong> ShiftLeftLogical128BitLane(Vector128<ulong> value, byte numBytes) => ShiftLeftLogical128BitLane(value, numBytes);
        
        /// <summary>
        /// __m128i _mm_srai_epi16 (__m128i a,  int immediate)
        /// </summary>
        public static Vector128<short> ShiftRightArithmetic(Vector128<short> value, byte count) => ShiftRightArithmetic(value, count);
        /// <summary>
        /// __m128i _mm_srai_epi32 (__m128i a,  int immediate)
        /// </summary>
        public static Vector128<int> ShiftRightArithmetic(Vector128<int> value, byte count) => ShiftRightArithmetic(value, count);
        
        /// <summary>
        /// __m128i _mm_srli_epi16 (__m128i a,  int immediate)
        /// </summary>
        public static Vector128<short> ShiftRightLogical(Vector128<short> value, byte count) => ShiftRightLogical(value, count);
        /// <summary>
        /// __m128i _mm_srli_epi16 (__m128i a,  int immediate)
        /// </summary>
        public static Vector128<ushort> ShiftRightLogical(Vector128<ushort> value, byte count) => ShiftRightLogical(value, count);
        /// <summary>
        /// __m128i _mm_srli_epi32 (__m128i a,  int immediate)
        /// </summary>
        public static Vector128<int> ShiftRightLogical(Vector128<int> value, byte count) => ShiftRightLogical(value, count);
        /// <summary>
        /// __m128i _mm_srli_epi32 (__m128i a,  int immediate)
        /// </summary>
        public static Vector128<uint> ShiftRightLogical(Vector128<uint> value, byte count) => ShiftRightLogical(value, count);
        /// <summary>
        /// __m128i _mm_srli_epi64 (__m128i a,  int immediate)
        /// </summary>
        public static Vector128<long> ShiftRightLogical(Vector128<long> value, byte count) => ShiftRightLogical(value, count);
        /// <summary>
        /// __m128i _mm_srli_epi64 (__m128i a,  int immediate)
        /// </summary>
        public static Vector128<ulong> ShiftRightLogical(Vector128<ulong> value, byte count) => ShiftRightLogical(value, count);
        
        /// <summary>
        /// __m128i _mm_bsrli_si128 (__m128i a, int imm8)
        /// </summary>
        public static Vector128<sbyte> ShiftRightLogical128BitLane(Vector128<sbyte> value, byte numBytes) => ShiftRightLogical128BitLane(value, numBytes);
        /// <summary>
        /// __m128i _mm_bsrli_si128 (__m128i a, int imm8)
        /// </summary>
        public static Vector128<byte> ShiftRightLogical128BitLane(Vector128<byte> value, byte numBytes) => ShiftRightLogical128BitLane(value, numBytes);
        /// <summary>
        /// __m128i _mm_bsrli_si128 (__m128i a, int imm8)
        /// </summary>
        public static Vector128<short> ShiftRightLogical128BitLane(Vector128<short> value, byte numBytes) => ShiftRightLogical128BitLane(value, numBytes);
        /// <summary>
        /// __m128i _mm_bsrli_si128 (__m128i a, int imm8)
        /// </summary>
        public static Vector128<ushort> ShiftRightLogical128BitLane(Vector128<ushort> value, byte numBytes) => ShiftRightLogical128BitLane(value, numBytes);
        /// <summary>
        /// __m128i _mm_bsrli_si128 (__m128i a, int imm8)
        /// </summary>
        public static Vector128<int> ShiftRightLogical128BitLane(Vector128<int> value, byte numBytes) => ShiftRightLogical128BitLane(value, numBytes);
        /// <summary>
        /// __m128i _mm_bsrli_si128 (__m128i a, int imm8)
        /// </summary>
        public static Vector128<uint> ShiftRightLogical128BitLane(Vector128<uint> value, byte numBytes) => ShiftRightLogical128BitLane(value, numBytes);
        /// <summary>
        /// __m128i _mm_bsrli_si128 (__m128i a, int imm8)
        /// </summary>
        public static Vector128<long> ShiftRightLogical128BitLane(Vector128<long> value, byte numBytes) => ShiftRightLogical128BitLane(value, numBytes);
        /// <summary>
        /// __m128i _mm_bsrli_si128 (__m128i a, int imm8)
        /// </summary>
        public static Vector128<ulong> ShiftRightLogical128BitLane(Vector128<ulong> value, byte numBytes) => ShiftRightLogical128BitLane(value, numBytes);
        
        /// <summary>
        /// __m128d _mm_sqrt_pd (__m128d a)
        /// </summary>
        public static Vector128<double> Sqrt(Vector128<double> value) => Sqrt(value);

        /// <summary>
        /// void _mm_store_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreAligned(sbyte* address, Vector128<sbyte> source) => StoreAligned(address, source);
        /// <summary>
        /// void _mm_store_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreAligned(byte* address, Vector128<byte> source) => StoreAligned(address, source);
        /// <summary>
        /// void _mm_store_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreAligned(short* address, Vector128<short> source) => StoreAligned(address, source);
        /// <summary>
        /// void _mm_store_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreAligned(ushort* address, Vector128<ushort> source) => StoreAligned(address, source);
        /// <summary>
        /// void _mm_store_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreAligned(int* address, Vector128<int> source) => StoreAligned(address, source);
        /// <summary>
        /// void _mm_store_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreAligned(uint* address, Vector128<uint> source) => StoreAligned(address, source);
        /// <summary>
        /// void _mm_store_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreAligned(long* address, Vector128<long> source) => StoreAligned(address, source);
        /// <summary>
        /// void _mm_store_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreAligned(ulong* address, Vector128<ulong> source) => StoreAligned(address, source);
        /// <summary>
        /// void _mm_store_pd (double* mem_addr, __m128d a)
        /// </summary>
        public static unsafe void StoreAligned(double* address, Vector128<double> source) => StoreAligned(address, source);

        /// <summary>
        /// void _mm_stream_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreAlignedNonTemporal(sbyte* address, Vector128<sbyte> source) => StoreAlignedNonTemporal(address, source);
        /// <summary>
        /// void _mm_stream_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreAlignedNonTemporal(byte* address, Vector128<byte> source) => StoreAlignedNonTemporal(address, source);
        /// <summary>
        /// void _mm_stream_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreAlignedNonTemporal(short* address, Vector128<short> source) => StoreAlignedNonTemporal(address, source);
        /// <summary>
        /// void _mm_stream_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreAlignedNonTemporal(ushort* address, Vector128<ushort> source) => StoreAlignedNonTemporal(address, source);
        /// <summary>
        /// void _mm_stream_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreAlignedNonTemporal(int* address, Vector128<int> source) => StoreAlignedNonTemporal(address, source);
        /// <summary>
        /// void _mm_stream_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreAlignedNonTemporal(uint* address, Vector128<uint> source) => StoreAlignedNonTemporal(address, source);
        /// <summary>
        /// void _mm_stream_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreAlignedNonTemporal(long* address, Vector128<long> source) => StoreAlignedNonTemporal(address, source);
        /// <summary>
        /// void _mm_stream_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreAlignedNonTemporal(ulong* address, Vector128<ulong> source) => StoreAlignedNonTemporal(address, source);
        /// <summary>
        /// void _mm_stream_pd (double* mem_addr, __m128d a)
        /// </summary>
        public static unsafe void StoreAlignedNonTemporal(double* address, Vector128<double> source) => StoreAlignedNonTemporal(address, source);

        /// <summary>
        /// void _mm_storeu_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void Store(sbyte* address, Vector128<sbyte> source) => Store(address, source);
        /// <summary>
        /// void _mm_storeu_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void Store(byte* address, Vector128<byte> source) => Store(address, source);
        /// <summary>
        /// void _mm_storeu_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void Store(short* address, Vector128<short> source) => Store(address, source);
        /// <summary>
        /// void _mm_storeu_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void Store(ushort* address, Vector128<ushort> source) => Store(address, source);
        /// <summary>
        /// void _mm_storeu_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void Store(int* address, Vector128<int> source) => Store(address, source);
        /// <summary>
        /// void _mm_storeu_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void Store(uint* address, Vector128<uint> source) => Store(address, source);
        /// <summary>
        /// void _mm_storeu_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void Store(long* address, Vector128<long> source) => Store(address, source);
        /// <summary>
        /// void _mm_storeu_si128 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void Store(ulong* address, Vector128<ulong> source) => Store(address, source);
        /// <summary>
        /// void _mm_storeu_pd (double* mem_addr, __m128d a)
        /// </summary>
        public static unsafe void Store(double* address, Vector128<double> source) => Store(address, source);
        
        /// <summary>
        /// void _mm_storeh_pd (double* mem_addr, __m128d a)
        /// </summary>
        public static unsafe void StoreHigh(double* address, Vector128<double> source) => StoreHigh(address, source);

        /// <summary>
        /// void _mm_storel_epi64 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreLow(long* address, Vector128<long> source) => StoreLow(address, source);
        /// <summary>
        /// void _mm_storel_epi64 (__m128i* mem_addr, __m128i a)
        /// </summary>
        public static unsafe void StoreLow(ulong* address, Vector128<ulong> source) => StoreLow(address, source);
        /// <summary>
        /// void _mm_storel_pd (double* mem_addr, __m128d a)
        /// </summary>
        public static unsafe void StoreLow(double* address, Vector128<double> source) => StoreLow(address, source);

        /// <summary>
        /// __m128i _mm_sub_epi8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<byte> Subtract(Vector128<byte> left, Vector128<byte> right) => Subtract(left, right);
        /// <summary>
        /// __m128i _mm_sub_epi8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<sbyte> Subtract(Vector128<sbyte> left, Vector128<sbyte> right) => Subtract(left, right);
        /// <summary>
        /// __m128i _mm_sub_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> Subtract(Vector128<short> left, Vector128<short> right) => Subtract(left, right);
        /// <summary>
        /// __m128i _mm_sub_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ushort> Subtract(Vector128<ushort> left, Vector128<ushort> right) => Subtract(left, right);
        /// <summary>
        /// __m128i _mm_sub_epi32 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<int> Subtract(Vector128<int> left, Vector128<int> right) => Subtract(left, right);
        /// <summary>
        /// __m128i _mm_sub_epi32 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<uint> Subtract(Vector128<uint> left, Vector128<uint> right) => Subtract(left, right);
        /// <summary>
        /// __m128i _mm_sub_epi64 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<long> Subtract(Vector128<long> left, Vector128<long> right) => Subtract(left, right);
        /// <summary>
        /// __m128i _mm_sub_epi64 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ulong> Subtract(Vector128<ulong> left, Vector128<ulong> right) => Subtract(left, right);
        /// <summary>
        /// __m128d _mm_sub_pd (__m128d a, __m128d b)
        /// </summary>
        public static Vector128<double> Subtract(Vector128<double> left, Vector128<double> right) => Subtract(left, right);

        /// <summary>
        /// __m128i _mm_subs_epi8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<sbyte> SubtractSaturate(Vector128<sbyte> left, Vector128<sbyte> right) => SubtractSaturate(left, right);
        /// <summary>
        /// __m128i _mm_subs_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> SubtractSaturate(Vector128<short> left, Vector128<short> right) => SubtractSaturate(left, right);
        /// <summary>
        /// __m128i _mm_subs_epu8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<byte> SubtractSaturate(Vector128<byte> left, Vector128<byte> right) => SubtractSaturate(left, right);
        /// <summary>
        /// __m128i _mm_subs_epu16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ushort> SubtractSaturate(Vector128<ushort> left, Vector128<ushort> right) => SubtractSaturate(left, right);

        /// <summary>
        /// __m128i _mm_unpackhi_epi8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<byte> UnpackHigh(Vector128<byte> left, Vector128<byte> right) => UnpackHigh(left, right);
        /// <summary>
        /// __m128i _mm_unpackhi_epi8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<sbyte> UnpackHigh(Vector128<sbyte> left, Vector128<sbyte> right) => UnpackHigh(left, right);
        /// <summary>
        /// __m128i _mm_unpackhi_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> UnpackHigh(Vector128<short> left, Vector128<short> right) => UnpackHigh(left, right);
        /// <summary>
        /// __m128i _mm_unpackhi_epi16 (__m128i a,  __m128i b)
        /// </summary
        public static Vector128<ushort> UnpackHigh(Vector128<ushort> left, Vector128<ushort> right) => UnpackHigh(left, right);
        /// <summary>
        /// __m128i _mm_unpackhi_epi32 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<int> UnpackHigh(Vector128<int> left, Vector128<int> right) => UnpackHigh(left, right);
        /// <summary>
        /// __m128i _mm_unpackhi_epi32 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<uint> UnpackHigh(Vector128<uint> left, Vector128<uint> right) => UnpackHigh(left, right);
        /// <summary>
        /// __m128i _mm_unpackhi_epi64 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<long> UnpackHigh(Vector128<long> left, Vector128<long> right) => UnpackHigh(left, right);
        /// <summary>
        /// __m128i _mm_unpackhi_epi64 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ulong> UnpackHigh(Vector128<ulong> left, Vector128<ulong> right) => UnpackHigh(left, right);
        /// <summary>
        /// __m128d _mm_unpackhi_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> UnpackHigh(Vector128<double> left, Vector128<double> right) => UnpackHigh(left, right);
        
        /// <summary>
        /// __m128i _mm_unpacklo_epi8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<byte> UnpackLow(Vector128<byte> left, Vector128<byte> right) => UnpackLow(left, right);
        /// <summary>
        /// __m128i _mm_unpacklo_epi8 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<sbyte> UnpackLow(Vector128<sbyte> left, Vector128<sbyte> right) => UnpackLow(left, right);
        /// <summary>
        /// __m128i _mm_unpacklo_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> UnpackLow(Vector128<short> left, Vector128<short> right) => UnpackLow(left, right);
        /// <summary>
        /// __m128i _mm_unpacklo_epi16 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ushort> UnpackLow(Vector128<ushort> left, Vector128<ushort> right) => UnpackLow(left, right);
        /// <summary>
        /// __m128i _mm_unpacklo_epi32 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<int> UnpackLow(Vector128<int> left, Vector128<int> right) => UnpackLow(left, right);
        /// <summary>
        /// __m128i _mm_unpacklo_epi32 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<uint> UnpackLow(Vector128<uint> left, Vector128<uint> right) => UnpackLow(left, right);
        /// <summary>
        /// __m128i _mm_unpacklo_epi64 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<long> UnpackLow(Vector128<long> left, Vector128<long> right) => UnpackLow(left, right);
        /// <summary>
        /// __m128i _mm_unpacklo_epi64 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ulong> UnpackLow(Vector128<ulong> left, Vector128<ulong> right) => UnpackLow(left, right);
        /// <summary>
        /// __m128d _mm_unpacklo_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> UnpackLow(Vector128<double> left, Vector128<double> right) => UnpackLow(left, right);
        
        /// <summary>
        /// __m128i _mm_xor_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<byte> Xor(Vector128<byte> left, Vector128<byte> right) => Xor(left, right);
        /// <summary>
        /// __m128i _mm_xor_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<sbyte> Xor(Vector128<sbyte> left, Vector128<sbyte> right) => Xor(left, right);
        /// <summary>
        /// __m128i _mm_xor_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<short> Xor(Vector128<short> left, Vector128<short> right) => Xor(left, right);
        /// <summary>
        /// __m128i _mm_xor_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ushort> Xor(Vector128<ushort> left, Vector128<ushort> right) => Xor(left, right);
        /// <summary>
        /// __m128i _mm_xor_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<int> Xor(Vector128<int> left, Vector128<int> right) => Xor(left, right);
        /// <summary>
        /// __m128i _mm_xor_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<uint> Xor(Vector128<uint> left, Vector128<uint> right) => Xor(left, right);
        /// <summary>
        /// __m128i _mm_xor_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<long> Xor(Vector128<long> left, Vector128<long> right) => Xor(left, right);
        /// <summary>
        /// __m128i _mm_xor_si128 (__m128i a,  __m128i b)
        /// </summary>
        public static Vector128<ulong> Xor(Vector128<ulong> left, Vector128<ulong> right) => Xor(left, right);
        /// <summary>
        /// __m128d _mm_xor_pd (__m128d a,  __m128d b)
        /// </summary>
        public static Vector128<double> Xor(Vector128<double> left, Vector128<double> right) => Xor(left, right);
    }
}
