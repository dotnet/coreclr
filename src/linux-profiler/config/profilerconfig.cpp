#include <assert.h>

#include "commonconfig.h"
#include "profilerconfig.h"

//
// Configuration parameters should be assigned to its defaults here.
//
ProfilerConfig::ProfilerConfig()
    : ExecutionTraceEnabled(false)
    , CollectionMethod(CollectionMethod::None)
    , SamplingTimeoutMs(10)
    , HighGranularityEnabled(true)
    , LineTraceEnabled(false)
    , MemoryTraceEnabled(false)
{}

void ProfilerConfig::Validate()
{
    if (CollectionMethod == CollectionMethod::Sampling)
    {
        if (SamplingTimeoutMs == 0)
        {
            throw config_error("sampling timeout should be non-zero");
        }
    }
}

std::vector<std::string> ProfilerConfig::Verify()
{
    std::vector<std::string> warnings;

    if (CollectionMethod != CollectionMethod::None && !ExecutionTraceEnabled)
    {
        warnings.push_back(
            "collection method specification requires execution tracing");
    }

    if (CollectionMethod != CollectionMethod::Instrumentation)
    {
        // Instrumentation specific options verification.
    }
    else if (CollectionMethod != CollectionMethod::Sampling)
    {
        // Sampling specific options verification.

        if (SamplingTimeoutMs != 0)
        {
            warnings.push_back(
                "sampling timeout specification requires sampling");
        }

        if (HighGranularityEnabled)
        {
            // We don't show this message if sampling have been required for
            // line tracing above.
            warnings.push_back("hight granularity option requires sampling");
        }
    }
    else
    {
        assert(CollectionMethod == CollectionMethod::None);
        // Common options verification.

        if (LineTraceEnabled)
        {
            warnings.push_back(
                "line tracing requires sampling or instrumentation");
        }
    }

    return warnings;
}

const char *ProfilerConfig::Name()
{
    return "Profiler configuration";
}
