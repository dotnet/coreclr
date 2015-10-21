//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=============================================================================
**
** Source: test1.c
**
** Purpose: Test to ensure that atan return the correct values
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
    double result = atan(value);

    /*
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    double delta = fabs(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("atan(%g) returned %20.17g when it should have returned %20.17g",
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
    double result = atan(value);

    if (!isnan(result))
    {
        Fail("atan(%g) returned %20.17g when it should have returned %20.17g",
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
        {  0.32951473309607836,    0.31830988618379067,    0.000000000000001 },  // expected:  1 / pi
        {  0.45054953406980750,    0.42331082513074800,    0.000000000000001 },  // expected:  pi - e
        {  0.46382906716062964,    0.43429448190325183,    0.000000000000001 },  // expected:  log10(e)
        {  0.73930295048660405,    0.63661977236758134,    0.000000000000001 },  // expected:  2 / pi
        {  0.83064087786078395,    0.69314718055994531,    0.000000000000001 },  // expected:  ln(2)
        {  0.85451043200960189,    0.70710678118654752,    0.000000000000001 },  // expected:  1 / sqrt(2)
        {  1,                      0.78539816339744831,    0.000000000000001 },  // expected:  pi / 4
        {  1.1134071468135374,     0.83900756059574755,    0.000000000000001 },  // expected:  pi - ln(10)
        {  1.5574077246549022,     1,                      0.00000000000001 },
        {  2.1108768356626451,     1.1283791670955126,     0.00000000000001 },   // expected:  2 / sqrt(pi)
        {  6.3341191670421916,     1.4142135623730950,     0.00000000000001 },   // expected:  sqrt(2)
        {  7.7635756709721848,     1.4426950408889634,     0.00000000000001 },   // expected:  log2(e)
        {  INFINITY,               1.5707963267948966,     0.00000000000001 },   // expected:  pi / 2
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
