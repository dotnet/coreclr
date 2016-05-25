// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*=============================================================================
**
** Source: test1.c (modf)
**
** Purpose: Test to ensure that modf return the correct values
** 
** Dependencies: PAL_Initialize
**               PAL_Terminate
**               Fail
**               fabs
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
    float value;             /* value to test the function with */
    float expected;          /* expected result */   
    float variance;          /* maximum delta between the expected and actual result */
    float expected_intpart;  /* expected result */
    float variance_intpart;  /* maximum delta between the expected and actual result */
};

/**
 * validate
 *
 * test validation function
 */
void __cdecl validate(float value, float expected, float variance, float expected_intpart, float variance_intpart)
{
    float result_intpart;
    float result = modff(value, &result_intpart);

    /*
     * The test is valid when the difference between result
     * and expected is less than or equal to variance
     */
    float delta = fabsf(result - expected);
    float delta_intpart = fabsf(result_intpart - expected_intpart);

    if ((delta > variance) || (delta_intpart > variance_intpart))
    {
        Fail("modff(%g) returned %10.9g with an intpart of %10.9g when it should have returned %10.9g with an intpart of %10.9g",
             value, result, result_intpart, expected, expected_intpart);
    }
}

/**
 * validate
 *
 * test validation function for values returning NaN
 */
void __cdecl validate_isnan(float value)
{
    float result_intpart;
    float result = modff(value, &result_intpart);

    if (!_isnan(result) || !_isnan(result_intpart))
    {
        Fail("modff(%g) returned %10.9g with an intpart of %10.9g when it should have returned %10.9g with an intpart of %10.9g",
             value, result, result_intpart, PAL_NAN, PAL_NAN);
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
        /* value              expected         variance    expected_intpart    variance_intpart */
        {  0,                 0,               PAL_EPSILON,    0,                  PAL_EPSILON },
        {  0.318309886f,      0.318309886f,    PAL_EPSILON,    0,                  PAL_EPSILON },       // value:  1 / pi
        {  0.434294482f,      0.434294482f,    PAL_EPSILON,    0,                  PAL_EPSILON },       // value:  log10(e)
        {  0.636619772f,      0.636619772f,    PAL_EPSILON,    0,                  PAL_EPSILON },       // value:  2 / pi
        {  0.693147181f,      0.693147181f,    PAL_EPSILON,    0,                  PAL_EPSILON },       // value:  ln(2)
        {  0.707106781f,      0.707106781f,    PAL_EPSILON,    0,                  PAL_EPSILON },       // value:  1 / sqrt(2)
        {  0.785398163f,      0.785398163f,    PAL_EPSILON,    0,                  PAL_EPSILON },       // value:  pi / 4
        {  1,                 0,               PAL_EPSILON,    1,                  PAL_EPSILON * 10 },
        {  1.12837917f,       0.128379167f,    PAL_EPSILON,    1,                  PAL_EPSILON * 10 },  // value:  2 / sqrt(pi)
        {  1.41421356f,       0.414213562f,    PAL_EPSILON,    1,                  PAL_EPSILON * 10 },  // value:  sqrt(2)
        {  1.44269504f,       0.442695041f,    PAL_EPSILON,    1,                  PAL_EPSILON * 10 },  // value:  log2(e)
        {  1.57079633f,       0.570796327f,    PAL_EPSILON,    1,                  PAL_EPSILON * 10 },  // value:  pi / 2
        {  2.30258509f,       0.302585093f,    PAL_EPSILON,    2,                  PAL_EPSILON * 10 },  // value:  ln(10)
        {  2.71828183f,       0.718281828f,    PAL_EPSILON,    2,                  PAL_EPSILON * 10 },  // value:  e
        {  3.14159265f,       0.141592654f,    PAL_EPSILON,    3,                  PAL_EPSILON * 10 },  // value:  pi
        {  PAL_POSINF,        0,               PAL_EPSILON,    PAL_POSINF,         0 }
        
    };

    /* PAL initialization */
    if (PAL_Initialize(argc, argv) != 0)
    {
        return FAIL;
    }

    for (int i = 0; i < (sizeof(tests) / sizeof(struct test)); i++)
    {
        validate( tests[i].value,  tests[i].expected, tests[i].variance,  tests[i].expected_intpart, tests[i].variance_intpart);
        validate(-tests[i].value, -tests[i].expected, tests[i].variance, -tests[i].expected_intpart, tests[i].variance_intpart);
    }

    validate_isnan(PAL_NAN);

    PAL_Terminate();
    return PASS;
}
