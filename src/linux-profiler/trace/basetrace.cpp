#include "profiler.h"
#include "profilerinfo.h"
#include "basetrace.h"

BaseTrace::BaseTrace(Profiler &profiler, ProfilerInfo &info)
    : m_disabled(true)
    , m_profiler(profiler)
    , m_info(info)
{
}

BaseTrace::~BaseTrace()
{
}

Log &BaseTrace::LOG() const noexcept
{
    return m_profiler.LOG();
}

ITraceLog &BaseTrace::TRACE() const noexcept
{
    return m_profiler.TRACE();
}
