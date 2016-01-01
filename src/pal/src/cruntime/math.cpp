//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*++

Module Name:

    math.cpp

Abstract:

    Implements all the math.h functions for the Platform Abstraction Layer.
    
--*/

#include "pal/palinternal.h"
#include "pal/dbgmsg.h"

#include <math.h>

#if HAVE_IEEEFP_H
#include <ieeefp.h>
#endif  // HAVE_IEEEFP_H

#include <errno.h>

SET_DEFAULT_DEBUG_CHANNEL(CRT);

// 7.12.4 Trigonometric Functions
/*++
Function:
    acos

See MSDN.
--*/
PALIMPORT double __cdecl PAL_acos(double x)
{
    double ret;
    PERF_ENTRY(acos);
    ENTRY("Entry to acos: double x=%f\n", x);

#if !(HAVE_COMPATIBLE_ACOS)
    errno = 0;
#endif  // !(HAVE_COMPATIBLE_ACOS)
        
    ret = acos(x);
    
#if !HAVE_COMPATIBLE_ACOS
    if (errno == EDOM)
    {
        ret = NAN;
    }
#endif  // !(HAVE_COMPATIBLE_ACOS)

    LOGEXIT("Exit from acos: double return=%f\n", ret);
    PERF_EXIT(acos);
    return ret;
}

/*++
Function:
    acosf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_acosf(float x)
{
    float ret;
    PERF_ENTRY(acosf);
    ENTRY("Entry to acosf: float x=%f\n", x);

#if !(HAVE_COMPATIBLE_ACOS)
    errno = 0;
#endif  // !(HAVE_COMPATIBLE_ACOS)
    
    ret = acosf(x);
    
#if !HAVE_COMPATIBLE_ACOS
    if (errno == EDOM)
    {
        ret = NAN;
    }
#endif  // !(HAVE_COMPATIBLE_ACOS)

    LOGEXIT("Exit from acosf: float return=%f\n", ret);
    PERF_EXIT(acosf);
    return ret;
}

/*++
Function:
    asin

See MSDN.
--*/
PALIMPORT double __cdecl PAL_asin(double x)
{
    double ret;
    PERF_ENTRY(asin);
    ENTRY("Entry to asin: double x=%f\n", x);

#if !(HAVE_COMPATIBLE_ASIN)
    errno = 0;
#endif  // !(HAVE_COMPATIBLE_ASIN)
    
    ret = asin(x);
    
#if !(HAVE_COMPATIBLE_ASIN)
    if (errno == EDOM)
    {
        ret = NAN;
    }
#endif  // !(HAVE_COMPATIBLE_ASIN)

    LOGEXIT("Exit from asin: double return=%f\n", ret);
    PERF_EXIT(asin);
    return ret;
}

/*++
Function:
    asinf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_asinf(float x)
{
    float ret;
    PERF_ENTRY(asinf);
    ENTRY("Entry to asinf: float x=%f\n", x);

#if !(HAVE_COMPATIBLE_ASIN)
    errno = 0;
#endif  // !(HAVE_COMPATIBLE_ASIN)
    
    ret = asinf(x);
    
#if !(HAVE_COMPATIBLE_ASIN)
    if (errno == EDOM)
    {
        ret = NAN;
    }
#endif  // !(HAVE_COMPATIBLE_ASIN)

    LOGEXIT("Exit from asinf: float return=%f\n", ret);
    PERF_EXIT(asinf);
    return ret;
}

/*++
Function:
    atan

See MSDN.
--*/
PALIMPORT double __cdecl PAL_atan(double x)
{
    double ret;
    PERF_ENTRY(atan);
    ENTRY("Entry to atan: double x=%f\n", x);

    ret = atan(x);

    LOGEXIT("Exit from atan: double return=%f\n", ret);
    PERF_EXIT(atan);
    return ret;
}

/*++
Function:
    atanf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_atanf(float x)
{
    float ret;
    PERF_ENTRY(atanf);
    ENTRY("Entry to atanf: float x=%f\n", x);

    ret = atanf(x);

    LOGEXIT("Exit from atanf: float return=%f\n", ret);
    PERF_EXIT(atanf);
    return ret;
}

/*++
Function:
    atan2

See MSDN.
--*/
PALIMPORT double __cdecl PAL_atan2(double y, double x)
{
    double ret;
    PERF_ENTRY(atan2);
    ENTRY("Entry to atan2: double y=%f, double x=%f\n", y, x);

#if !(HAVE_COMPATIBLE_ATAN2)
    errno = 0;
#endif  // !(HAVE_COMPATIBLE_ATAN2)
    
    ret = atan2(y, x);
    
#if !(HAVE_COMPATIBLE_ATAN2)
    if ((errno == EDOM) && (x == 0) && (y == 0)))
    {
        if (copysign(1, x) > 0)
        {
            ret = copysign(0, y);
        }
        else
        {
            ret = copysign(M_PI, y);
        }
    }
#endif  // !(HAVE_COMPATIBLE_ATAN2)

    LOGEXIT("Exit from atan2: double return=%f\n", ret);
    PERF_EXIT(atan2);
    return ret;
}

