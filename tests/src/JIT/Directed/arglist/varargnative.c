#include <assert.h>
#include <stdarg.h>
#include <stdio.h>
#include <string.h>
#include <stdlib.h>

#ifdef _MSC_VER
#define DLLEXPORT __declspec(dllexport)
#else
#define DLLEXPORT __attribute__((visibility("default")))
#endif // _MSC_VER

/* Structures */

/*
 * struct one_byte_struct (4 bytes)
*/
typedef struct 
{
    int one;
} one_int_struct;

/*
 * struct two_int_struct (8 bytes)
*/
typedef struct 
{
    int one;
    int two;
} two_int_struct;

/*
 * struct one_long_long_struct (8 bytes)
*/
typedef struct 
{
    __int64 one;
} one_long_long_struct;

/*
 * struct two_long_long_struct (16 bytes)
*/
typedef struct 
{
    __int64 one;
    __int64 two;
} two_long_long_struct;

/*
 * struct four_int_struct (16 bytes)
*/
typedef struct 
{
    int one;
    int two;
    int three;
    int four;
} four_int_struct;

/*
 * struct four_long_long_struct (32 bytes)
*/
typedef struct 
{
    __int64 one;
    __int64 two;
    __int64 three;
    __int64 four;
} four_long_long_struct;

/*
 * struct one_float_struct (4 bytes)
*/
typedef struct 
{
    float one;
} one_float_struct;

/*
 * struct two_float_struct (8 bytes)
*/
typedef struct 
{
    float one;
    float two;
} two_float_struct;

/*
 * struct one_double_struct (8 bytes)
*/
typedef struct 
{
    double one;
} one_double_struct;

/*
 * struct two_double_struct (16 bytes)
*/
typedef struct 
{
    double one;
    double two;
} two_double_struct;

/*
 * struct four_float_struct (16 bytes)
*/
typedef struct 
{
    float one;
    float two;
    float three;
    float four;
} four_float_struct;

/*
 * struct four_double_struct (32 bytes)
*/
typedef struct 
{
    double one;
    double two;
    double three;
    double four;
} four_double_struct;

/* Tests */

DLLEXPORT int test_passing_ints(int count, ...)
{
    va_list ap;
    int index, sum;

    va_start(ap, count);

    sum = 0;
    for (index = 0; index < count; ++index)
    {
        sum += va_arg(ap, int);
    }

    va_end(ap);
    return sum;
}

DLLEXPORT __int64 test_passing_longs(int count, ...)
{
    va_list ap;
    int index;
    __int64 sum;

    va_start(ap, count);

    sum = 0;
    for (index = 0; index < count; ++index)
    {
        sum += va_arg(ap, __int64);
    }

    va_end(ap);
    return sum;
}

DLLEXPORT float test_passing_floats(int count, ...)
{
    va_list ap;
    int index;
    float sum;

    va_start(ap, count);

    sum = 0;
    for (index = 0; index < count; ++index)
    {
        sum += va_arg(ap, float);
    }

    va_end(ap);
    return sum;
}

DLLEXPORT double test_passing_doubles(int count, ...)
{
    va_list ap;
    int index;
    double sum;

    va_start(ap, count);

    sum = 0;
    for (index = 0; index < count; ++index)
    {
        sum += va_arg(ap, double);
    }

    va_end(ap);
    return sum;
}

DLLEXPORT __int64 test_passing_int_and_longs(int int_count, int long_count, ...)
{
    va_list ap;
    int index, count;
    __int64 sum;

    printf ("int_count: %d\nlong_count:%d\n", int_count, long_count);

    count = int_count + long_count;
    va_start(ap, long_count);

    sum = 0;
    for (index = 0; index < int_count; ++index)
    {
        sum += va_arg(ap, int);
    }

    for (index = 0; index < long_count; ++index)
    {
        sum += va_arg(ap, __int64);
    }

    va_end(ap);
    return sum;
}

