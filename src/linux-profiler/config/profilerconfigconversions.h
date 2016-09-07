#ifndef _PROFILER_CONFIG_CONVERSIONS_H_
#define _PROFILER_CONFIG_CONVERSIONS_H_

#include <string.h>
#include <strings.h>

#include "profilerconfig.h"
#include "commonconfig.h"

template<>
CollectionMethod convert(const char *str)
{
    if (strcasecmp(str, "None") == 0 || strlen(str) == 0)
    {
        return CollectionMethod::None;
    }
    else if (strcasecmp(str, "Instrumentation") == 0)
    {
        return CollectionMethod::Instrumentation;
    }
    else if (strcasecmp(str, "Sampling") == 0)
    {
        return CollectionMethod::Sampling;
    }
    else
    {
        throw bad_conversion("incorrect value for type CollectionMethod");
    }
}

#endif // _PROFILER_CONFIG_CONVERSIONS_H_