/*++
Function:
    atan2f

See MSDN.
--*/
PALIMPORT float __cdecl PAL_atan2f(float y, float x)
{
    float ret;
    PERF_ENTRY(atan2f);
    ENTRY("Entry to atan2f: float y=%f, float x=%f\n", y, x);

#if !(HAVE_COMPATIBLE_ATAN2)
    errno = 0;
#endif  // !(HAVE_COMPATIBLE_ATAN2)
    
    ret = atan2f(y, x);
    
#if !(HAVE_COMPATIBLE_ATAN2)
    if ((errno == EDOM) && (x == 0) && (y == 0)))
    {
        if (copysign(1, x) > 0)
        {
            ret = copysign(0, y);
        }
        else
        {
            ret = copysign(M_PI, y);
        }
    }
#endif  // !(HAVE_COMPATIBLE_ATAN2)

    LOGEXIT("Exit from atan2f: float return=%f\n", ret);
    PERF_EXIT(atan2f);
    return ret;
}

/*++
Function:
    cos

See MSDN.
--*/
PALIMPORT double __cdecl PAL_cos(double x)
{
    double ret;
    PERF_ENTRY(cos);
    ENTRY("Entry to cos: double x=%f\n", x);

    ret = cos(x);

    LOGEXIT("Exit from cos: double return=%f\n", ret);
    PERF_EXIT(cos);
    return ret;
}

/*++
Function:
    cosf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_cosf(float x)
{
    float ret;
    PERF_ENTRY(cosf);
    ENTRY("Entry to cosf: float x=%f\n", x);

    ret = cosf(x);

    LOGEXIT("Exit from cosf: double return=%f\n", ret);
    PERF_EXIT(cosf);
    return ret;
}

/*++
Function:
    sin

See MSDN.
--*/
PALIMPORT double __cdecl PAL_sin(double x)
{
    double ret;
    PERF_ENTRY(sin);
    ENTRY("Entry to sin: double x=%f\n", x);

    ret = sin(x);

    LOGEXIT("Exit from sin: double return=%f\n", ret);
    PERF_EXIT(sin);
    return ret;
}

/*++
Function:
    sinf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_sinf(float x)
{
    float ret;
    PERF_ENTRY(sinf);
    ENTRY("Entry to sinf: float x=%f\n", x);

    ret = sinf(x);

    LOGEXIT("Exit from sinf: float return=%f\n", ret);
    PERF_EXIT(sinf);
    return ret;
}

/*++
Function:
    tan

See MSDN.
--*/
PALIMPORT double __cdecl PAL_tan(double x)
{
    double ret;
    PERF_ENTRY(tan);
    ENTRY("Entry to tan: double x=%f\n", x);

    ret = tan(x);

    LOGEXIT("Exit from tan: double return=%f\n", ret);
    PERF_EXIT(tan);
    return ret;
}

/*++
Function:
    tanf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_tanf(float x)
{
    float ret;
    PERF_ENTRY(tanf);
    ENTRY("Entry to tanf: float x=%f\n", x);

    ret = tanf(x);

    LOGEXIT("Exit from tanf: float return=%f\n", ret);
    PERF_EXIT(tanf);
    return ret;
}

// 7.12.5 Hyperbolic Functions
/*++
Function:
    acosh

See MSDN.
--*/
PALIMPORT double __cdecl PAL_acosh(double x)
{
    double ret;
    PERF_ENTRY(acosh);
    ENTRY("Entry to acosh: double x=%f\n", x);

    ret = acosh(x);

    LOGEXIT("Exit from acosh: double return=%f\n", ret);
    PERF_EXIT(acosh);
    return ret;
}

/*++
Function:
    acoshf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_acoshf(float x)
{
    float ret;
    PERF_ENTRY(acoshf);
    ENTRY("Entry to acoshf: float x=%f\n", x);

    ret = acoshf(x);

    LOGEXIT("Exit from acoshf: float return=%f\n", ret);
    PERF_EXIT(acoshf);
    return ret;
}

/*++
Function:
    asinh

See MSDN.
--*/
PALIMPORT double __cdecl PAL_asinh(double x)
{
    double ret;
    PERF_ENTRY(asinh);
    ENTRY("Entry to asinh: double x=%f\n", x);

    ret = asinh(x);

    LOGEXIT("Exit from asinh: double return=%f\n", ret);
    PERF_EXIT(asinh);
    return ret;
}

/*++
Function:
    asinhf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_asinhf(float x)
{
    float ret;
    PERF_ENTRY(asinhf);
    ENTRY("Entry to asinhf: float x=%f\n", x);

    ret = asinhf(x);

    LOGEXIT("Exit from asinhf: float return=%f\n", ret);
    PERF_EXIT(asinhf);
    return ret;
}

/*++
Function:
    atanh

See MSDN.
--*/
PALIMPORT double __cdecl PAL_atanh(double x)
{
    double ret;
    PERF_ENTRY(atanh);
    ENTRY("Entry to atanh: double x=%f\n", x);

    ret = atanh(x);

    LOGEXIT("Exit from atanh: double return=%f\n", ret);
    PERF_EXIT(atanh);
    return ret;
}

