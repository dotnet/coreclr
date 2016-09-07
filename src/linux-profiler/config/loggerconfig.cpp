#include "commonconfig.h"
#include "loggerconfig.h"

//
// Configuration parameters should be assigned to its defaults here.
//
LoggerConfig::LoggerConfig()
    : Level(LogLevel::Warn)
    , OutputStream(LoggerOutputStream::Stdout)
    , FileName()
{}

void LoggerConfig::Validate()
{
    if (OutputStream == LoggerOutputStream::File && FileName.empty())
    {
        throw config_error("file name is required for file output");
    }
}

std::vector<std::string> LoggerConfig::Verify()
{
    std::vector<std::string> warnings;

    if (!FileName.empty() && OutputStream != LoggerOutputStream::File)
    {
        warnings.push_back("file name is ignored for non-file output");
    }

    return warnings;
}

const char *LoggerConfig::Name()
{
    return "Logger configuration";
}
