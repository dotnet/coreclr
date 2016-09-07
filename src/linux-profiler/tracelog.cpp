#include <system_error>
#include <utility>
#include <mutex>

#include <errno.h>

#include <pal.h>

#include "tracelog.h"

class TraceLog final : public ITraceLog
{
public:
    TraceLog(StdOutStream_t)
        : m_pStream(PAL_stdout)
        , m_bIsOwner(false)
    {}

    TraceLog(StdErrStream_t)
        : m_pStream(PAL_stderr)
        , m_bIsOwner(false)
    {}

    TraceLog(FileStream_t, const std::string &filename)
    {
        m_pStream = PAL_fopen(filename.c_str(), "w");
        if (m_pStream == nullptr)
        {
            throw std::system_error(errno, std::system_category(),
                "can't create TraceLog object");
        }
        m_bIsOwner = true;
    }

    virtual ~TraceLog()
    {
        if (m_bIsOwner)
            PAL_fclose(m_pStream);
    }

    virtual void DumpAppDomainCreationFinished(
        AppDomainID appDomainId,
        LPCWCH      appDomainName,
        ProcessID   processId,
        HRESULT     hrStatus) override
    {
        if (appDomainName == nullptr)
            appDomainName = W("UNKNOWN");

        std::lock_guard<std::mutex> streamLock(m_mStream);
        PAL_fprintf(
            m_pStream, "apd crf 0x%p 0x%p 0x%08x \"%S\"\n",
            appDomainId, processId, hrStatus, appDomainName
        );
    }

    virtual void DumpAssemblyLoadFinished(
        AssemblyID  assemblyId,
        LPCWCH      assemblyName,
        AppDomainID appDomainId,
        ModuleID    moduleId,
        HRESULT     hrStatus) override
    {
        if (assemblyName == nullptr)
            assemblyName = W("UNKNOWN");

        std::lock_guard<std::mutex> streamLock(m_mStream);
        PAL_fprintf(
            m_pStream, "asm ldf 0x%p 0x%p 0x%p 0x%08x \"%S\"\n",
            assemblyId, appDomainId, moduleId, hrStatus, assemblyName
        );
    }

    virtual void DumpModuleLoadFinished(
        ModuleID    moduleId,
        LPCBYTE     baseLoadAddress,
        LPCWCH      moduleName,
        AssemblyID  assemblyId,
        HRESULT     hrStatus) override
    {
        if (moduleName == nullptr)
            moduleName = W("UNKNOWN");

        std::lock_guard<std::mutex> streamLock(m_mStream);
        PAL_fprintf(
            m_pStream, "mod ldf 0x%p 0x%p 0x%p 0x%08x \"%S\"\n",
            moduleId, baseLoadAddress, assemblyId, hrStatus, moduleName
        );
    }

    virtual void DumpModuleAttachedToAssembly(
        ModuleID    moduleId,
        AssemblyID  assemblyId) override
    {
        std::lock_guard<std::mutex> streamLock(m_mStream);
        PAL_fprintf(
            m_pStream, "mod ata 0x%p 0x%p\n", moduleId, assemblyId
        );
    }

    virtual void DumpClassLoadFinished(
        ClassID     classId,
        ModuleID    moduleId,
        mdTypeDef   typeDefToken,
        HRESULT     hrStatus) override
    {
        std::lock_guard<std::mutex> streamLock(m_mStream);
        PAL_fprintf(
            m_pStream, "cls ldf 0x%p 0x%p 0x%08x 0x%08x\n",
            classId, moduleId, typeDefToken, hrStatus
        );
    }

    virtual void DumpJITCompilationFinished(
        FunctionID  functionId,
        ClassID     classId,
        ModuleID    moduleId,
        mdToken     token,
        HRESULT     hrStatus,
        const FunctionInfo &info) override
    {
        std::lock_guard<std::mutex> streamLock(m_mStream);
        PAL_fprintf(
            m_pStream, "fun cmf 0x%p 0x%08x 0x%p 0x%p 0x%08x 0x%08x",
            functionId, info.internalId.id, classId, moduleId, token, hrStatus
        );
        DumpFunctionInfo(info);
        PAL_fprintf(m_pStream, "\n");
    }

