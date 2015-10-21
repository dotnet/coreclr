//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=============================================================================
**
** Source: test1.c
**
** Purpose: Test to ensure that acosh return the correct values
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
    double result = acosh(value);

    /*
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    double delta = fabs(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("acosh(%g) returned %20.17g when it should have returned %20.17g",
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
    double result = acosh(value);

    if (!isnan(result))
    {
        Fail("acosh(%g) returned %20.17g when it should have returned %20.17g",
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
        /* value                  expected                variance */
        {  1,                     0,                      0.000000000000001 },
        {  1.0510897883672876,    0.31830988618379067,    0.000000000000001 },  // expected:  1 / pi
        {  1.0957974645564909,    0.43429448190325183,    0.000000000000001 },  // expected:  log10(e)
        {  1.2095794864199787,    0.63661977236758134,    0.000000000000001 },  // expected:  2 / pi
        {  1.25,                  0.69314718055994531,    0.000000000000001 },  // expected:  ln(2)
        {  1.2605918365213561,    0.70710678118654752,    0.000000000000001 },  // expected:  1 / sqrt(2)
        {  1.3246090892520058,    0.78539816339744831,    0.000000000000001 },  // expected:  pi / 4
        {  1.5430806348152438,    1,                      0.00000000000001 },
        {  1.7071001431069344,    1.1283791670955126,     0.00000000000001 },   // expected:  2 / sqrt(pi)
        {  2.1781835566085709,    1.4142135623730950,     0.00000000000001 },   // expected:  sqrt(2)
        {  2.2341880974508023,    1.4426950408889634,     0.00000000000001 },   // expected:  log2(e)
        {  2.5091784786580568,    1.5707963267948966,     0.00000000000001 },   // expected:  pi / 2
        {  5.05,                  2.3025850929940457,     0.00000000000001 },   // expected:  ln(10)
        {  7.6101251386622884,    2.7182818284590452,     0.00000000000001 },   // expected:  e
        {  11.591953275521521,    3.1415926535897932,     0.00000000000001 },   // expected:  pi
        {  INFINITY,              INFINITY,               0 },
    };

    /* PAL initialization */
    if (PAL_Initialize(argc, argv) != 0)
    {
        return FAIL;
    }

    for (int i = 0; i < (sizeof(tests) / sizeof(struct test)); i++)
    {
        validate(tests[i].value, tests[i].expected, tests[i].variance);

        validate_isnan(-tests[i].value);
    }

    validate_isnan( NAN);
    validate_isnan( 0);
    validate_isnan(-INFINITY);

    PAL_Terminate();
    return PASS;
}
