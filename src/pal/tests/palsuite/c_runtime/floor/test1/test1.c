//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*============================================================================
**
** Source:  test1.c
**
** Purpose: Tests floor with simple positive and negative values.  Also tests 
**          extreme cases like extremely small values and positive and 
**          negative infinity.  Makes sure that calling floor on NaN returns 
**          NaN
**
**==========================================================================*/

#include <palsuite.h>

/**
 * Helper test structure
 */
struct test
{
    double value;     /* value to test the function with */
    double expected;  /* expected result */
	double variance;  /* maximum delta between the expected and actual result */
};

/**
 * validate
 *
 * test validation function
 */
void __cdecl validate(double value, double expected, double variance)
{
    double result = floor(value);

    /*
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    double delta = fabs(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("floor(%g) returned %20.15g when it should have returned %20.15g",
             value, result, expected);
    }
}

/**
 * validate
 *
 * test validation function for values returning NaN
 */
void __cdecl validate_isnan(double value)
{
    double result = floor(value);

    if (!isnan(result))
    {
        Fail("floor(%g) returned %20.15g when it should have returned %20.15g",
             value, result, NAN);
    }
}

/**
 * main
 * 
 * executable entry point
 */
int __cdecl main(int argc, char *argv[])
{
    struct test tests[] = 
    {
        /* value                   expected     variance */
        {  0.31830988618379067,    0,           0.000000000000001 },   // value:  1 / pi
        {  0.43429448190325183,    0,           0.000000000000001 },   // value:  log10(e)
        {  0.63661977236758134,    0,           0.000000000000001 },   // value:  2 / pi
        {  0.69314718055994531,    0,           0.000000000000001 },   // value:  ln(2)
        {  0.70710678118654752,    0,           0.000000000000001 },   // value:  1 / sqrt(2)
        {  0.78539816339744831,    0,           0.000000000000001 },   // value:  pi / 4
        {  1.1283791670955126,     1,           0.00000000000001 },   // value:  2 / sqrt(pi)
        {  1.4142135623730950,     1,           0.00000000000001 },   // value:  sqrt(2)
        {  1.4426950408889634,     1,           0.00000000000001 },   // value:  log2(e)
        {  1.5707963267948966,     1,           0.00000000000001 },   // value:  pi / 2
        {  2.3025850929940457,     2,           0.00000000000001 },   // value:  ln(10)
        {  2.7182818284590452,     2,           0.00000000000001 },   // value:  e
        {  3.1415926535897932,     3,           0.00000000000001 },   // value:  pi
		{  INFINITY,               INFINITY,    0 }
    };

    /* PAL initialization */
    if (PAL_Initialize(argc, argv) != 0)
    {
        return FAIL;
    }
	
	validate( 0,    0, 0.000000000000001);
	validate(-0.0,  0, 0.000000000000001);
	
    validate( 1,    1, 0.00000000000001);
	validate(-1.0, -1, 0.00000000000001);

    for (int i = 0; i < (sizeof(tests) / sizeof(struct test)); i++)
    {
        validate( tests[i].value,  tests[i].expected,       tests[i].variance);
        validate(-tests[i].value, -(tests[i].expected + 1), tests[i].variance);
    }
    
    validate_isnan(NAN);

    PAL_Terminate();
    return PASS;
}
