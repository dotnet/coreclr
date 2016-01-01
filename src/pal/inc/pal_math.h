//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*++

Module Name:

    pal_math.h

Abstract:

    Defines all the math.h functions for the Platform Abstraction Layer.
    
--*/

#ifndef __PAL_MATH_H__
#define __PAL_MATH_H__

#ifdef __cplusplus
extern "C" {
#endif

/* Some C runtime functions needs to be reimplemented by the PAL.
   To avoid name collisions, those functions have been renamed using
   defines */
#ifdef PLATFORM_UNIX
#ifndef PAL_STDCPP_COMPAT

// 7.12.0 General Macros
#define INFINITY                (1.0f / 0.0f)
#define NAN                     (0.0f / 0.0f)

// 7.12.3 Classification Macros
#define isfinite(x)             ((x - x) == 0)
#define isinf(x)                ((x == x) && ((x - x) != 0))
#define isnan(x)                (x != x)

// 7.12.4 Trigonometric Functions
#define acos                    PAL_acos
#define acosf                   PAL_acosf

#define asin                    PAL_asin
#define asinf                   PAL_asinf

#define atan                    PAL_atan
#define atanf                   PAL_atanf

#define atan2                   PAL_atan2
#define atan2f                  PAL_atan2f

#define cos                     PAL_cos
#define cosf                    PAL_cosf

#define sin                     PAL_sin
#define sinf                    PAL_sinf

#define tan                     PAL_tan
#define tanf                    PAL_tanf

// 7.12.5 Hyperbolic Functions
#define acosh                   PAL_acosh
#define acoshf                  PAL_acoshf

#define asinh                   PAL_asinh
#define asinhf                  PAL_asinhf

#define atanh                   PAL_atanh
#define atanhf                  PAL_atanhf

#define cosh                    PAL_cosh
#define coshf                   PAL_coshf

#define sinh                    PAL_sinh
#define sinhf                   PAL_sinhf

#define tanh                    PAL_tanh
#define tanhf                   PAL_tanhf

// 7.12.6 Exponential and Logarithmic Functions
#define exp                     PAL_exp
#define expf                    PAL_expf

#define exp2                    PAL_exp2
#define exp2f                   PAL_exp2f

#define expm1                   PAL_expm1
#define expm1f                  PAL_expm1f

#define frexp                   PAL_frexp
#define frexpf                  PAL_frexpf

#define ldexp                   PAL_ldexp
#define ldexpf                  PAL_ldexpf

#define log                     PAL_log
#define logf                    PAL_logf

#define log10                   PAL_log10
#define log10f                  PAL_log10f

#define log1p                   PAL_log1p
#define log1pf                  PAL_log1pf

#define log2                    PAL_log2
#define log2f                   PAL_log2f

#define modf                    PAL_modf
#define modff                   PAL_modff

#define scalbn                  PAL_scalbn
#define scalbnf                 PAL_scalbnf

// 7.12.7 Power and Absolute-Value Functions
#define cbrt                    PAL_cbrt
#define cbrtf                   PAL_cbrtf

#define fabs                    PAL_fabs
#define fabsf                   PAL_fabsf

#define hypot                   PAL_hypot
#define hypotf                  PAL_hypotf

#define pow                     PAL_pow
#define powf                    PAL_powf

#define sqrt                    PAL_sqrt
#define sqrtf                   PAL_sqrtf

// 7.12.8 Error and Gamma Functions
#define erf                     PAL_erf
#define erff                    PAL_erff

#define erfc                    PAL_erfc
#define erfcf                   PAL_erfcf

#define lgamma                  PAL_lgamma
#define lgammaf                 PAL_lgammaf

#define tgamma                  PAL_tgamma
#define tgammaf                 PAL_tgammaf

// 7.12.9 Nearest Integer Functions
#define ceil                    PAL_ceil
#define ceilf                   PAL_ceilf

#define floor                   PAL_floor
#define floorf                  PAL_floorf
    
// 7.12.10 Remainder Functions
#define fmod                    PAL_fmod
#define fmodf                   PAL_fmodf

#define remquo                  PAL_remquo
#define remquof                 PAL_remquof

// 7.12.11 Manipulation Functions
#define copysign                PAL_copysign
#define copysignf               PAL_copysignf

#define nextafter               PAL_nextafter
#define nextafterf              PAL_nextafterf

// 7.12.13 Floating Multiply-Add Function
#define fma                     PAL_fma
#define fmaf                    PAL_fmaf

#endif // !PAL_STDCPP_COMPAT
#endif // PLATFORM_UNIX

// 7.12.4 Trigonometric Functions
PALIMPORT double __cdecl acos(double);
PALIMPORT float __cdecl acosf(float);

PALIMPORT double __cdecl asin(double);
PALIMPORT float __cdecl asinf(float);

PALIMPORT double __cdecl atan(double);
PALIMPORT float __cdecl atanf(float);

PALIMPORT double __cdecl atan2(double, double);
PALIMPORT float __cdecl atan2f(float, float);

PALIMPORT double __cdecl cos(double);
PALIMPORT float __cdecl cosf(float);

PALIMPORT double __cdecl sin(double);
PALIMPORT float __cdecl sinf(float);

PALIMPORT double __cdecl tan(double);
PALIMPORT float __cdecl tanf(float);

// 7.12.5 Hyperbolic Functions
PALIMPORT double __cdecl acosh(double);
PALIMPORT float __cdecl acoshf(float);

PALIMPORT double __cdecl asinh(double);
PALIMPORT float __cdecl asinhf(float);

PALIMPORT double __cdecl atanh(double);
PALIMPORT float __cdecl atanhf(float);

PALIMPORT double __cdecl cosh(double);
PALIMPORT float __cdecl coshf(float);

PALIMPORT double __cdecl sinh(double);
PALIMPORT float __cdecl sinhf(float);

PALIMPORT double __cdecl tanh(double);
PALIMPORT float __cdecl tanhf(float);

// 7.12.6 Exponential and Logarithmic Functions
PALIMPORT double __cdecl exp(double);
PALIMPORT float __cdecl expf(float);

PALIMPORT double __cdecl exp2(double);
PALIMPORT float __cdecl exp2f(float);

PALIMPORT double __cdecl expm1(double);
PALIMPORT float __cdecl expm1f(float);

PALIMPORT double __cdecl frexp(double, int*);
PALIMPORT float __cdecl frexpf(float, int*);

PALIMPORT double __cdecl ldexp(double, int);
PALIMPORT float __cdecl ldexpf(float, int);
    
PALIMPORT double __cdecl log(double);
PALIMPORT float __cdecl logf(float);

PALIMPORT double __cdecl log10(double);
PALIMPORT float __cdecl log10f(float);

PALIMPORT double __cdecl log1p(double);
PALIMPORT float __cdecl log1pf(float);

PALIMPORT double __cdecl log2(double);
PALIMPORT float __cdecl log2f(float);

PALIMPORT double __cdecl modf(double, double*);
PALIMPORT float __cdecl modff(float, float*);

PALIMPORT double __cdecl scalbn(double, int);
PALIMPORT float __cdecl scalbnf(float, int);

// 7.12.7 Power and Absolute-Value Functions
PALIMPORT double __cdecl cbrt(double);
PALIMPORT float __cdecl cbrtf(float);

PALIMPORT double __cdecl fabs(double);
PALIMPORT float __cdecl fabsf(float);

PALIMPORT double __cdecl hypot(double, double);
PALIMPORT float __cdecl hypotf(float, float);

PALIMPORT double __cdecl pow(double, double);
PALIMPORT float __cdecl powf(float, float);

PALIMPORT double __cdecl sqrt(double);
PALIMPORT float __cdecl sqrtf(float);

// 7.12.8 Error and Gamma Functions
PALIMPORT double __cdecl erf(double);
PALIMPORT float __cdecl erff(float);

PALIMPORT double __cdecl erfc(double);
PALIMPORT float __cdecl erfcf(float);

PALIMPORT double __cdecl lgamma(double);
PALIMPORT float __cdecl lgammaf(float);

PALIMPORT double __cdecl tgamma(double);
PALIMPORT float __cdecl tgammaf(float);

// 7.12.9 Nearest Integer Functions
PALIMPORT double __cdecl ceil(double);
PALIMPORT float __cdecl ceilf(float);

PALIMPORT double __cdecl floor(double);
PALIMPORT float __cdecl floorf(float);

// 7.12.10 Remainder Functions
PALIMPORT double __cdecl fmod(double, double);
PALIMPORT float __cdecl fmodf(float, float);

PALIMPORT double __cdecl remquo(double, double, int*);
PALIMPORT float __cdecl remquof(float, float, int*);

// 7.12.11 Manipulation Functions
PALIMPORT double __cdecl copysign(double, double);
PALIMPORT float __cdecl copysignf(float, float);

PALIMPORT double __cdecl nextafter(double, double);
PALIMPORT float __cdecl nextafterf(float, float);

// 7.12.13 Floating Multiply-Add Function
PALIMPORT double __cdecl fma(double, double, double);
PALIMPORT float __cdecl fmaf(float, float, float);

#ifdef __cplusplus
}
#endif

#endif //__PAL_MATH_H__
