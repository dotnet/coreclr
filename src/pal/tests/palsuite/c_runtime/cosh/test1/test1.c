//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=============================================================================
**
** Source: test1.c
**
** Purpose: Test to ensure that cosh return the correct values
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
    double value;      /* value to test the function with */
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
    double result = cosh(value);

    /*
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    double delta = fabs(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("cosh(%g) returned %20.17g when it should have returned %20.17g",
             value, result, expected);
    }
}

/**
 * validate
 *
 * test validation function for values returning NAN
 */
void __cdecl validate_isnan(double value)
{
    double result = cosh(value);

    if (!isnan(result))
    {
        Fail("cosh(%g) returned %20.17g when it should have returned %20.17g",
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
        /* value                   expected             variance */
        {  0,                      1,                     0.00000000000001 },
        {  0.31830988618379067,    1.0510897883672876,    0.00000000000001 },  // value:  1 / pi
        {  0.43429448190325183,    1.0957974645564909,    0.00000000000001 },  // value:  log10(e)
        {  0.63661977236758134,    1.2095794864199787,    0.00000000000001 },  // value:  2 / pi
        {  0.69314718055994531,    1.25,                  0.00000000000001 },  // value:  ln(2)
        {  0.70710678118654752,    1.2605918365213561,    0.00000000000001 },  // value:  1 / sqrt(2)
        {  0.78539816339744831,    1.3246090892520058,    0.00000000000001 },  // value:  pi / 4
        {  1,                      1.5430806348152438,    0.00000000000001 },
        {  1.1283791670955126,     1.7071001431069344,    0.00000000000001 },  // value:  2 / sqrt(pi)
        {  1.4142135623730950,     2.1781835566085709,    0.00000000000001 },  // value:  sqrt(2)
        {  1.4426950408889634,     2.2341880974508023,    0.00000000000001 },  // value:  log2(e)
        {  1.5707963267948966,     2.5091784786580568,    0.00000000000001 },  // value:  pi / 2
        {  2.3025850929940457,     5.05,                  0.00000000000001 },  // value:  ln(10)
        {  2.7182818284590452,     7.6101251386622884,    0.00000000000001 },  // value:  e
        {  3.1415926535897932,     11.591953275521521,    0.0000000000001 },   // value:  pi
        {  INFINITY,               INFINITY,              0 },
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
    
    validate_isnan(NAN);

    PAL_Terminate();
    return PASS;
}