/*++
Function:
    atanhf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_atanhf(float x)
{
    float ret;
    PERF_ENTRY(atanhf);
    ENTRY("Entry to atanhf: float x=%f\n", x);

    ret = atanhf(x);

    LOGEXIT("Exit from atanhf: float return=%f\n", ret);
    PERF_EXIT(atanhf);
    return ret;
}

/*++
Function:
    cosh

See MSDN.
--*/
PALIMPORT double __cdecl PAL_cosh(double x)
{
    double ret;
    PERF_ENTRY(cosh);
    ENTRY("Entry to cosh: double x=%f\n", x);

    ret = cosh(x);

    LOGEXIT("Exit from cosh: double return=%f\n", ret);
    PERF_EXIT(cosh);
    return ret;
}

/*++
Function:
    coshf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_coshf(float x)
{
    float ret;
    PERF_ENTRY(coshf);
    ENTRY("Entry to coshf: float x=%f\n", x);

    ret = coshf(x);

    LOGEXIT("Exit from coshf: double return=%f\n", ret);
    PERF_EXIT(coshf);
    return ret;
}

/*++
Function:
    sinh

See MSDN.
--*/
PALIMPORT double __cdecl PAL_sinh(double x)
{
    double ret;
    PERF_ENTRY(sinh);
    ENTRY("Entry to sinh: double x=%f\n", x);

    ret = sinh(x);

    LOGEXIT("Exit from sinh: double return=%f\n", ret);
    PERF_EXIT(sinh);
    return ret;
}

/*++
Function:
    sinhf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_sinhf(float x)
{
    float ret;
    PERF_ENTRY(sinhf);
    ENTRY("Entry to sinhf: float x=%f\n", x);

    ret = sinhf(x);

    LOGEXIT("Exit from sinhf: float return=%f\n", ret);
    PERF_EXIT(sinhf);
    return ret;
}

/*++
Function:
    tanh

See MSDN.
--*/
PALIMPORT double __cdecl PAL_tanh(double x)
{
    double ret;
    PERF_ENTRY(tanh);
    ENTRY("Entry to tanh: double x=%f\n", x);

    ret = tanh(x);

    LOGEXIT("Exit from tanh: double return=%f\n", ret);
    PERF_EXIT(tanh);
    return ret;
}

/*++
Function:
    tanhf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_tanhf(float x)
{
    float ret;
    PERF_ENTRY(tanhf);
    ENTRY("Entry to tanhf: float x=%f\n", x);

    ret = tanhf(x);

    LOGEXIT("Exit from tanhf: float return=%f\n", ret);
    PERF_EXIT(tanhf);
    return ret;
}

// 7.12.6 Exponential and Logarithmic Functions
/*++
Function:
    exp

See MSDN.
--*/
PALIMPORT double __cdecl PAL_exp(double x)
{
    double ret;
    PERF_ENTRY(exp);
    ENTRY("Entry to exp: double x=%f\n", x);

#if !(HAVE_COMPATIBLE_EXP)
    if (x == 1) 
    {
        ret = M_E;
    }
    else
    {
#endif // !(HAVE_COMPATIBLE_EXP)
    
    ret = exp(x);
    
#if !HAVE_COMPATIBLE_EXP
    }
#endif // !(HAVE_COMPATIBLE_EXP)

    LOGEXIT("Exit from exp: double return=%f\n", ret);
    PERF_EXIT(exp);
    return ret;
}

/*++
Function:
    expf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_expf(float x)
{
    float ret;
    PERF_ENTRY(expf);
    ENTRY("Entry to expf: float x=%f\n", x);

#if !(HAVE_COMPATIBLE_EXP)
    if (x == 1) 
    {
        ret = M_E;
    }
    else
    {
#endif // !(HAVE_COMPATIBLE_EXP)
    
    ret = expf(x);

#if !HAVE_COMPATIBLE_EXP
    }
#endif // !(HAVE_COMPATIBLE_EXP)

    LOGEXIT("Exit from expf: float return=%f\n", ret);
    PERF_EXIT(expf);
    return ret;
}

/*++
Function:
    exp2

See MSDN.
--*/
PALIMPORT double __cdecl PAL_exp2(double x)
{
    double ret;
    PERF_ENTRY(exp2);
    ENTRY("Entry to exp2: double x=%f\n", x);

    ret = exp2(x);

    LOGEXIT("Exit from exp2: double return=%f\n", ret);
    PERF_EXIT(exp2);
    return ret;
}

/*++
Function:
    exp2f

See MSDN.
--*/
PALIMPORT float __cdecl PAL_exp2f(float x)
{
    float ret;
    PERF_ENTRY(exp2f);
    ENTRY("Entry to exp2f: float x=%f\n", x);

    ret = exp2f(x);

    LOGEXIT("Exit from exp2f: float return=%f\n", ret);
    PERF_EXIT(exp2f);
    return ret;
}

/*++
Function:
    expm1

See MSDN.
--*/
PALIMPORT double __cdecl PAL_expm1(double x)
{
    double ret;
    PERF_ENTRY(expm1);
    ENTRY("Entry to expm1: double x=%f\n", x);

    ret = expm1(x);

    LOGEXIT("Exit from expm1: double return=%f\n", ret);
    PERF_EXIT(expm1);
    return ret;
}