DLLEXPORT double test_passing_floats_and_doubles(int float_count, int double_count, ...)
{
    va_list ap;
    int index, count;
    double sum;

    count = float_count + double_count;
    va_start(ap, double_count);


    sum = 0;
    for (index = 0; index < float_count; ++index)
    {
        sum += va_arg(ap, float);
    }

    for (index = 0; index < double_count; ++index)
    {
        sum += va_arg(ap, double);
    }

    va_end(ap);
    return sum;
}

DLLEXPORT float test_passing_int_and_float(float expected_value, ...)
{
    va_list ap;
    int index, count;
    float sum;

    count = 6;
    va_start(ap, expected_value);


    sum = 0;
    for (index = 0; index < 6; ++index)
    {
        if (index % 2 == 0) {
            sum += va_arg(ap, int);
        }
        else
        {
            sum += va_arg(ap, float);
        }
    }

    va_end(ap);
    return sum;
}

DLLEXPORT double test_passing_long_and_double(double expected_value, ...)
{
    va_list ap;
    int index, count;
    double sum;

    count = 6;
    va_start(ap, expected_value);


    sum = 0;
    for (index = 0; index < 6; ++index)
    {
        if (index % 2 == 0) {
            sum += va_arg(ap, __int64);
        }
        else
        {
            sum += va_arg(ap, double);
        }
    }

    va_end(ap);
    return sum;
}

/*
    Returns: 0 if passed, -1 or 1 if not
*/
DLLEXPORT int check_string_from_format(char* expected, char* format, ...)
{
    va_list ap;
    char ch, temp_ch;
    int success, index, i_temp;
    char* calculated, *temp_str;
    double d_temp;
    char buffer[50];

    index = 0;
    success = 0;
    calculated = (char*)malloc(strlen(expected) + 1);

    va_start(ap, format);

    while (ch = *format++) {
        if ('%' == ch)
        {
            switch (ch = *format++)
            {
                case '%':
                    calculated[index++] = '%';
                    break;
                case 'c':
                case 'd':
                    i_temp = va_arg(ap, __int64);
                    itoa(i_temp, buffer, 10);
                    temp_str = buffer;

                    while (temp_ch = *temp_str++)
                    {
                        calculated[index++] = temp_ch;
                    }

                    break;
                case 'f':
                    d_temp = va_arg(ap, double);
                    
                    snprintf(buffer, 50, "%.2f", d_temp);
                    temp_str = buffer;

                    while (temp_ch = *temp_str++)
                    {
                        calculated[index++] = temp_ch;
                    }
                    
                    break;
            }
        }
        else
        {
            calculated[index++] = ch;
        }
    }

    va_end(ap);
    
    calculated[index] = '\0';

    printf("Expected:   %s\n", expected);
    printf("Calculated: %s\n", calculated);
    
    assert(strlen(expected) == strlen(calculated));

    free(calculated);
    
    return strcmp(expected, calculated);
}

