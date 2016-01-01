//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=============================================================================
**
** Source: test1.c
**
** Purpose: Test to ensure that acosf return the correct values
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
    float result = acosf(value);

    /*
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    float delta = fabsf(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("acosf(%g) returned %10.9g when it should have returned %10.9g",
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
    float result = acosf(value);

    if (!isnan(result))
    {
        Fail("acosf(%g) returned %10.9g when it should have returned %10.9g",
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
        /* value            expected      variance */
        { -1,               3.14159265f,     0.00001f },   // expected:  pi
        { -0.911733915f,    2.71828183f,     0.00001f },   // expected:  e
        { -0.668201510f,    2.30258509f,     0.00001f },   // expected:  ln(10)
        {  0,               1.57079633f,     0.00001f },   // expected:  pi / 2
        {  0.127751218f,    1.44269504f,     0.00001f },   // expected:  log2(e)
        {  0.155943695f,    1.41421356f,     0.00001f },   // expected:  sqrt(2)
        {  0.428125148f,    1.12837917f,     0.00001f },   // expected:  2 / sqrt(pi)
        {  0.540302306f,    1,               0.00001f },
        {  0.707106781f,    0.785398163f,    0.000001f },  // expected:  pi / 4,         value:  1 / sqrt(2)
        {  0.760244597f,    0.707106781f,    0.000001f },  // expected:  1 / sqrt(2)
        {  0.769238901f,    0.693147181f,    0.000001f },  // expected:  ln(2)
        {  0.804109828f,    0.636619772f,    0.000001f },  // expected:  2 / pi
        {  0.907167129f,    0.434294482f,    0.000001f },  // expected:  log10(e)
        {  0.949765715f,    0.318309886f,    0.000001f },  // expected:  1 / pi
        {  1,               0,               0.000001f },
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
    
    validate_isnan(-INFINITY);
    validate_isnan( NAN);
    validate_isnan( INFINITY);

    PAL_Terminate();
    return PASS;
}