/*++
Function:
    expm1f

See MSDN.
--*/
PALIMPORT float __cdecl PAL_expm1f(float x)
{
    float ret;
    PERF_ENTRY(expm1f);
    ENTRY("Entry to expm1f: float x=%f\n", x);

    ret = expm1f(x);

    LOGEXIT("Exit from expm1f: float return=%f\n", ret);
    PERF_EXIT(expm1f);
    return ret;
}

/*++
Function:
    frexp

See MSDN.
--*/
PALIMPORT double __cdecl PAL_frexp(double value, int* exp)
{
    double ret;
    PERF_ENTRY(frexp);
    ENTRY("Entry to frexp: double value=%f, int* exp=%p\n", value, exp);

    ret = frexp(value, exp);

    LOGEXIT("Exit from frexp: double return=%f, *exp=%d\n", ret, *exp);
    PERF_EXIT(frexp);
    return ret;
}

/*++
Function:
    frexpf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_frexpf(float value, int* exp)
{
    float ret;
    PERF_ENTRY(frexpf);
    ENTRY("Entry to frexpf: float value=%f, int* exp=%p\n", value, exp);

    ret = frexpf(value, exp);

    LOGEXIT("Exit from frexpf: float return=%f, *exp=%d\n", ret, *exp);
    PERF_EXIT(frexpf);
    return ret;
}

/*++
Function:
    ldexp

See MSDN.
--*/
PALIMPORT double __cdecl PAL_ldexp(double x, int exp)
{
    double ret;
    PERF_ENTRY(ldexp);
    ENTRY("Entry to ldexp: double x=%f, int exp=%d\n", x, exp);

    ret = ldexp(x, exp);

    LOGEXIT("Exit from ldexp: double return=%f\n", ret);
    PERF_EXIT(ldexp);
    return ret;
}

/*++
Function:
    ldexpf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_ldexpf(float x, int exp)
{
    float ret;
    PERF_ENTRY(ldexpf);
    ENTRY("Entry to ldexpf: float x=%f, int exp=%d\n", x, exp);

    ret = ldexpf(x, exp);

    LOGEXIT("Exit from ldexpf: float return=%f\n", ret);
    PERF_EXIT(ldexpf);
    return ret;
}

/*++
Function:
    log

See MSDN.
--*/
PALIMPORT double __cdecl PAL_log(double x)
{
    double ret;
    PERF_ENTRY(log);
    ENTRY("Entry to log: double x=%f\n", x);

#if !(HAVE_COMPATIBLE_LOG)
    errno = 0;
#endif // !(HAVE_COMPATIBLE_LOG)
    
    ret = log(x);
    
#if !(HAVE_COMPATIBLE_LOG)
    if ((errno == EDOM) && (x < 0))
    {
        ret = NAN;
    }
#endif // !(HAVE_COMPATIBLE_LOG)

    LOGEXIT("Exit from log: double return=%f\n", ret);
    PERF_EXIT(log);
    return ret;
}

/*++
Function:
    logf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_logf(float x)
{
    float ret;
    PERF_ENTRY(logf);
    ENTRY("Entry to logf: float x=%f\n", x);

#if !(HAVE_COMPATIBLE_LOG)
    errno = 0;
#endif // !(HAVE_COMPATIBLE_LOG)
    
    ret = logf(x);
    
#if !(HAVE_COMPATIBLE_LOG)
    if ((errno == EDOM) && (x < 0))
    {
        ret = NAN;
    }
#endif // !(HAVE_COMPATIBLE_LOG)

    LOGEXIT("Exit from logf: float return=%f\n", ret);
    PERF_EXIT(logf);
    return ret;
}

/*++
Function:
    log10

See MSDN.
--*/
PALIMPORT double __cdecl PAL_log10(double x)
{
    double ret;
    PERF_ENTRY(log10);
    ENTRY("Entry to log10: double x=%f\n", x);

#if !(HAVE_COMPATIBLE_LOG10)
    errno = 0;
#endif // !(HAVE_COMPATIBLE_LOG10)
    
    ret = log10(x);
    
#if !(HAVE_COMPATIBLE_LOG10)
    if ((errno == EDOM) && (x < 0))
    {
        ret = NAN;
    }
#endif // !(HAVE_COMPATIBLE_LOG10)

    LOGEXIT("Exit from log10: double return=%f\n", ret);
    PERF_EXIT(log10);
    return ret;
}

/*++
Function:
    log10f

See MSDN.
--*/
PALIMPORT float __cdecl PAL_log10f(float x)
{
    float ret;
    PERF_ENTRY(log10f);
    ENTRY("Entry to log10f: float x=%f\n", x);

#if !(HAVE_COMPATIBLE_LOG10)
    errno = 0;
#endif // !(HAVE_COMPATIBLE_LOG10)
    
    ret = log10f(x);
    
#if !(HAVE_COMPATIBLE_LOG10)
    if ((errno == EDOM) && (x < 0))
    {
        ret = NAN;
    }
#endif // !(HAVE_COMPATIBLE_LOG10)

    LOGEXIT("Exit from log10f: float return=%f\n", ret);
    PERF_EXIT(log10f);
    return ret;
}

