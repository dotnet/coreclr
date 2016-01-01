//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=====================================================================
**
** Source:  test1.c
**
** Purpose: Tests exp with a normal set of values.
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
    double result = exp(value);

    /*
     * The test is valid when the difference between the
     * result and the expectation is less than DELTA
     */
    double delta = fabs(result - expected);

    if ((delta != 0) && (delta >= variance))
    {
        Fail("exp(%g) returned %20.17g when it should have returned %20.17g",
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
    double result = exp(value);

    if (!isnan(result))
    {
        Fail("exp(%g) returned %20.17g when it should have returned %20.17g",
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
		{ -INFINITY,               0,                          1e-15 },   // value: -(inf)             expected: 0
		{ -3.1415926535897932,     0.043213918263772250,       1e-16 },   // value: -(pi)
		{ -2.7182818284590452,     0.065988035845312537,       1e-16 },   // value: -(e)
        { -2.3025850929940457,     0.1,                        1e-15 },   // value: -(ln(10))          expected: 1 / 10
        { -1.5707963267948966,     0.20787957635076191,        1e-15 },   // value: -(pi / 2)
        { -1.4426950408889634,     0.23629008834452270,        1e-15 },   // value: -(log2(e))
        { -1.4142135623730950,     0.24311673443421421,        1e-15 },   // value: -(sqrt(2))
        { -1.1283791670955126,     0.32355726390307110,        1e-15 },   // value: -(2 / sqrt(pi))
        { -1,                      0.36787944117144232,        1e-15 },   // value: -(1)
        { -0.78539816339744831,    0.45593812776599624,        1e-15 },   // value: -(pi / 4)
        { -0.70710678118654752,    0.49306869139523979,        1e-15 },   // value: -(1 / sqrt(2))
		{ -0.69314718055994531,    0.5,                        1e-15 },   // value: -(ln(2))           expected: 1 / 2
		{ -0.63661977236758134,    0.52907780826773535,        1e-15 },   // value: -(2 / pi)
		{ -0.43429448190325183,    0.64772148514180065,        1e-15 },   // value: -(log10(e))
	    { -0.31830988618379067,    0.72737734929521647,        1e-15 },   // value: -(1 / pi)
        {  0,                      1,                          1e-14 },   // value:  0                 expected: 1
        {  0.31830988618379067,    1.3748022274393586,         1e-14 },   // value:  1 / pi
        {  0.43429448190325183,    1.5438734439711811,         1e-14 },   // value:  log10(e)
        {  0.63661977236758134,    1.8900811645722220,         1e-14 },   // value:  2 / pi
        {  0.69314718055994531,    2,                          1e-14 },   // value:  ln(2)             expected: 2
        {  0.70710678118654752,    2.0281149816474725,         1e-14 },   // value:  1 / sqrt(2)
        {  0.78539816339744831,    2.1932800507380155,         1e-14 },   // value:  pi / 4
        {  1,                      2.7182818284590452,         1e-14 },	  //                           expected: e
        {  1.1283791670955126,     3.0906430223107976,         1e-14 },   // value:  2 / sqrt(pi)
        {  1.4142135623730950,     4.1132503787829275,         1e-14 },   // value:  sqrt(2)
        {  1.4426950408889634,     4.2320861065570819,         1e-14 },   // value:  log2(e)
        {  1.5707963267948966,     4.8104773809653517,         1e-14 },   // value:  pi / 2
        {  2.3025850929940457,     10,                         1e-13 },   // value:  ln(10)            expected: 10
        {  2.7182818284590452,     15.154262241479264,         1e-13 },   // value:  e
        {  3.1415926535897932,     23.140692632779269,         1e-13 },   // value:  pi
		{  INFINITY,               INFINITY,               0 },           // value:  inf               expected: inf
    };


    if (PAL_Initialize(argc, argv) != 0)
    {
        return FAIL;
    }

    for (int i = 0; i < (sizeof(tests) / sizeof(struct test)); i++)
    {
        validate(tests[i].value, tests[i].expected, tests[i].variance);
    }
	
	validate_isnan(NAN);

    PAL_Terminate();
    return PASS;
}
