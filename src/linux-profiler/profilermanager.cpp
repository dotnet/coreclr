#include <assert.h>

#include "profiler.h"
#include "environmentconfigprovider.h"
#include "profilermanager.h"

// NOTE: currently only one instance of the Profiler can be registered in the
// Profiler Manager. We use "hidden" global reference since this limitation
// shouldn't be demonstrated in the class private section. We can use a static
// global variable because the Profiler Manager instance is a singleton.
static Profiler *g_pProfilerObject = nullptr;

// static
ProfilerManager &ProfilerManager::Instance() noexcept
{
    static ProfilerManager s_ProfilerManagerInstance;
    return s_ProfilerManagerInstance;
}

ProfilerManager::ProfilerManager() noexcept
{
}

ProfilerManager::~ProfilerManager()
{
    // We should ensure that the DllDetachShutdown() method was called before
    // singleton destruction.
    assert(g_pProfilerObject == nullptr);
}

template <typename T>
T ProfilerManager::FetchConfig(const Profiler *pProfiler)
{
    // Ensure method is called for the global profiler instance;
    assert(g_pProfilerObject == pProfiler);

    T config;

    // Currently only environment is used as source of the configuration.
    EnvironmentConfigProvider().FetchConfig(config);

    config.Validate();

    auto warnings = config.Verify();
    if (!warnings.empty())
    {
        auto logLine = pProfiler->LOG().Warn();
        logLine << "Some errors detected in " << config.Name() << ":";
        for (const auto &warning : warnings)
        {
            logLine << "\n\t\t" << warning;
        }
    }

    return config;
}

bool ProfilerManager::IsProfilerRegistered(
    const Profiler *pProfiler) const noexcept
{
    return pProfiler != nullptr && g_pProfilerObject == pProfiler;
}

void ProfilerManager::RegisterProfiler(const Profiler *pProfiler)
{
    // NOTE: potential race condition should be avoided outside of this class.
    if (g_pProfilerObject == nullptr)
    {
        g_pProfilerObject = const_cast<Profiler*>(pProfiler);
    }
    else
    {
        // Ensure method is called for the global profiler instance;
        assert(g_pProfilerObject == pProfiler);
    }
}

void ProfilerManager::UnregisterProfiler(const Profiler *pProfiler)
{
    if (g_pProfilerObject == pProfiler)
    {
        g_pProfilerObject = nullptr;
    }
}

LoggerConfig ProfilerManager::FetchLoggerConfig(const Profiler *pProfiler)
{
    return FetchConfig<LoggerConfig>(pProfiler);
}

TraceLogConfig ProfilerManager::FetchTraceLogConfig(const Profiler *pProfiler)
{
    return FetchConfig<TraceLogConfig>(pProfiler);
}

ProfilerConfig ProfilerManager::FetchProfilerConfig(const Profiler *pProfiler)
{
    return FetchConfig<ProfilerConfig>(pProfiler);
}

void ProfilerManager::DllDetachShutdown() noexcept
{
    //
    // Since this function is called when DLL ends up its lifetime, we don't
    // worry about profiler's reference counter.
    //
    if (IsProfilerRegistered(g_pProfilerObject))
    {
        Profiler::RemoveObject(g_pProfilerObject);
    }
}
