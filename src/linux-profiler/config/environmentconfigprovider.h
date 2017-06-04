#ifndef _ENVIRONMENT_CONFIG_PROVIDER_H_
#define _ENVIRONMENT_CONFIG_PROVIDER_H_

#include "loggerconfig.h"
#include "tracelogconfig.h"
#include "profilerconfig.h"

class EnvironmentConfigProvider
{
private:
    // Internal template method that takes name of the environment variable and
    // tries to fetch it and convert to type T. If the value of the variable
    // is incorrect, bad_conversion exception should be thrown.
    //
    // FetchValue() will return true on success convertion or false if specified
    // variable is not presented.
    template<typename T>
    bool FetchValue(const char *name, T &value) const;

public:
    //
    // FetchConfig() overrides the configuration with values fetched from the
    // environment. Other values are not changed. The configuration wouldn't
    // chang if exception were thrown. Error messages of runtime exceptions are
    // complemented by information about the variable that caused the problem.
    // config_error class is used for this exceptions.
    //

    void FetchConfig(LoggerConfig &config) const;

    void FetchConfig(TraceLogConfig &config) const;

    void FetchConfig(ProfilerConfig &config) const;
};

#endif // _ENVIRONMENT_CONFIG_PROVIDER_H_
