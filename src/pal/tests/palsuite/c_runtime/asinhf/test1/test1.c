//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=============================================================================
**
** Source: test1.c
**
** Purpose: Test to ensure that asinhf return the correct values
** 
** Dependencies: PAL_Initialize
**               PAL_Terminate
**               Fail
**               fabsf
**
**===========================================================================*/

#include <palsuite.h>

/**
 * Helper test structure
 */
struct test
{
    float value;     /* value to test the function with */
    float expected;  /* expected result */
    float variance;  /* maximum delta between the expected and actual result */
};

/**
 * validate
 *
 * test validation function
 */
void __cdecl validate(float value, float expected, float variance)
{
    float result = asinhf(value);

    /*
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    float delta = fabsf(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("asinhf(%g) returned %10.9g when it should have returned %10.9g",
             value, result, expected);
    }
}

/**
 * validate
 *
 * test validation function for values returning NaN
 */
void __cdecl validate_isnan(float value)
{
    float result = asinhf(value);

    if (!isnan(result))
    {
        Fail("asinhf(%g) returned %10.9g when it should have returned %10.9g",
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
        /* value            expected         variance */
        {  0,               0,               0.000001f },
        {  0.323712439f,    0.318309886f,    0.000001f },  // expected:  1 / pi
        {  0.448075979f,    0.434294482f,    0.000001f },  // expected:  log10(e)
        {  0.680501678f,    0.636619772f,    0.000001f },  // expected:  2 / pi
        {  0.75f,           0.693147181f,    0.000001f },  // expected:  ln(2)
        {  0.767523145f,    0.707106781f,    0.000001f },  // expected:  1 / sqrt(2)
        {  0.868670961f,    0.785398163f,    0.000001f },  // expected:  pi / 4
        {  1.17520119f,     1,               0.00001f },
        {  1.38354288f,     1.12837917f,     0.00001f },   // expected:  2 / sqrt(pi)
        {  1.93506682f,     1.41421356f,     0.00001f },   // expected:  sqrt(2)
        {  1.99789801f,     1.44269504f,     0.00001f },   // expected:  log2(e)
        {  2.30129890f,     1.57079633f,     0.00001f },   // expected:  pi / 2
        {  4.95f,           2.30258509f,     0.00001f },   // expected:  ln(10)
        {  7.54413710f,     2.71828183f,     0.00001f },   // expected:  e
        {  11.5487394f,     3.14159265f,     0.00001f },   // expected:  pi
        {  INFINITY,        INFINITY,        0 },
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
