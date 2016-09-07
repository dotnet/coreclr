#ifndef _TRACE_LOG_H_
#define _TRACE_LOG_H_

#include <windows.h>

#include <cor.h>
#include <corhdr.h>
#include <corprof.h>

#include "functioninfo.h"
#include "stackchannel.h"

class ITraceLog
{
protected:
    class StdOutStream_t {};

    class StdErrStream_t {};

    class FileStream_t {};

public:
    static StdOutStream_t StdOutStream;

    static StdErrStream_t StdErrStream;

    static FileStream_t   FileStream;

    ITraceLog() = default;

    ITraceLog(const ITraceLog&) = delete;

    ITraceLog &operator=(const ITraceLog&) = delete;

    virtual ~ITraceLog() = default;

    static ITraceLog *Create(StdOutStream_t);

    static ITraceLog *Create(StdErrStream_t);

    static ITraceLog *Create(FileStream_t, const std::string &filename);

    // TODO: different methods to dump information.

    virtual void DumpAppDomainCreationFinished(
        AppDomainID appDomainId,
        LPCWCH      appDomainName,
        ProcessID   processId,
        HRESULT     hrStatus) = 0;

    virtual void DumpAssemblyLoadFinished(
        AssemblyID  assemblyId,
        LPCWCH      assemblyName,
        AppDomainID appDomainId,
        ModuleID    moduleId,
        HRESULT     hrStatus) = 0;

    virtual void DumpModuleLoadFinished(
        ModuleID    moduleId,
        LPCBYTE     baseLoadAddress,
        LPCWCH      moduleName,
        AssemblyID  assemblyId,
        HRESULT     hrStatus) = 0;

    virtual void DumpModuleAttachedToAssembly(
        ModuleID    moduleId,
        AssemblyID  assemblyId) = 0;

    virtual void DumpClassLoadFinished(
        ClassID     classId,
        ModuleID    moduleId,
        mdTypeDef   typeDefToken,
        HRESULT     hrStatus) = 0;

    virtual void DumpJITCompilationFinished(
        FunctionID  functionId,
        ClassID     classId,
        ModuleID    moduleId,
        mdToken     token,
        HRESULT     hrStatus,
        const FunctionInfo &info) = 0;

    virtual void DumpJITCachedFunctionSearchFinished(
        FunctionID  functionId,
        ClassID     classId,
        ModuleID    moduleId,
        mdToken     token,
        const FunctionInfo &info) = 0;

    virtual void DumpJITFunctionName(
        const FunctionInfo &info) = 0;

    virtual void DumpThreadCreated(
        ThreadID    threadId,
        InternalID  threadIid) = 0;

    virtual void DumpThreadDestroyed(
        InternalID  threadIid) = 0;

    virtual void DumpThreadAssignedToOSThread(
        InternalID  managedThreadIid,
        DWORD       osThreadId) = 0;

    virtual void DumpStackTraceSample(
        InternalID           threadIid,
        const SampleInfo     &info,
        const StackTraceDiff &diff) = 0;
};

#endif // _TRACE_LOG_H_
