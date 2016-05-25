// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*=============================================================================
**
** Source: test1.c
**
** Purpose: Test to ensure that fmodf return the correct values
** 
** Dependencies: PAL_Initialize
**               PAL_Terminate
**               Fail
**               fabsf
**
**===========================================================================*/

#include <palsuite.h>

// single has a machine epsilon of approx: 1.19e-07. However, due to floating-point precision
// errors, this is too accurate when comparing values computed by different math library implementations.
// Using the half-precision machine epsilon as our PAL_EPSILON should be 'good enough' for the purposes
// of the testing as it ensures we get the expected value and that it is at least 4 digits precise.
#define PAL_EPSILON 9.77e-04f

#define PAL_NAN     sqrt(-1.0)
#define PAL_POSINF -log(0.0)
#define PAL_NEGINF  log(0.0)

/**
 * Helper test structure
 */
struct test
{
    float numerator;    /* second component of the value to test the function with */
    float denominator;  /* first component of the value to test the function with */
    float expected;     /* expected result */
    float variance;     /* maximum delta between the expected and actual result */
};

/**
 * validate
 *
 * test validation function
 */
void __cdecl validate(float numerator, float denominator, float expected, float variance)
{
    float result = fmodf(numerator, denominator);

    /*
     * The test is valid when the difference between result
     * and expected is less than or equal to variance
     */
    float delta = fabsf(result - expected);

    if (delta > variance)
    {
        Fail("fmodf(%g, %g) returned %10.9g when it should have returned %10.9g",
             numerator, denominator, result, expected);
    }
}

/**
 * validate
 *
 * test validation function for values returning NaN
 */
void __cdecl validate_isnan(float numerator, float denominator)
{
    float result = fmodf(numerator, denominator);

    if (!_isnan(result))
    {
        Fail("fmodf(%g, %g) returned %10.9g when it should have returned %10.9g",
             numerator, denominator, result, PAL_NAN);
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
        /* numerator        denominator        expected          variance */
        {  0,               PAL_POSINF,        0,                PAL_EPSILON },
        {  0.312961796f,    0.949765715f,      0.312961796f,     PAL_EPSILON },
        {  0.420770483f,    0.907167129f,      0.420770483f,     PAL_EPSILON },
        {  0.594480769f,    0.804109828f,      0.594480769f,     PAL_EPSILON },
        {  0.638961276f,    0.769238901f,      0.638961276f,     PAL_EPSILON },
        {  0.649636939f,    0.760244597f,      0.649636939f,     PAL_EPSILON },
        {  0.707106781f,    0.707106781f,      0,                PAL_EPSILON },
        {  1,               1,                 0,                PAL_EPSILON },
        {  0.841470985f,    0.540302306f,      0.301168679f,     PAL_EPSILON },
        {  0.903719457f,    0.428125148f,      0.0474691617f,    PAL_EPSILON / 10 },
        {  0.987765946f,    0.155943695f,      0.0521037774f,    PAL_EPSILON / 10 },
        {  0.991806244f,    0.127751218f,      0.0975477216f,    PAL_EPSILON / 10 },
        {  0.743980337f,   -0.668201510f,      0.0757788268f,    PAL_EPSILON / 10 },
        {  0.410781291f,   -0.911733915f,      0.410781291f,     PAL_EPSILON },
        {  0,              -1,                 0,                PAL_EPSILON },
        {  1,               PAL_POSINF,        1,                PAL_EPSILON * 10 },
    };


    // PAL initialization
    if (PAL_Initialize(argc, argv) != 0)
    {
        return FAIL;
    }

    for (int i = 0; i < (sizeof(tests) / sizeof(struct test)); i++)
    {
        validate( tests[i].numerator,  tests[i].denominator,  tests[i].expected, tests[i].variance);
        validate(-tests[i].numerator,  tests[i].denominator, -tests[i].expected, tests[i].variance);
        validate( tests[i].numerator, -tests[i].denominator,  tests[i].expected, tests[i].variance);
        validate(-tests[i].numerator, -tests[i].denominator, -tests[i].expected, tests[i].variance);
    }

    validate_isnan( 0,     0);
    validate_isnan(-0.0f,  0);
    validate_isnan( 0,    -0.0f);
    validate_isnan(-0.0f, -0.0f);
    
    validate_isnan( 1,     0);
    validate_isnan(-1,     0);
    validate_isnan( 1,    -0.0f);
    validate_isnan(-1,    -0.0f);
    
    validate_isnan(PAL_POSINF, PAL_POSINF);
    validate_isnan(PAL_NEGINF, PAL_POSINF);
    validate_isnan(PAL_POSINF, PAL_NEGINF);
    validate_isnan(PAL_NEGINF, PAL_NEGINF);
    
    validate_isnan(PAL_POSINF,  0);
    validate_isnan(PAL_NEGINF,  0);
    validate_isnan(PAL_POSINF, -0.0f);
    validate_isnan(PAL_NEGINF, -0.0f);
    
    validate_isnan(PAL_POSINF,  1);
    validate_isnan(PAL_NEGINF,  1);
    validate_isnan(PAL_POSINF, -1);
    validate_isnan(PAL_NEGINF, -1);
    
    PAL_Terminate();
    return PASS;
}
