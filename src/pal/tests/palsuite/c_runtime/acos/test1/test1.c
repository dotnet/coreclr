// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*=============================================================================
**
** Source: test1.c
**
** Purpose: Test to ensure that acos return the correct values
** 
** Dependencies: PAL_Initialize
**               PAL_Terminate
**               Fail
**               fabs
**
**===========================================================================*/

#include <palsuite.h>

// double has a machine epsilon of approx: 2.22e-16. However, due to floating-point precision
// errors, this is too accurate when comparing values computed by different math library implementations.
// Using the single-precision machine epsilon as our PAL_EPSILON should be 'good enough' for the purposes
// of the testing as it ensures we get the expected value and that it is at least 7 digits precise.
#define PAL_EPSILON 1.19e-07

#define PAL_NAN     sqrt(-1.0)
#define PAL_POSINF -log(0.0)
#define PAL_NEGINF  log(0.0)

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
    double result = acos(value);

    /*
     * The test is valid when the difference between result
     * and expected is less than or equal to variance
     */
    double delta = fabs(result - expected);

    if (delta > variance)
    {
        Fail("acos(%g) returned %20.17g when it should have returned %20.17g",
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
    double result = acos(value);

    if (!_isnan(result))
    {
        Fail("acos(%g) returned %20.17g when it should have returned %20.17g",
             value, result, PAL_NAN);
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
        { -1,                      3.1415926535897932,     PAL_EPSILON * 10 },      // expected:  pi
        { -0.91173391478696510,    2.7182818284590452,     PAL_EPSILON * 10 },      // expected:  e
        { -0.66820151019031295,    2.3025850929940457,     PAL_EPSILON * 10 },      // expected:  ln(10)
        {  0,                      1.5707963267948966,     PAL_EPSILON * 10 },      // expected:  pi / 2
        {  0.12775121753523991,    1.4426950408889634,     PAL_EPSILON * 10 },      // expected:  log2(e)
        {  0.15594369476537447,    1.4142135623730950,     PAL_EPSILON * 10 },      // expected:  sqrt(2)
        {  0.42812514788535792,    1.1283791670955126,     PAL_EPSILON * 10 },      // expected:  2 / sqrt(pi)
        {  0.54030230586813972,    1,                      PAL_EPSILON * 10 },
        {  0.70710678118654752,    0.78539816339744831,    PAL_EPSILON },           // expected:  pi / 4,         value:  1 / sqrt(2)
        {  0.76024459707563015,    0.70710678118654752,    PAL_EPSILON },           // expected:  1 / sqrt(2)
        {  0.76923890136397213,    0.69314718055994531,    PAL_EPSILON },           // expected:  ln(2)
        {  0.80410982822879171,    0.63661977236758134,    PAL_EPSILON },           // expected:  2 / pi
        {  0.90716712923909839,    0.43429448190325183,    PAL_EPSILON },           // expected:  log10(e)
        {  0.94976571538163866,    0.31830988618379067,    PAL_EPSILON },           // expected:  1 / pi
        {  1,                      0,                      PAL_EPSILON },
    };

    /* PAL initialization */
    if (PAL_Initialize(argc, argv) != 0)
    {
        return FAIL;
    }

    for (int i = 0; i < (sizeof(tests) / sizeof(struct test)); i++)
    {
        validate(tests[i].value, tests[i].expected, tests[i].variance);
    }
    
    validate_isnan(PAL_NEGINF);
    validate_isnan(PAL_NAN);
    validate_isnan(PAL_POSINF);

    PAL_Terminate();
    return PASS;
}
