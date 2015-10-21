//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=============================================================================
**
** Source: test1.c
**
** Purpose: Test to ensure that fabs return the correct values
** 
** Dependencies: PAL_Initialize
**               PAL_Terminate
**               Fail
**
**===========================================================================*/

#include <palsuite.h>

/**
 * Helper test structure
 */
struct test
{
    double value;     /* value to test the function with */
    double expected;  /* expected result */
    double variance;  /* maximum delta between the expected and actual result */
};

/**
 * validate
 *
 * test validation function
 */
void __cdecl validate(double value, double expected, double variance)
{
    double result = fabs(value);

    /*
     * The test is valid when the difference between the
     * result and the fabsectation is less than DELTA
     */
    double delta = fabs(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("fabs(%g) returned %20.17g when it should have returned %20.17g",
             value, result, expected);
    }
}

/**
 * validate
 *
 * test validation function for values returning NaN
 */
void __cdecl validate_isnan(double value)
{
    double result = fabs(value);

    if (!isnan(result))
    {
        Fail("fabs(%g) returned %20.17g when it should have returned %20.17g",
             value, result, NAN);
    }
}

/**
 * main
 * 
 * executable entry point
 */
INT __cdecl main(INT argc, CHAR **argv)
{
    struct test tests[] = 
    {
        /* value                   expected                variance */
		{ -INFINITY,               INFINITY,               0 },
		{ -3.1415926535897932,     3.1415926535897932,     0.00000000000001 },   // value: -(pi)              expected:  pi
		{ -2.7182818284590452,     2.7182818284590452,     0.00000000000001 },   // value: -(e)               expected:  e
        { -2.3025850929940457,     2.3025850929940457,     0.00000000000001 },   // value: -(ln(10))          expected:  ln(10)
        { -1.5707963267948966,     1.5707963267948966,     0.00000000000001 },   // value: -(pi / 2)          expected:  pi / 2
        { -1.4426950408889634,     1.4426950408889634,     0.00000000000001 },   // value: -(log2(e))         expected:  log2(e)
        { -1.4142135623730950,     1.4142135623730950,     0.00000000000001 },   // value: -(sqrt(2))         expected:  sqrt(2)
        { -1.1283791670955126,     1.1283791670955126,     0.00000000000001 },   // value: -(2 / sqrt(pi))    expected:  2 / sqrt(pi)
        { -1,                      1,                      0.00000000000001 },
        { -0.78539816339744831,    0.78539816339744831,    0.000000000000001 },  // value: -(pi / 4)          expected:  pi / 4
        { -0.70710678118654752,    0.70710678118654752,    0.000000000000001 },  // value: -(1 / sqrt(2))     expected:  1 / sqrt(2)
        { -0.69314718055994531,    0.69314718055994531,    0.000000000000001 },  // value: -(ln(2))           expected:  ln(2)
        { -0.63661977236758134,    0.63661977236758134,    0.000000000000001 },  // value: -(2 / pi)          expected:  2 / pi
        { -0.43429448190325183,    0.43429448190325183,    0.000000000000001 },  // value: -(log10(e))        expected:  log10(e)
        { -0.31830988618379067,    0.31830988618379067,    0.000000000000001 },  // value: -(1 / pi)          expected:  1 / pi
        { -0.0,                    0,                      0.000000000000001 },
    };


    // PAL initialization
    if (PAL_Initialize(argc, argv) != 0)
    {
        return FAIL;
    }

    for (int i = 0; i < (sizeof(tests) / sizeof(struct test)); i++)
    {
        validate( tests[i].value, tests[i].expected, tests[i].variance);
        validate(-tests[i].value, tests[i].expected, tests[i].variance);
    }
    
    validate_isnan(NAN);

    PAL_Terminate();
    return PASS;
}