/*++
Function:
    log1p

See MSDN.
--*/
PALIMPORT double __cdecl PAL_log1p(double x)
{
    double ret;
    PERF_ENTRY(log1p);
    ENTRY("Entry to log1p: double x=%f\n", x);

    ret = log1p(x);

    LOGEXIT("Exit from log1p: double return=%f\n", ret);
    PERF_EXIT(log1p);
    return ret;
}

/*++
Function:
    log1pf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_log1pf(float x)
{
    float ret;
    PERF_ENTRY(log1pf);
    ENTRY("Entry to log1pf: float x=%f\n", x);

    ret = log1pf(x);

    LOGEXIT("Exit from log1pf: float return=%f\n", ret);
    PERF_EXIT(log1pf);
    return ret;
}

/*++
Function:
    log2

See MSDN.
--*/
PALIMPORT double __cdecl PAL_log2(double x)
{
    double ret;
    PERF_ENTRY(log2);
    ENTRY("Entry to log2: double x=%f\n", x);

    ret = log2(x);

    LOGEXIT("Exit from log2: double return=%f\n", ret);
    PERF_EXIT(log2);
    return ret;
}

/*++
Function:
    log2f

See MSDN.
--*/
PALIMPORT float __cdecl PAL_log2f(float x)
{
    float ret;
    PERF_ENTRY(log2f);
    ENTRY("Entry to log2f: float x=%f\n", x);

    ret = log2f(x);

    LOGEXIT("Exit from log2f: float return=%f\n", ret);
    PERF_EXIT(log2f);
    return ret;
}

/*++
Function:
    modf

See MSDN.
--*/
PALIMPORT double __cdecl PAL_modf(double value, double* iptr)
{
    double ret;
    PERF_ENTRY(modf);
    ENTRY("Entry to modf: double value=%f, double* iptr=%p\n", value, iptr);

    ret = modf(value, iptr);

    LOGEXIT("Exit from modf: double return=%f, *iptr=%f\n", ret, *iptr);
    PERF_EXIT(modf);
    return ret;
}

/*++
Function:
    modff

See MSDN.
--*/
PALIMPORT float __cdecl PAL_modff(float value, float* iptr)
{
    float ret;
    PERF_ENTRY(modff);
    ENTRY("Entry to modff: float value=%f, float* iptr=%p\n", value, iptr);

    ret = modff(value, iptr);

    LOGEXIT("Exit from modff: double return=%f, *iptr=%f\n", ret, *iptr);
    PERF_EXIT(modff);
    return ret;
}

/*++
Function:
    scalbn

See MSDN.
--*/
PALIMPORT double __cdecl PAL_scalbn(double x, int n)
{
    double ret;
    PERF_ENTRY(scalbn);
    ENTRY("Entry to scalbn: double x=%f, int n=%d\n", x, n);

    ret = scalbn(x, n);

    LOGEXIT("Exit from scalbn: double return=%f\n", ret);
    PERF_EXIT(scalbn);
    return ret;
}

/*++
Function:
    scalbnf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_scalbnf(float x, int n)
{
    float ret;
    PERF_ENTRY(scalbnf);
    ENTRY("Entry to scalbnf: float x=%f, int n=%d\n", x, n);

    ret = scalbnf(x, n);

    LOGEXIT("Exit from scalbnf: float return=%f\n", ret);
    PERF_EXIT(scalbnf);
    return ret;
}

// 7.12.7 Power and Absolute-Value Functions
/*++
Function:
    cbrt

See MSDN.
--*/
PALIMPORT double __cdecl PAL_cbrt(double x)
{
    double ret;
    PERF_ENTRY(cbrt);
    ENTRY("Entry to cbrt: double x=%f\n", x);

    ret = cbrt(x);

    LOGEXIT("Exit from cbrt: double return=%f\n", ret);
    PERF_EXIT(cbrt);
    return ret;
}

/*++
Function:
    cbrtf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_cbrtf(float x)
{
    float ret;
    PERF_ENTRY(cbrtf);
    ENTRY("Entry to cbrtf: float x=%f\n", x);

    ret = cbrtf(x);

    LOGEXIT("Exit from cbrtf: float return=%f\n", ret);
    PERF_EXIT(cbrtf);
    return ret;
}

/*++
Function:
    fabs

See MSDN.
--*/
PALIMPORT double __cdecl PAL_fabs(double x)
{
    double ret;
    PERF_ENTRY(fabs);
    ENTRY("Entry to fabs: double x=%f\n", x);

    ret = fabs(x);

    LOGEXIT("Exit from fabs: double return=%f\n", ret);
    PERF_EXIT(fabs);
    return ret;
}

/*++
Function:
    fabsf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_fabsf(float x)
{
    float ret;
    PERF_ENTRY(fabsf);
    ENTRY("Entry to fabsf: float x=%f\n", x);

    ret = fabsf(x);

    LOGEXIT("Exit from fabsf: float return=%f\n", ret);
    PERF_EXIT(fabsf);
    return ret;
}

/*++
Function:
    hypot

See MSDN.
--*/
PALIMPORT double __cdecl PAL_hypot(double x, double y)
{
    double ret;
    PERF_ENTRY(hypot);
    ENTRY("Entry to hypot: double x=%f, double y=%f\n", x, y);

    ret = hypot(x, y);

    LOGEXIT("Exit from hypot: double return=%f\n", ret);
    PERF_EXIT(hypot);
    return ret;
}

