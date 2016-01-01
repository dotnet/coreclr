//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*============================================================================
**
** Source:  test1.c
**
** Purpose: Checks that isfinite correctly classifies all types
**          of floating point numbers (NaN, -Infinity, Infinity,
**          finite nonzero, unnormalized, 0, and -0)
**
**==========================================================================*/

#include <palsuite.h>

/*
The IEEE double precision floating point stanfdard looks like this:

  S EEEEEEEEEEE FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
  0 1        11 12                                                63

S is the sign bit.  The E bits are the expfonent, and the 52 F bits are
the fraction.  These represent a value, V.

If E=2047 and F is nonzero, then V=NaN ("Not a number")
If E=2047 and F is zero and S is 1, then V=-Infinity
If E=2047 and F is zero and S is 0, then V=Infinity
If 0<E<2047 then V=(-1)^S * 2^(E-1023) * (1.F) where "1.F" is the binary
   number created by prefixing F with a leading 1 and a binary point.
If E=0 and F is nonzero, then V=(-1)^S * 2^(-1022) * (0.F) These are
   "unnormalized" values.
If E=0 and F is zero and S is 1, then V=-0
If E=0 and F is zero and S is 0, then V=0

*/

int __cdecl main(int argc, char **argv)
{
    /*non-finite numbers*/
    __int64 lnan =              0x7fffffffffffffff;
    __int64 lnan2 =             0xffffffffffffffff;
    __int64 lneginf =           0xfff0000000000000;
    __int64 linf =              0x7ff0000000000000;

    /*finite numbers*/
    __int64 lUnnormalized =     0x000fffffffffffff;
    __int64 lNegUnnormalized =  0x800fffffffffffff;
    __int64 lNegZero =          0x8000000000000000;

    double nan = *(double *)&lnan;
    double nan2 = *(double *)&lnan2;
    double neginf = *(double *)&lneginf;
    double inf = *(double *)&linf;
    double unnormalized = *(double *)&lUnnormalized;
    double negUnnormalized = *(double *)&lNegUnnormalized;
    double negZero = *(double *)&lNegZero;
    double pos = 123.456;
    double neg = -123.456;

    /*
     * Initialize the PAL and return FAIL if this fails
     */
    if (0 != (PAL_Initialize(argc, argv)))
    {
        return FAIL;
    }

    /*non-finite numbers*/
    if (isfinite(nan) || isfinite(nan2))
    {
        Fail ("isfinite() found NAN to be finite.\n");
    }
    if (isfinite(neginf))
    {
        Fail ("isfinite() found negative infinity to be finite.\n");
    }
    if (isfinite(inf))
    {
        Fail ("isfinite() found infinity to be finite.\n");
    }

    /*finite numbers*/
    if (!isfinite(unnormalized))
    {
        Fail ("isfinite() found an unnormalized value to be infinite.\n");
    }
    if (!isfinite(negUnnormalized))
    {
        Fail ("isfinite() found a negative unnormalized value to be infinite.\n");
    }
    if (!isfinite((double)0))
    {
        Fail ("isfinite found zero to be infinite.\n");
    }
    if (!isfinite(negZero))
    {
        Fail ("isfinite() found negative zero to be infinite.\n");
    }
    if (!isfinite(pos))
    {
        Fail ("isfinite() found %f to be infinite.\n", pos);
    }
    if (!isfinite(neg))
    {
        Fail ("isfinite() found %f to be infinite.\n", neg);
    }
    PAL_Terminate();
    return PASS;
}
