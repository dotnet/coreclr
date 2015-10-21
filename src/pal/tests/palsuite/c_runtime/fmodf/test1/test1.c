//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

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
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    float delta = fabsf(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("fmodf(%g, %g) returned %20.15g when it should have returned %20.15g",
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

    if (!isnan(result))
    {
        Fail("fmodf(%g, %g) returned %20.15g when it should have returned %20.15g",
             numerator, denominator, result, NAN);
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
		/* numerator        denominator      expected          variance */
		{  0,               INFINITY,        0,                0.000001f },
		{  0.312961796f,    0.949765715f,    0.312961796f,     0.000001f },
        {  0.420770483f,    0.907167129f,    0.420770483f,     0.000001f },
        {  0.594480769f,    0.804109828f,    0.594480769f,     0.000001f },
        {  0.638961276f,    0.769238901f,    0.638961276f,     0.000001f },
        {  0.649636939f,    0.760244597f,    0.649636939f,     0.000001f },
        {  0.707106781f,    0.707106781f,    0,                0.000001f },
		{  1,               1,               0,                0.000001f },
        {  0.841470985f,    0.540302306f,    0.301168679f,     0.000001f },
        {  0.903719457f,    0.428125148f,    0.0474691617f,    0.0000001f },
        {  0.987765946f,    0.155943695f,    0.0521037774f,    0.0000001f },
        {  0.991806244f,    0.127751218f,    0.0975477216f,    0.0000001f },
		{  0.743980337f,   -0.668201510f,    0.0757788268f,    0.0000001f },
		{  0.410781291f,   -0.911733915f,    0.410781291f,     0.000001f },
		{  0,              -1,               0,                0.000001f },
		{  1,               INFINITY,        1,                0.00001f },
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
	
	validate_isnan( INFINITY,  INFINITY);
	validate_isnan(-INFINITY,  INFINITY);
	validate_isnan( INFINITY, -INFINITY);
	validate_isnan(-INFINITY, -INFINITY);
	
	validate_isnan( INFINITY,  0);
	validate_isnan(-INFINITY,  0);
	validate_isnan( INFINITY, -0.0f);
	validate_isnan(-INFINITY, -0.0f);
	
	validate_isnan( INFINITY,  1);
	validate_isnan(-INFINITY,  1);
	validate_isnan( INFINITY, -1);
	validate_isnan(-INFINITY, -1);
	
    PAL_Terminate();
    return PASS;
}
