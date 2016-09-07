#ifndef _LOGGER_CONFIG_CONVERSIONS_H_
#define _LOGGER_CONFIG_CONVERSIONS_H_

#include <stdlib.h>
#include <errno.h>

#include <strings.h>

#include "loggerconfig.h"
#include "commonconfig.h"

template<>
LogLevel convert(const char *str)
{
    if (strcasecmp(str, "None") == 0)
    {
        return LogLevel::None;
    }
    else if (strcasecmp(str, "Fatal") == 0)
    {
        return LogLevel::Fatal;
    }
    else if (strcasecmp(str, "Error") == 0)
    {
        return LogLevel::Error;
    }
    else if (strcasecmp(str, "Warn") == 0)
    {
        return LogLevel::Warn;
    }
    else if (strcasecmp(str, "Info") == 0)
    {
        return LogLevel::Info;
    }
    else if (strcasecmp(str, "Debug") == 0)
    {
        return LogLevel::Debug;
    }
    else if (strcasecmp(str, "Trace") == 0)
    {
        return LogLevel::Trace;
    }
    else if (strcasecmp(str, "All") == 0)
    {
        return LogLevel::All;
    }

    // Trying number values.

    long value;
    char *str_end;

    errno = 0;
    value = strtol(str, &str_end, 0);

    if (errno == ERANGE)
    {
        throw bad_conversion("is out of range");
    }

    if (str == str_end)
    {
        throw bad_conversion("incorrect value for type LogLevel");
    }

    if (*str_end != '\0')
    {
        throw bad_conversion("contains not number symbols");
    }

    if (value < static_cast<long>(LogLevel::None) ||
        value > static_cast<long>(LogLevel::All))
    {
        throw bad_conversion("is out of range");
    }

    return static_cast<LogLevel>(value);
}

template<>
LoggerOutputStream convert(const char *str)
{
    if (strcasecmp(str, "Stdout") == 0)
    {
        return LoggerOutputStream::Stdout;
    }
    else if (strcasecmp(str, "Stderr") == 0)
    {
        return LoggerOutputStream::Stderr;
    }
    else if (strcasecmp(str, "File") == 0)
    {
        return LoggerOutputStream::File;
    }
    else
    {
        throw bad_conversion("incorrect value for type LoggerOutputStream");
    }
}

#endif // _LOGGER_CONFIG_CONVERSIONS_H_
