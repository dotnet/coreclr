#ifndef _LOGGER_CONFIG_H_
#define _LOGGER_CONFIG_H_

#include <vector>
#include <string>

#include "log.h"

//
// The LoggerConfig structure describes configuration of the Logger.
//

enum class LoggerOutputStream
{
    Stdout,
    Stderr,
    File,
};

struct LoggerConfig
{
    // Creates configuration with default values.
    LoggerConfig();

    //
    // Common options.
    //

    // The level of the Logger verbosity. Can be: None, Fatal, Error, Warn,
    // Info, Debug, Trace, All. Each next level of verbosity enables all
    // previous levels.
    LogLevel Level;

    // An output stream where the Logger puts information. Can be: Stdout,
    // Stderr, File.
    LoggerOutputStream OutputStream;

    //
    // File Output options.
    //

    // The name of the file or path to which is used by the Logger to store
    // information when File output stream is used.
    std::string FileName;

    // TODO: other settings for File Output.

    //
    // Validation and verification.
    //

    void Validate();

    std::vector<std::string> Verify();

    const char *Name();
};

#endif // _LOGGER_CONFIG_H_
