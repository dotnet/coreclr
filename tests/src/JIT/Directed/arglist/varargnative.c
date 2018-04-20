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
 * struct four_byte_struct (4 bytes)
*/
typedef struct 
{
    int one;
} four_byte_struct;

/*
 * struct eight_byte_struct (8 bytes)
*/
typedef struct 
{
    int one;
    int two;
} eight_byte_struct;

/*
 * struct eight_byte_struct_b (8 bytes)
*/
typedef struct 
{
    long one;
} eight_byte_struct_b;

/*
 * struct sixteen_byte_struct (16 bytes)
*/
typedef struct 
{
    long one;
    long two;
} sixteen_byte_struct;

/*
 * struct sixteen_byte_struct_b (16 bytes)
*/
typedef struct 
{
    int one;
    int two;
    int three;
    int four;
} sixteen_byte_struct_b;

/*
 * struct thirty_two_byte_struct (32 bytes)
*/
typedef struct 
{
    long one;
    long two;
    long three;
    long four;
} thirty_two_byte_struct;

/*
 * struct four_byte_struct_float (4 bytes)
*/
typedef struct 
{
    float one;
} four_byte_struct_float;

/*
 * struct eight_byte_struct_float (8 bytes)
*/
typedef struct 
{
    float one;
    float two;
} eight_byte_struct_float;

/*
 * struct eight_byte_struct_float_b (8 bytes)
*/
typedef struct 
{
    double one;
} eight_byte_struct_float_b;

/*
 * struct sixteen_byte_struct_float (16 bytes)
*/
typedef struct 
{
    double one;
    double two;
} sixteen_byte_struct_float;

/*
 * struct sixteen_byte_struct_float_b (16 bytes)
*/
typedef struct 
{
    float one;
    float two;
    float three;
    float four;
} sixteen_byte_struct_float_b;

/*
 * struct thirty_two_byte_struct_float (32 bytes)
*/
typedef struct 
{
    double one;
    double two;
    double three;
    double four;
} thirty_two_byte_struct_float;

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

DLLEXPORT long test_passing_longs(int count, ...)
{
    va_list ap;
    int index;
    long sum;

    va_start(ap, count);

    sum = 0;
    for (index = 0; index < count; ++index)
    {
        sum += va_arg(ap, long);
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

DLLEXPORT long test_passing_int_and_longs(int int_count, int long_count, ...)
{
    va_list ap;
    int index, count;
    long sum;

    printf ("int_count: %d\nlong_count:%d\n", int_count, long_count);

    count = int_count + long_count;
    va_start(ap, count);

    sum = 0;
    for (index = 0; index < int_count; ++index)
    {
        sum += va_arg(ap, int);
    }

    for (index = 0; index < long_count; ++index)
    {
        sum += va_arg(ap, long);
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
    va_start(ap, count);


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
                    i_temp = va_arg(ap, long);
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

DLLEXPORT int check_passing_struct(int count, ...)
{
    va_list ap;
    int is_b, is_floating, is_mixed, byte_count, struct_count;
    
    int expected_value_i;
    long expected_value_l;
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
                // This is eight_byte_struct_b
                eight_byte_struct_b s;
                long sum;

                expected_value_l = va_arg(ap, long);
                sum = 0;

                while (struct_count--) {
                    s = va_arg(ap, eight_byte_struct_b);
                    sum += s.one;
                }

                if (sum != expected_value_l) passed = 1;
            }
            else
            {
                // This is eight_byte_struct
                eight_byte_struct s;
                int sum;

                expected_value_i = va_arg(ap, int);
                sum = 0;

                while (struct_count--) {
                    s = va_arg(ap, eight_byte_struct);
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
                // This is sixteen_byte_struct_b
                sixteen_byte_struct_b s;
                int sum;

                expected_value_i = va_arg(ap, int);
                sum = 0;

                while (struct_count--) {
                    s = va_arg(ap, sixteen_byte_struct_b);
                    sum += s.one + s.two + s.three + s.four;
                }

                if (sum != expected_value_i) passed = 1;
            }
            else
            {
                // This is sixteen_byte_struct
                sixteen_byte_struct s;
                long sum;

                expected_value_l = va_arg(ap, long);
                sum = 0;

                while (struct_count--) {
                    s = va_arg(ap, sixteen_byte_struct);
                    sum += s.one + s.two;
                }

                if (sum != expected_value_l) passed = 1;
            }
        }

        else if (byte_count == 32)
        {
            // This is sixteen_byte_struct
            thirty_two_byte_struct s;
            long sum;

            expected_value_l = va_arg(ap, long);
            sum = 0;

            while (struct_count--) {
                s = va_arg(ap, thirty_two_byte_struct);
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
                // This is eight_byte_struct_float_b
                eight_byte_struct_float_b s;
                double sum;

                expected_value_d = va_arg(ap, double);
                sum = 0;

                while (struct_count--) {
                    s = va_arg(ap, eight_byte_struct_float_b);
                    sum += s.one;
                }

                if (sum != expected_value_d) passed = 1;
            }
            else
            {
                // This is eight_byte_struct_float
                eight_byte_struct_float s;
                float sum;

                expected_value_f = va_arg(ap, float);
                sum = 0;

                while (struct_count--) {
                    s = va_arg(ap, eight_byte_struct_float);
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
                // This is sixteen_byte_struct_float_b
                sixteen_byte_struct_float_b s;
                float sum;

                expected_value_f = va_arg(ap, float);
                sum = 0;

                while (struct_count--) {
                    s = va_arg(ap, sixteen_byte_struct_float_b);
                    sum += s.one + s.two + s.three + s.four;
                }

                if (sum != expected_value_f) passed = 1;
            }
            else
            {
                // This is sixteen_byte_struct_float
                sixteen_byte_struct_float s;
                double sum;

                expected_value_d = va_arg(ap, double);
                sum = 0;

                while (struct_count--) {
                    s = va_arg(ap, sixteen_byte_struct_float);
                    sum += s.one + s.two;
                }

                if (sum != expected_value_d) passed = 1;
            }
        }

        else if (byte_count == 32)
        {
            // This is thirty_two_byte_struct_float
            thirty_two_byte_struct_float s;
            double sum;

            expected_value_d = va_arg(ap, double);
            sum = 0;

            while (struct_count--) {
                s = va_arg(ap, thirty_two_byte_struct_float);
                sum += s.one + s.two + s.three + s.four;
            }

            if (sum != expected_value_d) passed = 1;
        }
    }

    va_end(ap);
    return passed;
}