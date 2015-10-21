//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=============================================================================
**
** Source: test1.c
**
** Purpose: Test to ensure that tanf return the correct values
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
    float result = tanf(value);

    /*
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    float delta = fabsf(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("tanf(%g) returned %10.9g when it should have returned %10.9g",
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
    float result = tanf(value);

    if (!isnan(result))
    {
        Fail("tanf(%g) returned %10.9g when it should have returned %10.9g",
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
        {  0.318309886f,    0.329514733f,    0.000001f },  // value:  1 / pi
        {  0.434294482f,    0.463829067f,    0.000001f },  // value:  log10(e)
        {  0.636619772f,    0.739302950f,    0.000001f },  // value:  2 / pi
        {  0.693147181f,    0.830640878f,    0.000001f },  // value:  ln(2)
        {  0.707106781f,    0.854510432f,    0.000001f },  // value:  1 / sqrt(2)
        {  0.785398163f,    1,               0.00001f },   // value:  pi / 4
        {  1,               1.55740772f,     0.00001f },
        {  1.12837917f,     2.11087684f,     0.00001f },   // value:  2 / sqrt(pi)
        {  1.41421356f,     6.33411917f,     0.00001f },   // value:  sqrt(2)
        {  1.44269504f,     7.76357567f,     0.00001f },   // value:  log2(e)
    // SEE BELOW -- {  1.57079633f,     INFINITY,        0 },          // value:  pi / 2
        {  2.30258509f,    -1.11340715f,     0.00001f },   // value:  ln(10)
        {  2.71828183f,    -0.450549534f,    0.000001f },  // value:  e
        {  3.14159265f,     0,               0.000001f },  // value:  pi
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
	// Normally, tanf(pi / 2) would return INFINITY (atan2f(INFINITY) does return (pi / 2)).
	// However, it seems instead (on all supported systems), we get a different number entirely.
	validate( 1.57079633f, -22877332.0f, 0);
    validate(-1.57079633f,  22877332.0f, 0);
    
    validate_isnan(-INFINITY);
    validate_isnan( NAN);
    validate_isnan( INFINITY);

    PAL_Terminate();
    return PASS;
}