/*++
Function:
    hypotf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_hypotf(float x, float y)
{
    float ret;
    PERF_ENTRY(hypotf);
    ENTRY("Entry to hypotf: float x=%f, float y=%f\n", x, y);

    ret = hypotf(x, y);

    LOGEXIT("Exit from hypotf: float return=%f\n", ret);
    PERF_EXIT(hypotf);
    return ret;
}

/*++
Function:
    pow

See MSDN.
--*/
PALIMPORT double __cdecl PAL_pow(double x, double y)
{
    double ret;
    PERF_ENTRY(pow);
    ENTRY("Entry to pow: double x=%f, double y=%f\n", x, y);

#if !(HAVE_COMPATIBLE_POW)
    if (isinf(y) && !isnan(x))
    {
        if ((x == 1) || (x == -1))
        {
            ret = NAN;
        }
        else if ((x > -1) && (x < 1))
        {
            ret = (y == INFINITY) ? 0 : INFINITY;
        }
        else
        {
            ret = (y == INFINITY) ? INFINITY : 0;
        }
    }
    else
#endif // !(HAVE_COMPATIBLE_POW)
    
    if ((y == 0) && isnan(x))
    {
        // Windows returns NaN for pow(NaN, 0), but POSIX specifies
        // a return value of 1 for that case.  We need to return
        // the same result as Windows.
        ret = NAN;
    }
    else
    {
        ret = pow(x, y);
    }
    
#if !(HAVE_VALID_NEGATIVE_INF_POW) && HAVE_VALID_POSITIVE_INF_POW
    if ((ret == INFINITY) && (x < 0) && isfinite(x) && (ceil(y / 2) != floor(y / 2)))
    {
        ret = -INFINITY;
    }
#endif // !(HAVE_VALID_NEGATIVE_INF_POW)

#if !(HAVE_VALID_POSITIVE_INF_POW)
    /*
    * The even/odd test in the if (this one and the one above) used to be ((long long) y % 2 == 0)
    * on SPARC (long long) y for large y (>2**63) is always 0x7fffffff7fffffff, which
    * is an odd number, so the test ((long long) y % 2 == 0) will always fail for
    * large y. Since large double numbers are always even (e.g., the representation of
    * 1E20+1 is the same as that of 1E20, the last .+1. is too insignificant to be part
    * of the representation), this test will always return the wrong result for large y.
    * 
    * The (ceil(y / 2) == floor(y / 2)) test is slower, but more robust.
    */
    if ((ret == -INFINITY) && (x < 0) && isfinite(x) && ceil(y / 2) == floor(y / 2))
    {
        ret = INFINITY;
    }
#endif // !(HAVE_VALID_POSITIVE_INF_POW)

    LOGEXIT("Exit from pow: double return=%f\n", ret);
    PERF_EXIT(pow);
    return ret;
}

/*++
Function:
    powf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_powf(float x, float y)
{
    float ret;
    PERF_ENTRY(powf);
    ENTRY("Entry to powf: float x=%f, float y=%f\n", x, y);
    
#if !(HAVE_COMPATIBLE_POW)
    if (isinf(y) && !isnan(x))
    {
        if ((x == 1) || (x == -1))
        {
            ret = NAN;
        }
        else if ((x > -1) && (x < 1))
        {
            ret = (y == INFINITY) ? 0 : INFINITY;
        }
        else
        {
            ret = (y == INFINITY) ? INFINITY : 0;
        }
    }
    else
#endif // !(HAVE_COMPATIBLE_POW)
    
    if ((y == 0) && isnan(x))
    {
        // Windows returns NaN for pow(NaN, 0), but POSIX specifies
        // a return value of 1 for that case.  We need to return
        // the same result as Windows.
        ret = NAN;
    }
    else
    {
        ret = powf(x, y);
    }
    
#if !(HAVE_VALID_NEGATIVE_INF_POW) && HAVE_VALID_POSITIVE_INF_POW
    if ((ret == INFINITY) && (x < 0) && isfinite(x) && (ceil(y / 2) != floor(y / 2)))
    {
        ret = -INFINITY;
    }
#endif // !(HAVE_VALID_NEGATIVE_INF_POW)

#if !(HAVE_VALID_POSITIVE_INF_POW)
    /*
    * The even/odd test in the if (this one and the one above) used to be ((long long) y % 2 == 0)
    * on SPARC (long long) y for large y (>2**63) is always 0x7fffffff7fffffff, which
    * is an odd number, so the test ((long long) y % 2 == 0) will always fail for
    * large y. Since large double numbers are always even (e.g., the representation of
    * 1E20+1 is the same as that of 1E20, the last .+1. is too insignificant to be part
    * of the representation), this test will always return the wrong result for large y.
    * 
    * The (ceil(y / 2) == floor(y / 2)) test is slower, but more robust.
    */
    if ((ret == -INFINITY) && (x < 0) && isfinite(x) && ceil(y / 2) == floor(y / 2))
    {
        ret = INFINITY;
    }
