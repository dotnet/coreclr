// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.Intrinsics;

namespace System.Runtime.Intrinsics.X86
{
    /// <summary>
    /// This class provides access to Intel SSE hardware instructions via intrinsics
    /// </summary>
    [CLSCompliant(false)]
    public static class Sse
    {
        public static bool IsSupported { get { return false; } }

        /// <summary>
        /// __m128 _mm_add_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> Add(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_add_ss (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> AddScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_and_ps (__m128 a, __m128 b)
        /// </summary>
        public static Vector128<float> And(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_andnot_ps (__m128 a, __m128 b)
        /// </summary>
        public static Vector128<float> AndNot(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpeq_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareEqual(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// int _mm_comieq_ss (__m128 a, __m128 b)
        /// </summary>
        public static bool CompareEqualOrderedScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// int _mm_ucomieq_ss (__m128 a, __m128 b)
        /// </summary>
        public static bool CompareEqualUnorderedScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpeq_ss (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareEqualScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpgt_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareGreaterThan(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// int _mm_comigt_ss (__m128 a, __m128 b)
        /// </summary>
        public static bool CompareGreaterThanOrderedScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// int _mm_ucomigt_ss (__m128 a, __m128 b)
        /// </summary>
        public static bool CompareGreaterThanUnorderedScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpgt_ss (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareGreaterThanScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpge_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareGreaterThanOrEqual(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// int _mm_comige_ss (__m128 a, __m128 b)
        /// </summary>
        public static bool CompareGreaterThanOrEqualOrderedScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// int _mm_ucomige_ss (__m128 a, __m128 b)
        /// </summary>
        public static bool CompareGreaterThanOrEqualUnorderedScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpge_ss (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareGreaterThanOrEqualScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmplt_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareLessThan(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// int _mm_comilt_ss (__m128 a, __m128 b)
        /// </summary>
        public static bool CompareLessThanOrderedScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// int _mm_ucomilt_ss (__m128 a, __m128 b)
        /// </summary>
        public static bool CompareLessThanUnorderedScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmplt_ss (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareLessThanScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmple_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareLessThanOrEqual(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// int _mm_comile_ss (__m128 a, __m128 b)
        /// </summary>
        public static bool CompareLessThanOrEqualOrderedScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// int _mm_ucomile_ss (__m128 a, __m128 b)
        /// </summary>
        public static bool CompareLessThanOrEqualUnorderedScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmple_ss (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareLessThanOrEqualScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpneq_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareNotEqual(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpneq_ss (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareNotEqualScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// int _mm_comineq_ss (__m128 a, __m128 b)
        /// </summary>
        public static bool CompareNotEqualOrderedScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// int _mm_ucomineq_ss (__m128 a, __m128 b)
        /// </summary>
        public static bool CompareNotEqualUnorderedScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpngt_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareNotGreaterThan(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpngt_ss (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareNotGreaterThanScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpnge_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareNotGreaterThanOrEqual(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpnge_ss (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareNotGreaterThanOrEqualScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpnlt_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareNotLessThan(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpnlt_ss (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareNotLessThanScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpnle_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareNotLessThanOrEqual(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpnle_ss (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareNotLessThanOrEqualScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpord_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareOrdered(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpord_ss (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareOrderedScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpunord_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareUnordered(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cmpunord_ss (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> CompareUnorderedScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// int _mm_cvtss_si32 (__m128 a)
        /// </summary>
        public static int ConvertToInt32(Vector128<float> value) { throw new PlatformNotSupportedException(); }
        /// <summary>
        /// __int64 _mm_cvtss_si64 (__m128 a)
        /// </summary>
        public static long ConvertToInt64(Vector128<float> value) { throw new PlatformNotSupportedException(); }
        /// <summary>
        /// float _mm_cvtss_f32 (__m128 a)
        /// </summary>
        public static float ConvertToSingle(Vector128<float> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_cvtsi32_ss (__m128 a, int b)
        /// </summary>
        public static Vector128<float> ConvertToVector128SingleScalar(Vector128<float> upper, int value) { throw new PlatformNotSupportedException(); }
        /// <summary>
        /// __m128 _mm_cvtsi64_ss (__m128 a, __int64 b)
        /// </summary>
        public static Vector128<float> ConvertToVector128SingleScalar(Vector128<float> upper, long value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// int _mm_cvttss_si32 (__m128 a)
        /// </summary>
        public static int ConvertToInt32WithTruncation(Vector128<float> value) { throw new PlatformNotSupportedException(); }
        /// <summary>
        /// __int64 _mm_cvttss_si64 (__m128 a)
        /// </summary>
        public static long ConvertToInt64WithTruncation(Vector128<float> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_div_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> Divide(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_div_ss (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> DivideScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_loadu_ps (float const* mem_address)
        /// </summary>
        public static unsafe Vector128<float> LoadVector128(float* address) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_load_ss (float const* mem_address)
        /// </summary>
        public static unsafe Vector128<float> LoadScalar(float* address) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_load_ps (float const* mem_address)
        /// </summary>
        public static unsafe Vector128<float> LoadAlignedVector128(float* address) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_loadh_pi (__m128 a, __m64 const* mem_addr)
        /// </summary>
        public static unsafe Vector128<float> LoadHigh(Vector128<float> value, float* address) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_loadh_pi (__m128 a, __m64 const* mem_addr)
        /// </summary>
        public static unsafe Vector128<float> LoadLow(Vector128<float> value, float* address) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_max_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> Max(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_max_ss (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> MaxScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_min_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> Min(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_min_ss (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> MinScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_move_ss (__m128 a, __m128 b)
        /// </summary>
        public static Vector128<float> MoveScalar(Vector128<float> upper, Vector128<float> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_movehl_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> MoveHighToLow(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_movelh_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> MoveLowToHigh(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// int _mm_movemask_ps (__m128 a)
        /// </summary>
        public static int MoveMask(Vector128<float> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_mul_ps (__m128 a, __m128 b)
        /// </summary>
        public static Vector128<float> Multiply(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_mul_ss (__m128 a, __m128 b)
        /// </summary>
        public static Vector128<float> MultiplyScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_or_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> Or(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_rcp_ps (__m128 a)
        /// </summary>
        public static Vector128<float> Reciprocal(Vector128<float> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_rcp_ss (__m128 a)
        /// </summary>
        public static Vector128<float> ReciprocalScalar(Vector128<float> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_rsqrt_ps (__m128 a)
        /// </summary>
        public static Vector128<float> ReciprocalSqrt(Vector128<float> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_rsqrt_ss (__m128 a)
        /// </summary>
        public static Vector128<float> ReciprocalSqrtScalar(Vector128<float> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_set_ps (float e3, float e2, float e1, float e0)
        /// </summary>
        public static Vector128<float> SetVector128(float e3, float e2, float e1, float e0) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_set_ss (float a)
        /// </summary>
        public static Vector128<float> SetScalar(float value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_set1_ps (float a)
        /// </summary>
        public static Vector128<float> SetAllVector128(float value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128d _mm_setzero_ps (void)
        /// </summary>
        public static Vector128<float> SetZeroVector128() { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_castpd_ps (__m128d a)
        /// __m128i _mm_castpd_si128 (__m128d a)
        /// __m128d _mm_castps_pd (__m128 a)
        /// __m128i _mm_castps_si128 (__m128 a)
        /// __m128d _mm_castsi128_pd (__m128i a)
        /// __m128 _mm_castsi128_ps (__m128i a)
        /// </summary>
        public static Vector128<U> StaticCast<T, U>(Vector128<T> value) where T : struct where U : struct { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_shuffle_ps (__m128 a,  __m128 b, unsigned int control)
        /// </summary>
        public static Vector128<float> Shuffle(Vector128<float> left, Vector128<float> right, byte control) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_sqrt_ps (__m128 a)
        /// </summary>
        public static Vector128<float> Sqrt(Vector128<float> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_sqrt_ss (__m128 a)
        /// </summary>
        public static Vector128<float> SqrtScalar(Vector128<float> value) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// void _mm_store_ps (float* mem_addr, __m128 a)
        /// </summary>
        public static unsafe void StoreAligned(float* address, Vector128<float> source) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// void _mm_stream_ps (float* mem_addr, __m128 a)
        /// </summary>
        public static unsafe void StoreAlignedNonTemporal(float* address, Vector128<float> source) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// void _mm_storeu_ps (float* mem_addr, __m128 a)
        /// </summary>
        public static unsafe void Store(float* address, Vector128<float> source) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// void _mm_store_ss (float* mem_addr, __m128 a)
        /// </summary>
        public static unsafe void StoreScalar(float* address, Vector128<float> source) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// void _mm_storeh_pi (__m64* mem_addr, __m128 a)
        /// </summary>
        public static unsafe void StoreHigh(float* address, Vector128<float> source) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// void _mm_storel_pi (__m64* mem_addr, __m128 a)
        /// </summary>
        public static unsafe void StoreLow(float* address, Vector128<float> source) { throw new PlatformNotSupportedException(); }
        
        /// <summary>
        /// __m128d _mm_sub_ps (__m128d a, __m128d b)
        /// </summary>
        public static Vector128<float> Subtract(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_sub_ss (__m128 a, __m128 b)
        /// </summary>
        public static Vector128<float> SubtractScalar(Vector128<float> left, Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_unpackhi_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> UnpackHigh(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_unpacklo_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> UnpackLow(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }

        /// <summary>
        /// __m128 _mm_xor_ps (__m128 a,  __m128 b)
        /// </summary>
        public static Vector128<float> Xor(Vector128<float> left,  Vector128<float> right) { throw new PlatformNotSupportedException(); }
    }
}
