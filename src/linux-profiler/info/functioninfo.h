#ifndef _FUNCTION_INFO_
#define _FUNCTION_INFO_

#include <vector>

#include <cor.h>
#include <corhdr.h>
#include <corprof.h>

#include "mappedinfo.h"

class Profiler;

class ExecutionTrace;

struct FunctionInfo : public MappedInfo<FunctionID>
{
    typedef std::basic_string<WCHAR> String;

    ExecutionTrace *executionTrace;

    String className;
    String name;
    String signature;

    COR_PRF_CODE_INFO              firstCodeInfo;
    std::vector<COR_PRF_CODE_INFO> codeInfo;

    std::vector<COR_DEBUG_IL_TO_NATIVE_MAP> ILToNativeMapping;
    // TODO: other useful stuff.

private:
    HRESULT GetClassName(
        const Profiler &profiler,
        IMetaDataImport *pMDImport,
        mdTypeDef classToken,
        String &className) noexcept;

public:
    HRESULT InitializeCodeInfo(const Profiler &profiler) noexcept;

    HRESULT InitializeILToNativeMapping(const Profiler &profiler) noexcept;

    HRESULT InitializeNameAndSignature(const Profiler &profiler) noexcept;
};

#endif // _FUNCTION_INFO_