    virtual void DumpJITCachedFunctionSearchFinished(
        FunctionID  functionId,
        ClassID     classId,
        ModuleID    moduleId,
        mdToken     token,
        const FunctionInfo &info) override
    {
        std::lock_guard<std::mutex> streamLock(m_mStream);
        PAL_fprintf(
            m_pStream, "fun csf 0x%p 0x%08x 0x%p 0x%p 0x%08x",
            functionId, info.internalId.id, classId, moduleId, token
        );
        DumpFunctionInfo(info);
        PAL_fprintf(m_pStream, "\n");
    }

    virtual void DumpJITFunctionName(
        const FunctionInfo &info) override
    {
        std::lock_guard<std::mutex> streamLock(m_mStream);
        PAL_fprintf(m_pStream, "fun nam 0x%08x \"", info.internalId.id);
        if (!info.className.empty())
        {
            PAL_fprintf(m_pStream, "%S::", info.className.c_str());
        }
        PAL_fprintf(m_pStream, "%S\"\n", info.name.c_str());
    }

    virtual void DumpThreadCreated(
        ThreadID    threadId,
        InternalID  threadIid) override
    {
        std::lock_guard<std::mutex> streamLock(m_mStream);
        PAL_fprintf(m_pStream, "thr crt 0x%p 0x%08x\n", threadId, threadIid.id);
    }

    virtual void DumpThreadDestroyed(
        InternalID  threadIid) override
    {
        std::lock_guard<std::mutex> streamLock(m_mStream);
        PAL_fprintf(m_pStream, "thr dst 0x%08x\n", threadIid.id);
    }

    virtual void DumpThreadAssignedToOSThread(
        InternalID  managedThreadIid,
        DWORD       osThreadId) override
    {
        std::lock_guard<std::mutex> streamLock(m_mStream);
        PAL_fprintf(
            m_pStream, "thr aos 0x%08x %d\n", managedThreadIid.id, osThreadId
        );
    }

    virtual void DumpStackTraceSample(
        InternalID           threadIid,
        const SampleInfo     &info,
        const StackTraceDiff &diff) override
    {
        std::lock_guard<std::mutex> streamLock(m_mStream);
        PAL_fprintf(m_pStream, "str sam 0x%08x %d", threadIid.id, info.ticks);
        PAL_fprintf(m_pStream, info.count > 0 ? " %lu" : " ?", info.count);
        DumpStackTraceDiff(diff);
        PAL_fprintf(m_pStream, "\n");
    }

private:
    PAL_FILE  *m_pStream;
    std::mutex m_mStream;
    bool       m_bIsOwner;

    void DumpFunctionInfo(const FunctionInfo &info)
    {
        for (const auto &ci : info.codeInfo)
        {
            PAL_fprintf(m_pStream, " 0x%p:0x%x",
                ci.startAddress, ci.size);
        }

        for (const auto &m : info.ILToNativeMapping)
        {
            PAL_fprintf(m_pStream, " 0x%x:0x%x:0x%x",
                m.ilOffset, m.nativeStartOffset, m.nativeEndOffset);
        }
    }

    void DumpStackTraceDiff(const StackTraceDiff &diff)
    {
        PAL_fprintf(m_pStream, diff.IP() != 0 ? " %d:%d:%p" : " %d:%d",
            diff.MatchPrefixSize(), diff.StackSize(), diff.IP());
        for (const auto &frame : diff)
        {
            PAL_fprintf(m_pStream, frame.ip != 0 ? " 0x%x:%p" : " 0x%x",
                frame.functionIid.id, frame.ip);
        }
    }
};

// static
ITraceLog *ITraceLog::Create(StdOutStream_t StdOutStream)
{
    return new TraceLog(StdOutStream);
}

// static
ITraceLog *ITraceLog::Create(StdErrStream_t StdErrStream)
{
    return new TraceLog(StdErrStream);
}

// static
ITraceLog *ITraceLog::Create(
    FileStream_t FileStream, const std::string &filename)
{
    return new TraceLog(FileStream, filename);
}
