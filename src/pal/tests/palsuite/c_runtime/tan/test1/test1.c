//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=============================================================================
**
** Source: test1.c
**
** Purpose: Test to ensure that tan return the correct values
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
    double result = tan(value);

    /*
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    double delta = fabs(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("tan(%g) returned %20.17g when it should have returned %20.17g",
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
    double result = tan(value);

    if (!isnan(result))
    {
        Fail("tan(%g) returned %20.17g when it should have returned %20.17g",
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
        /* value                   expected                variance */
        {  0,                      0,                      0.000000000000001 },
        {  0.31830988618379067,    0.32951473309607836,    0.000000000000001 },  // value:  1 / pi
        {  0.43429448190325183,    0.46382906716062964,    0.000000000000001 },  // value:  log10(e)
        {  0.63661977236758134,    0.73930295048660405,    0.000000000000001 },  // value:  2 / pi
        {  0.69314718055994531,    0.83064087786078395,    0.000000000000001 },  // value:  ln(2)
        {  0.70710678118654752,    0.85451043200960189,    0.000000000000001 },  // value:  1 / sqrt(2)
        {  0.78539816339744831,    1,                      0.00000000000001 },   // value:  pi / 4
        {  1,                      1.5574077246549022,     0.00000000000001 },
        {  1.1283791670955126,     2.1108768356626451,     0.00000000000001 },   // value:  2 / sqrt(pi)
        {  1.4142135623730950,     6.3341191670421916,     0.00000000000001 },   // value:  sqrt(2)
        {  1.4426950408889634,     7.7635756709721848,     0.00000000000001 },   // value:  log2(e)
    // SEE BELOW -- {  1.5707963267948966,     INFINITY,               0 },                  // value:  pi / 2
        {  2.3025850929940457,    -1.1134071468135374,     0.00000000000001 },   // value:  ln(10)
        {  2.7182818284590452,    -0.45054953406980750,    0.000000000000001 },  // value:  e
        {  3.1415926535897932,     0,                      0.000000000000001 },  // value:  pi
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
	// Normally, tan(pi / 2) would return INFINITY (atan2(INFINITY) does return (pi / 2)).
	// However, it seems instead (on all supported systems), we get a different number entirely.
	validate( 1.5707963267948966,  16331239353195370.0, 0);
    validate(-1.5707963267948966, -16331239353195370.0, 0);
    
    validate_isnan(-INFINITY);
    validate_isnan( NAN);
    validate_isnan( INFINITY);

    PAL_Terminate();
    return PASS;
}
