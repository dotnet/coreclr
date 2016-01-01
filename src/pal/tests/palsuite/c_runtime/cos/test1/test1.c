//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=============================================================================
**
** Source: test1.c
**
** Purpose: Test to ensure that cos return the correct values
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
    double result = cos(value);

    /*
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    double delta = fabs(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("cos(%g) returned %20.17g when it should have returned %20.17g",
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
    double result = cos(value);

    if (!isnan(result))
    {
        Fail("cos(%g) returned %20.17g when it should have returned %20.17g",
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
        {  0,                      1,                      0.00000000000001 },
        {  0.31830988618379067,    0.94976571538163866,    0.000000000000001 },  // value:  1 / pi
        {  0.43429448190325183,    0.90716712923909839,    0.000000000000001 },  // value:  log10(e)
        {  0.63661977236758134,    0.80410982822879171,    0.000000000000001 },  // value:  2 / pi
        {  0.69314718055994531,    0.76923890136397213,    0.000000000000001 },  // value:  ln(2)
        {  0.70710678118654752,    0.76024459707563015,    0.000000000000001 },  // value:  1 / sqrt(2)
        {  0.78539816339744831,    0.70710678118654752,    0.000000000000001 },  // value:  pi / 4,         expected:  1 / sqrt(2)
        {  1,                      0.54030230586813972,    0.000000000000001 },
        {  1.1283791670955126,     0.42812514788535792,    0.000000000000001 },  // value:  2 / sqrt(pi)
        {  1.4142135623730950,     0.15594369476537447,    0.000000000000001 },  // value:  sqrt(2)
        {  1.4426950408889634,     0.12775121753523991,    0.000000000000001 },  // value:  log2(e)
        {  1.5707963267948966,     0,                      0.000000000000001 },  // value:  pi / 2
        {  2.3025850929940457,    -0.66820151019031295,    0.000000000000001 },  // value:  ln(10)
        {  2.7182818284590452,    -0.91173391478696510,    0.000000000000001 },  // value:  e
        {  3.1415926535897932,    -1,                      0.00000000000001 },   // value:  pi
    };

    /* PAL initialization */
    if (PAL_Initialize(argc, argv) != 0)
    {
        return FAIL;
    }

    for (int i = 0; i < (sizeof(tests) / sizeof(struct test)); i++)
    {
        validate( tests[i].value, tests[i].expected, tests[i].variance);
        validate(-tests[i].value, tests[i].expected, tests[i].variance);
    }
    
    validate_isnan(-INFINITY);
    validate_isnan( NAN);
    validate_isnan( INFINITY);

    PAL_Terminate();
    return PASS;
}