/*
    Returns: 0 if passed, 1 if not
*/
DLLEXPORT int check_passing_struct(int count, ...)
{
    va_list ap;
    int is_b, is_floating, is_mixed, byte_count, struct_count;
    
    int expected_value_i;
    __int64 expected_value_l;
    float expected_value_f;
    double expected_value_d;

    int passed = 0;

    va_start(ap, count);

    is_b = va_arg(ap, int);
    is_floating = va_arg(ap, int);
    is_mixed = va_arg(ap, int);
    byte_count = va_arg(ap, int);
    struct_count = va_arg(ap, int);

    if (!is_floating)
    {
        if (byte_count == 8)
        {
            // Eight byte structs.
            if (is_b)
            {
                // This is one_long_long_struct
                one_long_long_struct s;
                __int64 sum;

                expected_value_l = va_arg(ap, __int64);
                sum = 0;

                while (struct_count--) {
                    s = va_arg(ap, one_long_long_struct);
                    sum += s.one;
                }

                if (sum != expected_value_l) passed = 1;
            }
            else
            {
                // This is two_int_struct
                two_int_struct s;
                int sum;

                expected_value_i = va_arg(ap, int);
                sum = 0;

                while (struct_count--) {
                    s = va_arg(ap, two_int_struct);
                    sum += s.one + s.two;
                }

                if (sum != expected_value_i) passed = 1;
            }
        }
        else if (byte_count == 16)
        {
            // 16 byte structs.
            if (is_b)
            {
                // This is four_int_struct
                four_int_struct s;
                int sum;

                expected_value_i = va_arg(ap, int);
                sum = 0;

                while (struct_count--) {
                    s = va_arg(ap, four_int_struct);
                    sum += s.one + s.two + s.three + s.four;
                }

                if (sum != expected_value_i) passed = 1;
            }
            else
            {
                // This is two_long_long_struct
                two_long_long_struct s;
                __int64 sum;

                expected_value_l = va_arg(ap, __int64);
                sum = 0;

                while (struct_count--) {
                    s = va_arg(ap, two_long_long_struct);
                    sum += s.one + s.two;
                }

                if (sum != expected_value_l) passed = 1;
            }
        }

        else if (byte_count == 32)
        {
            // This is sixteen_byte_struct
            four_long_long_struct s;
            __int64 sum;

            expected_value_l = va_arg(ap, __int64);
            sum = 0;

            while (struct_count--) {
                s = va_arg(ap, four_long_long_struct);
                sum += s.one + s.two + s.three + s.four;
            }

            if (sum != expected_value_l) passed = 1;
        }
    }
    else
    {
        if (byte_count == 8)
        {
            // Eight byte structs.
            if (is_b)
            {
                // This is one_double_struct
                one_double_struct s;
                double sum;

                expected_value_d = va_arg(ap, double);
                sum = 0;

                while (struct_count--) {
                    s = va_arg(ap, one_double_struct);
                    sum += s.one;
                }

                if (sum != expected_value_d) passed = 1;
            }
            else
            {
                // This is two_float_struct
                two_float_struct s;
                float sum;

                expected_value_f = va_arg(ap, float);
                sum = 0;

                while (struct_count--) {
                    s = va_arg(ap, two_float_struct);
                    sum += s.one + s.two;
                }

                if (sum != expected_value_f) passed = 1;
            }
        }
        else if (byte_count == 16)
        {
            // 16 byte structs.
            if (is_b)
            {
                // This is four_float_struct
                four_float_struct s;
                float sum;

                expected_value_f = va_arg(ap, float);
                sum = 0;

                while (struct_count--) {
                    s = va_arg(ap, four_float_struct);
                    sum += s.one + s.two + s.three + s.four;
                }

                if (sum != expected_value_f) passed = 1;
            }
            else
            {
                // This is two_double_struct
                two_double_struct s;
                double sum;

                expected_value_d = va_arg(ap, double);
                sum = 0;

                while (struct_count--) {
                    s = va_arg(ap, two_double_struct);
                    sum += s.one + s.two;
                }

                if (sum != expected_value_d) passed = 1;
            }
        }

        else if (byte_count == 32)
        {
            // This is four_double_struct
            four_double_struct s;
            double sum;

            expected_value_d = va_arg(ap, double);
            sum = 0;

            while (struct_count--) {
                s = va_arg(ap, four_double_struct);
                sum += s.one + s.two + s.three + s.four;
            }

            if (sum != expected_value_d) passed = 1;
        }
    }

    va_end(ap);
    return passed;
}

/*
    Returns: 0 if passed, 1 if not
*/
DLLEXPORT int check_passing_four_sixteen_byte_structs(int count, ...)
{
    va_list ap;
    int passed, index;
    two_long_long_struct s;
    __int64 expected_value, calculated_value;

    passed = 0;
    calculated_value = 0;

    va_start(ap, count);

    expected_value = va_arg(ap, __int64);

    for (index = 0; index < 4; ++index) {
        s = va_arg(ap, two_long_long_struct);

        calculated_value += s.one + s.two;
    }

    va_end(ap);

    passed = expected_value == calculated_value ? 0 : 1;
    return passed;
}