#endif // !(HAVE_VALID_POSITIVE_INF_POW)

    LOGEXIT("Exit from powf: float return=%f\n", ret);
    PERF_EXIT(powf);
    return ret;
}

/*++
Function:
    sqrt

See MSDN.
--*/
PALIMPORT double __cdecl PAL_sqrt(double x)
{
    double ret;
    PERF_ENTRY(sqrt);
    ENTRY("Entry to sqrt: double x=%f\n", x);

    ret = sqrt(x);

    LOGEXIT("Exit from sqrt: double return=%f\n", ret);
    PERF_EXIT(sqrt);
    return ret;
}

/*++
Function:
    sqrtf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_sqrtf(float x)
{
    float ret;
    PERF_ENTRY(sqrtf);
    ENTRY("Entry to sqrtf: float x=%f\n", x);

    ret = sqrtf(x);

    LOGEXIT("Exit from sqrtf: float return=%f\n", ret);
    PERF_EXIT(sqrtf);
    return ret;
}

// 7.12.8 Error and Gamma Functions
/*++
Function:
    erf

See MSDN.
--*/
PALIMPORT double __cdecl PAL_erf(double x)
{
    double ret;
    PERF_ENTRY(erf);
    ENTRY("Entry to erf: double x=%f\n", x);

    ret = erf(x);

    LOGEXIT("Exit from erf: double return=%f\n", ret);
    PERF_EXIT(erf);
    return ret;
}

/*++
Function:
    erff

See MSDN.
--*/
PALIMPORT float __cdecl PAL_erff(float x)
{
    float ret;
    PERF_ENTRY(erff);
    ENTRY("Entry to erff: float x=%f\n", x);

    ret = erff(x);

    LOGEXIT("Exit from erff: float return=%f\n", ret);
    PERF_EXIT(erff);
    return ret;
}

/*++
Function:
    erfc

See MSDN.
--*/
PALIMPORT double __cdecl PAL_erfc(double x)
{
    double ret;
    PERF_ENTRY(erfc);
    ENTRY("Entry to erfc: double x=%f\n", x);

    ret = erfc(x);

    LOGEXIT("Exit from erfc: double return=%f\n", ret);
    PERF_EXIT(erfc);
    return ret;
}

/*++
Function:
    erfcf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_erfcf(float x)
{
    float ret;
    PERF_ENTRY(erfcf);
    ENTRY("Entry to erfcf: float x=%f\n", x);

    ret = erfcf(x);

    LOGEXIT("Exit from erfcf: float return=%f\n", ret);
    PERF_EXIT(erfcf);
    return ret;
}

/*++
Function:
    lgamma

See MSDN.
--*/
PALIMPORT double __cdecl PAL_lgamma(double x)
{
    double ret;
    PERF_ENTRY(lgamma);
    ENTRY("Entry to lgamma: double x=%f\n", x);

    ret = lgamma(x);

    LOGEXIT("Exit from lgamma: double return=%f\n", ret);
    PERF_EXIT(lgamma);
    return ret;
}

/*++
Function:
    lgammaf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_lgammaf(float x)
{
    float ret;
    PERF_ENTRY(lgammaf);
    ENTRY("Entry to lgammaf: float x=%f\n", x);

    ret = lgammaf(x);

    LOGEXIT("Exit from lgammaf: float return=%f\n", ret);
    PERF_EXIT(lgammaf);
    return ret;
}

/*++
Function:
    tgamma

See MSDN.
--*/
PALIMPORT double __cdecl PAL_tgamma(double x)
{
    double ret;
    PERF_ENTRY(tgamma);
    ENTRY("Entry to tgamma: double x=%f\n", x);

    ret = tgamma(x);

    LOGEXIT("Exit from tgamma: double return=%f\n", ret);
    PERF_EXIT(tgamma);
    return ret;
}

/*++
Function:
    tgammaf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_tgammaf(float x)
{
    float ret;
    PERF_ENTRY(tgammaf);
    ENTRY("Entry to tgammaf: float x=%f\n", x);

    ret = tgammaf(x);

    LOGEXIT("Exit from tgammaf: float return=%f\n", ret);
    PERF_EXIT(tgammaf);
    return ret;
}

// 7.12.9 Nearest Integer Functions
/*++
Function:
    ceil

See MSDN.
--*/
PALIMPORT double __cdecl PAL_ceil(double x)
{
    double ret;
    PERF_ENTRY(ceil);
    ENTRY("Entry to ceil: double x=%f\n", x);

    ret = ceil(x);

    LOGEXIT("Exit from ceil: double return=%f\n", ret);
    PERF_EXIT(ceil);
    return ret;
}

/*++
Function:
    ceilf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_ceilf(float x)
{
    float ret;
    PERF_ENTRY(ceilf);
    ENTRY("Entry to ceilf: float x=%f\n", x);

    ret = ceilf(x);

    LOGEXIT("Exit from ceilf: float return=%f\n", ret);
    PERF_EXIT(ceilf);
    return ret;
}

/*++
Function:
    floor

See MSDN.
--*/
PALIMPORT double __cdecl PAL_floor(double x)
{
    double ret;
    PERF_ENTRY(floor);
    ENTRY("Entry to floor: double x=%f\n", x);

    ret = floor(x);

    LOGEXIT("Exit from floor: double return=%f\n", ret);
    PERF_EXIT(floor);
    return ret;
}

