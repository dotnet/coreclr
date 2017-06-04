#ifndef _TRACE_LOG_CONFIG_CONVERSIONS_H_
#define _TRACE_LOG_CONFIG_CONVERSIONS_H_

#include <strings.h>

#include "tracelogconfig.h"
#include "commonconfig.h"

template<>
TraceLogOutputStream convert(const char *str)
{
    if (strcasecmp(str, "Stdout") == 0)
    {
        return TraceLogOutputStream::Stdout;
    }
    else if (strcasecmp(str, "Stderr") == 0)
    {
        return TraceLogOutputStream::Stderr;
    }
    else if (strcasecmp(str, "File") == 0)
    {
        return TraceLogOutputStream::File;
    }
    else
    {
        throw bad_conversion("incorrect value for type TraceLogOutputStream");
    }
}

#endif // _TRACE_LOG_CONFIG_CONVERSIONS_H_
