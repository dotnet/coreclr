//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=============================================================================
**
** Source: test1.c
**
** Purpose: Test to ensure that asin return the correct values
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
    double result = asin(value);

    /*
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    double delta = fabs(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("asin(%g) returned %20.17g when it should have returned %20.17g",
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
    double result = asin(value);

    if (!isnan(result))
    {
        Fail("asin(%g) returned %20.17g when it should have returned %20.17g",
             value, result, NAN);
    }
}

/**
 * validate
 *
 * test validation function for values returning +INF
 */
void __cdecl validate_isinf_positive(double value)
{
    double result = asin(value);

    if (result != INFINITY)
    {
        Fail("asin(%g) returned %20.17g when it should have returned %20.17g",
             value, result, INFINITY);
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
        {  0.31296179620778659,    0.31830988618379067,    0.000000000000001 },  // expected:  1 / pi
        {  0.41078129050290870,    0.42331082513074800,    0.000000000000001 },  // expected:  pi - e
        {  0.42077048331375735,    0.43429448190325183,    0.000000000000001 },  // expected:  log10(e)
        {  0.59448076852482208,    0.63661977236758134,    0.000000000000001 },  // expected:  2 / pi
        {  0.63896127631363480,    0.69314718055994531,    0.000000000000001 },  // expected:  ln(2)
        {  0.64963693908006244,    0.70710678118654752,    0.000000000000001 },  // expected:  1 / sqrt(2)
        {  0.70710678118654752,    0.78539816339744831,    0.000000000000001 },  // expected:  pi / 4,         value: 1 / sqrt(2)
        {  0.74398033695749319,    0.83900756059574755,    0.000000000000001 },  // expected:  pi - ln(10)
        {  0.84147098480789651,    1,                      0.00000000000001 },
        {  0.90371945743584630,    1.1283791670955126,     0.00000000000001 },   // expected:  2 / sqrt(pi)  v
        {  0.98776594599273553,    1.4142135623730950,     0.00000000000001 },   // expected:  sqrt(2)
        {  0.99180624439366372,    1.4426950408889634,     0.00000000000001 },   // expected:  log2(e)
        {  1,                      1.5707963267948966,     0.00000000000001 },   // expected:  pi / 2
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
