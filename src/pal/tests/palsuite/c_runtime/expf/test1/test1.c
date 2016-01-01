//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=====================================================================
**
** Source:  test1.c
**
** Purpose: Tests expf with a normal set of values.
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
    float result = expf(value);

    /*
     * The test is valid when the difference between the
     * result and the expfectation is less than DELTA
     */
    float delta = fabsf(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("expf(%g) returned %10.9g when it should have returned %10.9g",
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
    float result = expf(value);

    if (!isnan(result))
    {
        Fail("expf(%g) returned %10.9g when it should have returned %10.9g",
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
        /* value          expected             variance */
        { -INFINITY,        0,                  1e-6f },   // value: -(inf)             expected: 0
        { -3.14159265f,     0.0432139183f,      1e-7f },   // value: -(pi)
        { -2.71828183f,     0.0659880358f,      1e-7f },   // value: -(e)
        { -2.30258509f,     0.1f,               1e-6f },   // value: -(ln(10))          expected: 1 / 10
        { -1.57079633f,     0.207879576f,       1e-6f },   // value: -(pi / 2)
        { -1.44269504f,     0.236290088f,       1e-6f },   // value: -(log2(e))
        { -1.41421356f,     0.243116734f,       1e-6f },   // value: -(sqrt(2))
        { -1.12837917f,     0.323557264f,       1e-6f },   // value: -(2 / sqrt(pi))
        { -1,               0.367879441f,       1e-6f },   // value: -(1)
        { -0.785398163f,    0.455938128f,       1e-6f },   // value: -(pi / 4)
        { -0.707106781f,    0.493068691f,       1e-6f },   // value: -(1 / sqrt(2))
        { -0.693147181f,    0.5f,               1e-6f },   // value: -(ln(2))           expected: 1 / 2
        { -0.636619772f,    0.529077808f,       1e-6f },   // value: -(2 / pi)
        { -0.434294482f,    0.647721485f,       1e-6f },   // value: -(log10(e))
        { -0.318309886f,    0.727377349f,       1e-6f },   // value: -(1 / pi)
        {  0,               1,                  1e-5f },   // value:  0                 expected: 1
        {  0.318309886f,    1.37480223f,        1e-5f },   // value:  1 / pi
        {  0.434294482f,    1.54387344f,        1e-5f },   // value:  log10(e)
        {  0.636619772f,    1.89008116f,        1e-5f },   // value:  2 / pi
        {  0.693147181f,    2,                  1e-5f },   // value:  ln(2)             expected: 2
        {  0.707106781f,    2.02811498f,        1e-5f },   // value:  1 / sqrt(2)
        {  0.785398163f,    2.19328005f,        1e-5f },   // value:  pi / 4
        {  1,               2.71828183f,        1e-5f },   // value:  1                 expected: e
        {  1.12837917f,     3.09064302f,        1e-5f },   // value:  2 / sqrt(pi)
        {  1.41421356f,     4.11325038f,        1e-5f },   // value:  sqrt(2)
        {  1.44269504f,     4.23208611f,        1e-5f },   // value:  log2(e)
        {  1.57079633f,     4.81047738f,        1e-5f },   // value:  pi / 2
        {  2.30258509f,     10,                 1e-4f },   // value:  ln(10)            expected: 10
        {  2.71828183f,     15.1542622f,        1e-4f },   // value:  e
        {  3.14159265f,     23.1406926f,        1e-4f },   // value:  pi
        {  INFINITY,        INFINITY,           0 },       // value:  inf               expected: inf
    };


    if (PAL_Initialize(argc, argv) != 0)
    {
        return FAIL;
    }

    for (int i = 0; i < (sizeof(tests) / sizeof(struct test)); i++)
    {
        validate(tests[i].value, tests[i].expected, tests[i].variance);
    }
    
    validate_isnan(NAN);

    PAL_Terminate();
    return PASS;
}
