//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=============================================================================
**
** Source: test1.c
**
** Purpose: Test to ensure that asinh return the correct values
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
    double result = asinh(value);

    /*
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    double delta = fabs(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("asinh(%g) returned %20.17g when it should have returned %20.17g",
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
    double result = asinh(value);

    if (!isnan(result))
    {
        Fail("asinh(%g) returned %20.17g when it should have returned %20.17g",
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
        {  0.32371243907207108,    0.31830988618379067,    0.000000000000001 },  // expected:  1 / pi
        {  0.44807597941469025,    0.43429448190325183,    0.000000000000001 },  // expected:  log10(e)
        {  0.68050167815224332,    0.63661977236758134,    0.000000000000001 },  // expected:  2 / pi
        {  0.75,                   0.69314718055994531,    0.000000000000001 },  // expected:  ln(2)
        {  0.76752314512611633,    0.70710678118654752,    0.000000000000001 },  // expected:  1 / sqrt(2)
        {  0.86867096148600961,    0.78539816339744831,    0.000000000000001 },  // expected:  pi / 4
        {  1.1752011936438015,     1,                      0.00000000000001 },
        {  1.3835428792038633,     1.1283791670955126,     0.00000000000001 },   // expected:  2 / sqrt(pi)
        {  1.9350668221743567,     1.4142135623730950,     0.00000000000001 },   // expected:  sqrt(2)
        {  1.9978980091062796,     1.4426950408889634,     0.00000000000001 },   // expected:  log2(e)
        {  2.3012989023072949,     1.5707963267948966,     0.00000000000001 },   // expected:  pi / 2
        {  4.95,                   2.3025850929940457,     0.00000000000001 },   // expected:  ln(10)
        {  7.5441371028169758,     2.7182818284590452,     0.00000000000001 },   // expected:  e
        {  11.548739357257748,     3.1415926535897932,     0.00000000000001 },   // expected:  pi
        {  INFINITY,               INFINITY,               0 },
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

    validate_isnan(NAN);

    PAL_Terminate();
    return PASS;
}
