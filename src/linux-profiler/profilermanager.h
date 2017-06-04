#ifndef _PROFILER_MANAGER_H_
#define _PROFILER_MANAGER_H_

#include "loggerconfig.h"
#include "tracelogconfig.h"
#include "profilerconfig.h"

class Profiler; // Forward declaration instead of the header inclusion.

class ProfilerManager
{
public:
    // Get the instance of the singleton. It will be instantiated at first call.
    static ProfilerManager &Instance() noexcept;

private:
    // Signleton can be instantiated only by the public Instance() static
    // member function.
    ProfilerManager() noexcept;

    // Singleton can be destroyed only during process termination.
    ~ProfilerManager();

    template<typename T>
    T FetchConfig(const Profiler *pProfiler);

public:
    // Check if the Profiler is registered in the Profiler Manager.
    bool IsProfilerRegistered(const Profiler *pProfiler) const noexcept;

    // Register the Profiler in the Profiler Manager.
    void RegisterProfiler(const Profiler *pProfiler);

    // Remove the Profiler from the Profiler Manager.
    void UnregisterProfiler(const Profiler *pProfiler);

    // Get logger configuration from the Global Area for the specified Profiler.
    LoggerConfig FetchLoggerConfig(const Profiler *pProfiler);

    // Get trace configuration from the Global Area for the specified Profiler.
    TraceLogConfig FetchTraceLogConfig(const Profiler *pProfiler);

    // Get configuration from the Global Area for the specified Profiler.
    ProfilerConfig FetchProfilerConfig(const Profiler *pProfiler);

    // This function is called when DLL is detached and we should perform
    // cleanup of the Global Area.
    void DllDetachShutdown() noexcept;

    //
    // Singleton is a noncopyable object.
    //

    ProfilerManager(const ProfilerManager&) = delete;

    ProfilerManager &operator=(const ProfilerManager&) = delete;
};

#endif // _PROFILER_MANAGER_H_
