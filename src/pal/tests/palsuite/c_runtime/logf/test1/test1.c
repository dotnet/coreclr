//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=====================================================================
**
** Source:  test1.c
**
** Purpose: Tests logf with a normal set of values.
**
**
**===================================================================*/

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
    float result = logf(value);

    /*
     * The test is valid when the difference between the
     * result and the logfectation is less than DELTA
     */
    float delta = fabsf(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("logf(%g) returned %20.17g when it should have returned %20.17g",
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
    float result = logf(value);

    if (!isnan(result))
    {
        Fail("logf(%g) returned %20.17g when it should have returned %20.17g",
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
        /* value               expected        variance */
        {  0,                 -INFINITY,       0 },      // expected: -(inf)              value: 0
        {  0.0432139183f,     -3.14159265f,    1e-5f },  // expected: -(pi)
        {  0.0659880358f,     -2.71828183f,    1e-5f },  // expected: -(e)
        {  0.1f,              -2.30258509f,    1e-5f },  // expected: -(ln(10))           value: 1 / 10
        {  0.207879576f,      -1.57079633f,    1e-5f },  // expected: -(pi / 2)
        {  0.236290088f,      -1.44269504f,    1e-5f },  // expected: -(log2(e))
        {  0.243116734f,      -1.41421356f,    1e-5f },  // expected: -(sqrt(2))
        {  0.323557264f,      -1.12837917f,    1e-5f },  // expected: -(2 / sqrt(pi))
        {  0.367879441f,      -1,              1e-5f },  // expected: -(1)
        {  0.455938128f,      -0.785398163f,   1e-6f },  // expected: -(pi / 4)
        {  0.493068691f,      -0.707106781f,   1e-6f },  // expected: -(1 / sqrt(2))
        {  0.5f,              -0.693147181f,   1e-6f },  // expected: -(ln(2))            value: 1 / 2
        {  0.529077808f,      -0.636619772f,   1e-6f },  // expected: -(2 / pi)
        {  0.647721485f,      -0.434294482f,   1e-6f },  // expected: -(log10(e))
        {  0.727377349f,      -0.318309886f,   1e-6f },  // expected: -(1 / pi)
        {  1,                  0,              1e-6f },  // expected:  0                  value: 1
        {  1.37480223f,        0.318309886f,   1e-6f },  // expected:  1 / pi
        {  1.54387344f,        0.434294482f,   1e-6f },  // expected:  log10(e)
        {  1.89008116f,        0.636619772f,   1e-6f },  // expected:  2 / pi
        {  2,                  0.693147181f,   1e-6f },  // expected:  ln(2)              value: 2
        {  2.02811498f,        0.707106781f,   1e-6f },  // expected:  1 / sqrt(2)
        {  2.19328005f,        0.785398163f,   1e-6f },  // expected:  pi / 4
        {  2.71828183f,        1,              1e-5f },  // expected:  1                  value: e
        {  3.09064302f,        1.12837917f,    1e-5f },  // expected:  2 / sqrt(pi)
        {  4.11325038f,        1.41421356f,    1e-5f },  // expected:  sqrt(2)
        {  4.23208611f,        1.44269504f,    1e-5f },  // expected:  log2(e)
        {  4.81047738f,        1.57079633f,    1e-5f },  // expected:  pi / 2
        {  10,                 2.30258509f,    1e-5f },  // expected:  ln(10)             value: 10
        {  15.1542622f,        2.71828183f,    1e-5f },  // expected:  e
        {  23.1406926f,        3.14159265f,    1e-5f },  // expected:  pi
        {  INFINITY,           INFINITY,       0 },      // expected:  inf                value: inf
    };


    if (PAL_Initialize(argc, argv) != 0)
    {
        return FAIL;
    }

    for (int i = 0; i < (sizeof(tests) / sizeof(struct test)); i++)
    {
        validate(tests[i].value, tests[i].expected, tests[i].variance);
    }
    
    validate_isnan(-INFINITY);
    validate_isnan( NAN);

    PAL_Terminate();
    return PASS;
}
