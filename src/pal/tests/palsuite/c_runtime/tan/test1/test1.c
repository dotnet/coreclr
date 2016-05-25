// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*=============================================================================
**
** Source: test1.c
**
** Purpose: Test to ensure that tan return the correct values
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
    double result = tan(value);

    /*
     * The test is valid when the difference between result
     * and expected is less than or equal to variance
     */
    double delta = fabs(result - expected);

    if (delta > variance)
    {
        Fail("tan(%g) returned %20.17g when it should have returned %20.17g",
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
    double result = tan(value);

    if (!_isnan(result))
    {
        Fail("tan(%g) returned %20.17g when it should have returned %20.17g",
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
        {  0,                      0,                      PAL_EPSILON },
        {  0.31830988618379067,    0.32951473309607836,    PAL_EPSILON },       // value:  1 / pi
        {  0.43429448190325183,    0.46382906716062964,    PAL_EPSILON },       // value:  log10(e)
        {  0.63661977236758134,    0.73930295048660405,    PAL_EPSILON },       // value:  2 / pi
        {  0.69314718055994531,    0.83064087786078395,    PAL_EPSILON },       // value:  ln(2)
        {  0.70710678118654752,    0.85451043200960189,    PAL_EPSILON },       // value:  1 / sqrt(2)
        {  0.78539816339744831,    1,                      PAL_EPSILON * 10 },  // value:  pi / 4
        {  1,                      1.5574077246549022,     PAL_EPSILON * 10 },
        {  1.1283791670955126,     2.1108768356626451,     PAL_EPSILON * 10 },  // value:  2 / sqrt(pi)
        {  1.4142135623730950,     6.3341191670421916,     PAL_EPSILON * 10 },  // value:  sqrt(2)
        {  1.4426950408889634,     7.7635756709721848,     PAL_EPSILON * 10 },  // value:  log2(e)
    // SEE BELOW -- {  1.5707963267948966,     PAL_POSINF,             0 },                 // value:  pi / 2
        {  2.3025850929940457,    -1.1134071468135374,     PAL_EPSILON * 10 },  // value:  ln(10)
        {  2.7182818284590452,    -0.45054953406980750,    PAL_EPSILON },       // value:  e
        {  3.1415926535897932,     0,                      PAL_EPSILON },       // value:  pi
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
    
    // -- SPECIAL CASE --
    // Normally, tan(pi / 2) would return PAL_POSINF (atan2(PAL_POSINF) does return (pi / 2)).
    // However, it seems instead (on all supported systems), we get a different number entirely.
    validate( 1.5707963267948966,  16331239353195370.0, 0);
    validate(-1.5707963267948966, -16331239353195370.0, 0);
    
    validate_isnan(PAL_NEGINF);
    validate_isnan(PAL_NAN);
    validate_isnan(PAL_POSINF);

    PAL_Terminate();
    return PASS;
}
