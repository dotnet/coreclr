//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=============================================================================
**
** Source: test1.c
**
** Purpose: Test to ensure that fmod return the correct values
** 
** Dependencies: PAL_Initialize
**               PAL_Terminate
**               Fail
**               fabs
**
**===========================================================================*/

#include <palsuite.h>

/**
 * Helper test structure
 */
struct test
{
	double numerator;    /* second component of the value to test the function with */
    double denominator;  /* first component of the value to test the function with */
    double expected;     /* expected result */
	double variance;     /* maximum delta between the expected and actual result */
};

/**
 * validate
 *
 * test validation function
 */
void __cdecl validate(double numerator, double denominator, double expected, double variance)
{
    double result = fmod(numerator, denominator);

    /*
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    double delta = fabs(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("fmod(%g, %g) returned %20.15g when it should have returned %20.15g",
             numerator, denominator, result, expected);
    }
}

/**
 * validate
 *
 * test validation function for values returning NaN
 */
void __cdecl validate_isnan(double numerator, double denominator)
{
    double result = fmod(numerator, denominator);

    if (!isnan(result))
    {
        Fail("fmod(%g, %g) returned %20.15g when it should have returned %20.15g",
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
		/* numerator               denominator             expected                variance */
		{  0,                      INFINITY,               0,                       0.000000000000001 },
		{  0.31296179620778659,    0.94976571538163866,    0.31296179620778658,     0.000000000000001 },
        {  0.42077048331375735,    0.90716712923909839,    0.42077048331375733,     0.000000000000001 },
        {  0.59448076852482208,    0.80410982822879171,    0.59448076852482212,     0.000000000000001 },
        {  0.63896127631363480,    0.76923890136397213,    0.63896127631363475,     0.000000000000001 },
        {  0.64963693908006244,    0.76024459707563015,    0.64963693908006248,     0.000000000000001 },
        {  0.70710678118654752,    0.70710678118654752,    0,                       0.000000000000001 },
		{  1,                      1,                      0,                       0.000000000000001 },
        {  0.84147098480789651,    0.54030230586813972,    0.30116867893975674,     0.000000000000001 },
        {  0.90371945743584630,    0.42812514788535792,    0.047469161665130377,    0.0000000000000001 },
        {  0.98776594599273553,    0.15594369476537447,    0.052103777400488605,    0.0000000000000001 },
        {  0.99180624439366372,    0.12775121753523991,    0.097547721646984359,    0.0000000000000001 },
		{  0.74398033695749319,   -0.66820151019031295,    0.075778826767180285,    0.0000000000000001 },
		{  0.41078129050290870,   -0.91173391478696510,    0.41078129050290868,     0.000000000000001 },
		{  0,                     -1,                      0,                       0.000000000000001 },
		{  1,                      INFINITY,               1,                       0.00000000000001 },
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

	validate_isnan( 0,    0);
	validate_isnan(-0.0,  0);
	validate_isnan( 0,   -0.0);
	validate_isnan(-0.0, -0.0);
	
	validate_isnan( 1,    0);
	validate_isnan(-1.0,  0);
	validate_isnan( 1,   -0.0);
	validate_isnan(-1.0, -0.0);
	
	validate_isnan( INFINITY,  INFINITY);
	validate_isnan(-INFINITY,  INFINITY);
	validate_isnan( INFINITY, -INFINITY);
	validate_isnan(-INFINITY, -INFINITY);
	
	validate_isnan( INFINITY,  0);
	validate_isnan(-INFINITY,  0);
	validate_isnan( INFINITY, -0.0);
	validate_isnan(-INFINITY, -0.0);
	
	validate_isnan( INFINITY,  1);
	validate_isnan(-INFINITY,  1);
	validate_isnan( INFINITY, -1.0);
	validate_isnan(-INFINITY, -1.0);
	
    PAL_Terminate();
    return PASS;
}
