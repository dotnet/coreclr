//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=====================================================================
**
** Source:  test1.c
**
** Purpose: Tests that atan2f returns correct values for a subset of values.
**          Tests with positive and negative values of x and y to ensure
**          atan2f is returning results from the correct quadrant.
**
**
**===================================================================*/

#include <palsuite.h>

struct test
{
	float y;         /* second component of the value to test the function with */
    float x;         /* first component of the value to test the function with */
    float expected;  /* expected result */
	float variance;  /* maximum delta between the expected and actual result */
};

/**
 * validate
 *
 * test validation function
 */
void __cdecl validate(float y, float x, float expected, float variance)
{
    float result = atan2f(y, x);

    /*
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    float delta = fabsf(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("atan2f(%g, %g) returned %10.9g when it should have returned %10.9g",
             y, x, result, expected);
    }
}

/**
 * validate
 *
 * test validation function for values returning NaN
 */
void __cdecl validate_isnan(float y, float x)
{
    float result = atan2f(y, x);

    if (!isnan(result))
    {
        Fail("atan2f(%g, %g) returned %10.9g when it should have returned %10.9g",
             y, x, result, NAN);
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
		/* y                x                expected         variance */
		{  0,               INFINITY,        0,               0.000001f },
		{  0,               0,               0,               0.000001f },
		{  0.312961796f,    0.949765715f,    0.318309886f,    0.000001f },  // expected:  1 / pi
        {  0.420770483f,    0.907167129f,    0.434294482f,    0.000001f },  // expected:  log10(e)
        {  0.594480769f,    0.804109828f,    0.636619772f,    0.000001f },  // expected:  2 / pi
        {  0.638961276f,    0.769238901f,    0.693147181f,    0.000001f },  // expected:  ln(2)
        {  0.649636939f,    0.760244597f,    0.707106781f,    0.000001f },  // expected:  1 / sqrt(2)
        {  0.707106781f,    0.707106781f,    0.785398163f,    0.000001f },  // expected:  pi / 4,         value:  1 / sqrt(2)
		{  1,               1,               0.785398163f,    0.000001f },  // expected:  pi / 4
		{  INFINITY,        INFINITY,        0.785398163f,    0.000001f },  // expected:  pi / 4
        {  0.841470985f,    0.540302306f,    1,               0.00001f },
        {  0.903719457f,    0.428125148f,    1.128379167f,    0.00001f },   // expected:  2 / sqrt(pi)
        {  0.987765946f,    0.155943695f,    1.414213562f,    0.00001f },   // expected:  sqrt(2)
        {  0.991806244f,    0.127751218f,    1.442695041f,    0.00001f },   // expected:  log2(e)
        {  1,               0,               1.570796327f,    0.00001f },   // expected:  pi / 2
		{  INFINITY,        0,               1.570796327f,    0.00001f },   // expected:  pi / 2
		{  INFINITY,        1,               1.570796327f,    0.00001f },   // expected:  pi / 2
		{  0.743980337f,   -0.668201510f,    2.302585093f,    0.00001f },   // expected:  ln(10)
		{  0.410781291f,   -0.911733915f,    2.718281828f,    0.00001f },   // expected:  e
		{  0,              -1,               3.141592654f,    0.00001f },   // expected:  pi
		{  1,               INFINITY,        0,               0.000001f },
    };

    if (PAL_Initialize(argc, argv) != 0)
    {
        return FAIL;
    }

    for (int i = 0; i < (sizeof(tests) / sizeof(struct test)); i++)
    {
		const float pi = 3.141592654f;
		
        validate( tests[i].y,  tests[i].x,  tests[i].expected,      tests[i].variance);
		validate(-tests[i].y,  tests[i].x, -tests[i].expected,      tests[i].variance);
		validate( tests[i].y, -tests[i].x,  pi - tests[i].expected, tests[i].variance);
		validate(-tests[i].y, -tests[i].x,  tests[i].expected - pi, tests[i].variance);
    }
	
	validate_isnan(-INFINITY, NAN);
	validate_isnan( NAN,     -INFINITY);
	validate_isnan( NAN,      INFINITY);
	validate_isnan( INFINITY, NAN);
	
	validate_isnan(NAN,      -1);
	validate_isnan(NAN,      -0.0f);
	validate_isnan(NAN,       0);
	validate_isnan(NAN,       1);
	
	validate_isnan(-1,        NAN);
	validate_isnan(-0.0f,     NAN);
	validate_isnan( 0,        NAN);
	validate_isnan( 1,        NAN);
	
	validate_isnan(NAN,       NAN);

    PAL_Terminate();
    return PASS;
}
