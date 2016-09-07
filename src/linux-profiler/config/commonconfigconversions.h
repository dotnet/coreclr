#ifndef _COMMON_CONFIG_CONVERSIONS_H_
#define _COMMON_CONFIG_CONVERSIONS_H_

#include <string>

#include <stdlib.h>
#include <string.h>
#include <errno.h>

#include <strings.h>

#include "commonconfig.h"

template<>
bool convert(const char *str)
{
    if (
        strcmp    (str, "1")        == 0 ||
        strcasecmp(str, "true")     == 0 ||
        strcasecmp(str, "on")       == 0 ||
        strcasecmp(str, "yes")      == 0 ||
        strcasecmp(str, "enabled")  == 0 ||
        strcasecmp(str, "")         == 0
    )
    {
        return true;
    }
    else if (
        strcmp    (str, "0")        == 0 ||
        strcasecmp(str, "false")    == 0 ||
        strcasecmp(str, "off")      == 0 ||
        strcasecmp(str, "no")       == 0 ||
        strcasecmp(str, "disabled") == 0
    )
    {
        return false;
    }
    else
    {
        throw bad_conversion("incorrect value for type bool");
    }
}

template<>
unsigned long convert(const char *str)
{
    unsigned long value;
    char *str_end;

    errno = 0;
    value = strtoul(str, &str_end, 0);

    if (errno == ERANGE)
    {
        throw bad_conversion("is out of range for unsigned long");
    }

    if (str == str_end)
    {
        throw bad_conversion("incorrect value for type unsigned long");
    }

    if (*str_end != '\0')
    {
        throw bad_conversion("contains not number symbols");
    }

    return value;
}

template<>
std::string convert(const char *str)
{
    return str;
}

#endif // _COMMON_CONFIG_CONVERSIONS_H_