/*++
Function:
    floorf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_floorf(float x)
{
    float ret;
    PERF_ENTRY(floorf);
    ENTRY("Entry to floorf: float x=%f\n", x);

    ret = floorf(x);

    LOGEXIT("Exit from floorf: float return=%f\n", ret);
    PERF_EXIT(floorf);
    return ret;
}

// 7.12.10 Remainder Functions
/*++
Function:
    fmod

See MSDN.
--*/
PALIMPORT double __cdecl PAL_fmod(double x, double y)
{
    double ret;
    PERF_ENTRY(fmod);
    ENTRY("Entry to fmod: double x=%f, double y=%f\n", x, y);

    ret = fmod(x, y);

    LOGEXIT("Exit from fmod: double return=%f\n", ret);
    PERF_EXIT(fmod);
    return ret;
}

/*++
Function:
    fmodf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_fmodf(float x, float y)
{
    float ret;
    PERF_ENTRY(fmodf);
    ENTRY("Entry to fmodf: float x=%f, float y=%f\n", x, y);

    ret = fmodf(x, y);

    LOGEXIT("Exit from fmodf: float return=%f\n", ret);
    PERF_EXIT(fmodf);
    return ret;
}

/*++
Function:
    remquo

See MSDN.
--*/
PALIMPORT double __cdecl PAL_remquo(double x, double y, int* quo)
{
    double ret;
    PERF_ENTRY(remquo);
    ENTRY("Entry to remquo: double x=%f, double y=%f, int* quo=%p\n", x, y, quo);

    ret = remquo(x, y, quo);

    LOGEXIT("Exit from remquo: double return=%f, *quo=%d\n", ret, *quo);
    PERF_EXIT(remquo);
    return ret;
}

/*++
Function:
    remquof

See MSDN.
--*/
PALIMPORT float __cdecl PAL_remquo(float x, float y, int* quo)
{
    float ret;
    PERF_ENTRY(remquof);
    ENTRY("Entry to remquof: float x=%f, float y=%f, int* quo=%p\n", x, y, quo);

    ret = remquof(x, y, quo);

    LOGEXIT("Exit from remquof: float return=%f, *quo=%d\n", ret, *quo);
    PERF_EXIT(remquof);
    return ret;
}

// 7.12.11 Manipulation Functions
/*++
Function:
    copysign

See MSDN.
--*/
PALIMPORT double __cdecl PAL_copysign(double x, double y)
{
    double ret;
    PERF_ENTRY(copysign);
    ENTRY("Entry to copysign: double x=%f, double y=%f\n", x, y);

    ret = copysign(x, y);

    LOGEXIT("Exit from copysign: double return=%f\n", ret);
    PERF_EXIT(copysign);
    return ret;
}

/*++
Function:
    copysignf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_copysignf(float x, float y)
{
    float ret;
    PERF_ENTRY(copysignf);
    ENTRY("Entry to copysignf: float x=%f, float y=%f\n", x, y);

    ret = copysignf(x, y);

    LOGEXIT("Exit from copysignf: float return=%f\n", ret);
    PERF_EXIT(copysignf);
    return ret;
}

/*++
Function:
    nextafter

See MSDN.
--*/
PALIMPORT double __cdecl PAL_nextafter(double x, double y)
{
    double ret;
    PERF_ENTRY(nextafter);
    ENTRY("Entry to nextafter: double x=%f, double y=%f\n", x, y);

    ret = nextafter(x, y);

    LOGEXIT("Exit from nextafter: double return=%f\n", ret);
    PERF_EXIT(nextafter);
    return ret;
}

/*++
Function:
    nextafterf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_nextafterf(float x, float y)
{
    float ret;
    PERF_ENTRY(nextafterf);
    ENTRY("Entry to nextafterf: float x=%f, float y=%f\n", x, y);

    ret = nextafterf(x, y);

    LOGEXIT("Exit from nextafterf: float return=%f\n", ret);
    PERF_EXIT(nextafterf);
    return ret;
}

// 7.12.13 Floating Multiply-Add Function
/*++
Function:
    fma

See MSDN.
--*/
PALIMPORT double __cdecl PAL_fma(double x, double y, double z)
{
    double ret;
    PERF_ENTRY(fma);
    ENTRY("Entry to fma: double x=%f, double y=%f, double z=%f\n", x, y, z);

    ret = fma(x, y, z);

    LOGEXIT("Exit from fma: double return=%f\n", ret);
    PERF_EXIT(fma);
    return ret;
}

/*++
Function:
    fmaf

See MSDN.
--*/
PALIMPORT float __cdecl PAL_fmaf(float x, float y, float z)
{
    float ret;
    PERF_ENTRY(fmaf);
    ENTRY("Entry to fmaf: float x=%f, float y=%f, float z=%f\n", x, y, z);

    ret = fmaf(x, y, z);

    LOGEXIT("Exit from fmaf: float return=%f\n", ret);
    PERF_EXIT(fmaf);
    return ret;
}
