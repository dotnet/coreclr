#ifndef _TRACE_LOG_CONFIG_H_
#define _TRACE_LOG_CONFIG_H_

#include <vector>
#include <string>

//
// The TraceLogConfig structure describes configuration of the TraceLog.
//

enum class TraceLogOutputStream
{
    Stdout,
    Stderr,
    File,
};

struct TraceLogConfig
{
    // Creates configuration with default values.
    TraceLogConfig();

    //
    // Common options.
    //

    // An output stream where the TraceLog puts information. Can be: Stdout,
    // Stderr, File.
    TraceLogOutputStream OutputStream;

    //
    // File Output options.
    //

    // The name of the file or path to which is used by the TraceLog to store
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

#endif // _TRACE_LOG_CONFIG_H_
