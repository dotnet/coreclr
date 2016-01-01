//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=====================================================================
**
** Source:  test1.c
**
** Purpose: Tests that atan2 returns correct values for a subset of values.
**          Tests with positive and negative values of x and y to ensure
**          atan2 is returning results from the correct quadrant.
**
**
**===================================================================*/

#include <palsuite.h>

struct test
{
	double y;         /* second component of the value to test the function with */
    double x;         /* first component of the value to test the function with */
    double expected;  /* expected result */
	double variance;  /* maximum delta between the expected and actual result */
};

/**
 * validate
 *
 * test validation function
 */
void __cdecl validate(double y, double x, double expected, double variance)
{
    double result = atan2(y, x);

    /*
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    double delta = fabs(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("atan2(%g, %g) returned %20.15g when it should have returned %20.15g",
             y, x, result, expected);
    }
}

/**
 * validate
 *
 * test validation function for values returning NaN
 */
void __cdecl validate_isnan(double y, double x)
{
    double result = atan2(y, x);

    if (!isnan(result))
    {
        Fail("atan2(%g, %g) returned %20.15g when it should have returned %20.15g",
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
		/* y                       x                       expected                variance */
		{  0,                      INFINITY,               0,                      0.000000000000001 },
		{  0,                      0,                      0,                      0.000000000000001 },
		{  0.31296179620778659,    0.94976571538163866,    0.31830988618379067,    0.000000000000001 },  // expected:  1 / pi
        {  0.42077048331375735,    0.90716712923909839,    0.43429448190325183,    0.000000000000001 },  // expected:  log10(e)
        {  0.59448076852482208,    0.80410982822879171,    0.63661977236758134,    0.000000000000001 },  // expected:  2 / pi
        {  0.63896127631363480,    0.76923890136397213,    0.69314718055994531,    0.000000000000001 },  // expected:  ln(2)
        {  0.64963693908006244,    0.76024459707563015,    0.70710678118654752,    0.000000000000001 },  // expected:  1 / sqrt(2)
        {  0.70710678118654752,    0.70710678118654752,    0.78539816339744831,    0.000000000000001 },  // expected:  pi / 4,         value:  1 / sqrt(2)
		{  1,                      1,                      0.78539816339744831,    0.000000000000001 },  // expected:  pi / 4
		{  INFINITY,               INFINITY,               0.78539816339744831,    0.000000000000001 },  // expected:  pi / 4
        {  0.84147098480789651,    0.54030230586813972,    1,                      0.00000000000001 },
        {  0.90371945743584630,    0.42812514788535792,    1.1283791670955126,     0.00000000000001 },   // expected:  2 / sqrt(pi)
        {  0.98776594599273553,    0.15594369476537447,    1.4142135623730950,     0.00000000000001 },   // expected:  sqrt(2)
        {  0.99180624439366372,    0.12775121753523991,    1.4426950408889634,     0.00000000000001 },   // expected:  log2(e)
        {  1,                      0,                      1.5707963267948966,     0.00000000000001 },   // expected:  pi / 2
		{  INFINITY,               0,                      1.5707963267948966,     0.00000000000001 },   // expected:  pi / 2
		{  INFINITY,               1,                      1.5707963267948966,     0.00000000000001 },   // expected:  pi / 2
		{  0.74398033695749319,   -0.66820151019031295,    2.3025850929940457,     0.00000000000001 },   // expected:  ln(10)
		{  0.41078129050290870,   -0.91173391478696510,    2.7182818284590452,     0.00000000000001 },   // expected:  e
		{  0,                     -1,                      3.1415926535897932,     0.00000000000001 },   // expected:  pi
		{  1,                      INFINITY,               0,                      0.000000000000001 },
    };

    if (PAL_Initialize(argc, argv) != 0)
    {
        return FAIL;
    }

    for (int i = 0; i < (sizeof(tests) / sizeof(struct test)); i++)
    {
		const double pi = 3.1415926535897932;
		
        validate( tests[i].y,  tests[i].x,  tests[i].expected,      tests[i].variance);
		validate(-tests[i].y,  tests[i].x, -tests[i].expected,      tests[i].variance);
		validate( tests[i].y, -tests[i].x,  pi - tests[i].expected, tests[i].variance);
		validate(-tests[i].y, -tests[i].x,  tests[i].expected - pi, tests[i].variance);
    }
	
	validate_isnan(-INFINITY, NAN);
	validate_isnan( NAN,     -INFINITY);
	validate_isnan( NAN,      INFINITY);
	validate_isnan( INFINITY, NAN);
	
	validate_isnan( NAN,     -1);
	validate_isnan( NAN,     -0.0);
	validate_isnan( NAN,      0);
	validate_isnan( NAN,      1);
	
	validate_isnan(-1,        NAN);
	validate_isnan(-0.0,      NAN);
	validate_isnan( 0,        NAN);
	validate_isnan( 1,        NAN);
	
	validate_isnan( NAN,      NAN);

    PAL_Terminate();
    return PASS;
}
