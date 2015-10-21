//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*============================================================================
**
** Source:  test1.c
**
** Purpose: Tests ceilf with simple positive and negative values.  Also tests 
**          extreme cases like extremely small values and positive and 
**          negative infinity.  Makes sure that calling ceilf on NaN returns 
**          NaN
**
**==========================================================================*/

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
    float result = ceilf(value);

    /*
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    float delta = fabs(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("ceilf(%g) returned %10.9g when it should have returned %10.9g",
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
    float result = ceilf(value);

    if (!isnan(result))
    {
        Fail("ceilf(%g) returned %10.9g when it should have returned %10.9g",
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
        /* value            expected     variance */
        {  0.318309886f,    1,           0.00001f },   // value:  1 / pi
        {  0.434294482f,    1,           0.00001f },   // value:  log10(e)
        {  0.636619772f,    1,           0.00001f },   // value:  2 / pi
        {  0.693147181f,    1,           0.00001f },   // value:  ln(2)
        {  0.707106781f,    1,           0.00001f },   // value:  1 / sqrt(2)
        {  0.785398163f,    1,           0.00001f },   // value:  pi / 4
        {  1.12837917f,     2,           0.00001f },   // value:  2 / sqrt(pi)
        {  1.41421356f,     2,           0.00001f },   // value:  sqrt(2)
        {  1.44269504f,     2,           0.00001f },   // value:  log2(e)
        {  1.57079633f,     2,           0.00001f },   // value:  pi / 2
        {  2.30258509f,     3,           0.00001f },   // value:  ln(10)
        {  2.71828183f,     3,           0.00001f },   // value:  e
        {  3.14159265f,     4,           0.00001f },   // value:  pi
		{  INFINITY,        INFINITY,    0 }
    };

    /* PAL initialization */
    if (PAL_Initialize(argc, argv) != 0)
    {
        return FAIL;
    }
	
	validate( 0,     0, 0.000001f);
	validate(-0.0f,  0, 0.000001f);
	
    validate( 1,     1, 0.00001f);
	validate(-1.0,  -1, 0.00001f);

    for (int i = 0; i < (sizeof(tests) / sizeof(struct test)); i++)
    {
        validate( tests[i].value, tests[i].expected,     tests[i].variance);
        validate(-tests[i].value, 1 - tests[i].expected, tests[i].variance);
    }
    
    validate_isnan(NAN);

    PAL_Terminate();
    return PASS;
}
