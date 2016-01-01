//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=====================================================================
**
** Source:  test1.c
**
** Purpose: Tests log with a normal set of values.
**
**
**===================================================================*/

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
    double result = log(value);

    /*
     * The test is valid when the difference between the
     * result and the logectation is less than DELTA
     */
    double delta = fabs(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("log(%g) returned %20.17g when it should have returned %20.17g",
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
    double result = log(value);

    if (!isnan(result))
    {
        Fail("log(%g) returned %20.17g when it should have returned %20.17g",
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
        /* value                       expected               variance */
        {  0,                         -INFINITY,              0 },      // expected: -(inf)              value: 0
        {  0.043213918263772250,      -3.1415926535897932,    1e-14 },  // expected: -(pi)
        {  0.065988035845312537,      -2.7182818284590452,    1e-14 },  // expected: -(e)
        {  0.1,                       -2.3025850929940457,    1e-14 },  // expected: -(ln(10))           value: 1 / 10
        {  0.20787957635076191,       -1.5707963267948966,    1e-14 },  // expected: -(pi / 2)
        {  0.23629008834452270,       -1.4426950408889634,    1e-14 },  // expected: -(log2(e))
        {  0.24311673443421421,       -1.4142135623730950,    1e-14 },  // expected: -(sqrt(2))
        {  0.32355726390307110,       -1.1283791670955126,    1e-14 },  // expected: -(2 / sqrt(pi))
        {  0.36787944117144232,       -1,                     1e-14 },  // expected: -(1)
        {  0.45593812776599624,       -0.78539816339744831,   1e-15 },  // expected: -(pi / 4)
        {  0.49306869139523979,       -0.70710678118654752,   1e-15 },  // expected: -(1 / sqrt(2))
        {  0.5,                       -0.69314718055994531,   1e-15 },  // expected: -(ln(2))            value: 1 / 2
        {  0.52907780826773535,       -0.63661977236758134,   1e-15 },  // expected: -(2 / pi)
        {  0.64772148514180065,       -0.43429448190325183,   1e-15 },  // expected: -(log10(e))
        {  0.72737734929521647,       -0.31830988618379067,   1e-15 },  // expected: -(1 / pi)
        {  1,                          0,                     1e-15 },  // expected:  0                  value: 1
        {  1.3748022274393586,         0.31830988618379067,   1e-15 },  // expected:  1 / pi
        {  1.5438734439711811,         0.43429448190325183,   1e-15 },  // expected:  log10(e)
        {  1.8900811645722220,         0.63661977236758134,   1e-15 },  // expected:  2 / pi
        {  2,                          0.69314718055994531,   1e-15 },  // expected:  ln(2)              value: 2
        {  2.0281149816474725,         0.70710678118654752,   1e-15 },  // expected:  1 / sqrt(2)
        {  2.1932800507380155,         0.78539816339744831,   1e-15 },  // expected:  pi / 4
        {  2.7182818284590452,         1,                     1e-14 },  //                               value: e
        {  3.0906430223107976,         1.1283791670955126,    1e-14 },  // expected:  2 / sqrt(pi)
        {  4.1132503787829275,         1.4142135623730950,    1e-14 },  // expected:  sqrt(2)
        {  4.2320861065570819,         1.4426950408889634,    1e-14 },  // expected:  log2(e)
        {  4.8104773809653517,         1.5707963267948966,    1e-14 },  // expected:  pi / 2
        {  10,                         2.3025850929940457,    1e-14 },  // expected:  ln(10)             value: 10
        {  15.154262241479264,         2.7182818284590452,    1e-14 },  // expected:  e
        {  23.140692632779269,         3.1415926535897932,    1e-14 },  // expected:  pi
        {  INFINITY,                   INFINITY,              0 },      // expected:  inf                value: inf
    };


    if (PAL_Initialize(argc, argv) != 0)
    {
        return FAIL;
    }

    for (int i = 0; i < (sizeof(tests) / sizeof(struct test)); i++)
    {
        validate(tests[i].value, tests[i].expected, tests[i].variance);
    }
    
    validate_isnan(-INFINITY);
    validate_isnan( NAN);

    PAL_Terminate();
    return PASS;
}
