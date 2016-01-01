//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=============================================================================
**
** Source: test1.c
**
** Purpose: Test to ensure that atanh return the correct values
** 
** Dependencies: PAL_Initialize
**               PAL_Terminate
**               Fail
**               fabs
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
    double result = atanh(value);

    /*
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    double delta = fabs(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("atanh(%g) returned %20.17g when it should have returned %20.17g",
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
    double result = atanh(value);

    if (!isnan(result))
    {
        Fail("atanh(%g) returned %20.17g when it should have returned %20.17g",
             value, result, NAN);
    }
}

/**
 * main
 * 
 * executable entry point
 */
int __cdecl main(int argc, char **argv)
{
    struct test tests[] = 
    {
        /* value                   expected                variance */
        {  0,                      0,                      0.000000000000001 },
        {  0.30797791269089433,    0.31830988618379067,    0.000000000000001 },  // expected:  1 / pi
        {  0.40890401183401433,    0.43429448190325183,    0.000000000000001 },  // expected:  log10(e)
        {  0.56259360033158334,    0.63661977236758134,    0.000000000000001 },  // expected:  2 / pi
        {  0.6,                    0.69314718055994531,    0.000000000000001 },  // expected:  ln(2)
        {  0.60885936501391381,    0.70710678118654752,    0.000000000000001 },  // expected:  1 / sqrt(2)
        {  0.65579420263267244,    0.78539816339744831,    0.000000000000001 },  // expected:  pi / 4
        {  0.76159415595576489,    1,                      0.00000000000001 },
        {  0.81046380599898809,    1.1283791670955126,     0.00000000000001 },   // expected:  2 / sqrt(pi)
        {  0.88838556158566054,    1.4142135623730950,     0.00000000000001 },   // expected:  sqrt(2)
        {  0.89423894585503855,    1.4426950408889634,     0.00000000000001 },   // expected:  log2(e)
        {  0.91715233566727435,    1.5707963267948966,     0.00000000000001 },   // expected:  pi / 2
        {  0.98019801980198020,    2.3025850929940457,     0.00000000000001 },   // expected:  ln(10)
        {  0.99132891580059984,    2.7182818284590452,     0.00000000000001 },   // expected:  e
        {  0.99627207622074994,    3.1415926535897932,     0.00000000000001 },   // expected:  pi
		{  1,                      INFINITY,               0 }
    };

    /* PAL initialization */
    if (PAL_Initialize(argc, argv) != 0)
    {
        return FAIL;
    }

    for (int i = 0; i < (sizeof(tests) / sizeof(struct test)); i++)
    {
        validate( tests[i].value,  tests[i].expected, tests[i].variance);
        validate(-tests[i].value, -tests[i].expected, tests[i].variance);
    }
    
	validate_isnan(-INFINITY);
    validate_isnan( NAN);
	validate_isnan( INFINITY);

    PAL_Terminate();
    return PASS;
}
