#include "commonconfig.h"
#include "tracelogconfig.h"

//
// Configuration parameters should be assigned to its defaults here.
//
TraceLogConfig::TraceLogConfig()
    : OutputStream(TraceLogOutputStream::Stdout)
    , FileName()
{}

void TraceLogConfig::Validate()
{
    if (OutputStream == TraceLogOutputStream::File && FileName.empty())
    {
        throw config_error("file name is required for file output");
    }
}

std::vector<std::string> TraceLogConfig::Verify()
{
    std::vector<std::string> warnings;

    if (!FileName.empty() && OutputStream != TraceLogOutputStream::File)
    {
        warnings.push_back("file name is ignored for non-file output");
    }

    return warnings;
}

const char *TraceLogConfig::Name()
{
    return "TraceLog configuration";
}
