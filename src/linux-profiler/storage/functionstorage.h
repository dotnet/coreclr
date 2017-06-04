#ifndef _FUNCTION_STORAGE_H_
#define _FUNCTION_STORAGE_H_

#include "mappedstorage.h"
#include "functioninfo.h"

class FunctionStorage : public MappedStorage<FunctionID, FunctionInfo>
{
private:
    using Base = MappedStorage<FunctionID, FunctionInfo>;

public:
    FunctionStorage(ExecutionTrace *pExecutionTrace)
        : Base()
        , m_pExecutionTrace(pExecutionTrace)
    {}

    std::pair<FunctionInfo&, bool>
    Place(FunctionID id, const FunctionInfo::String &name = W(""))
    {
        auto res = this->Base::Place(id);
        res.first.executionTrace = m_pExecutionTrace;
        res.first.name = name;
        return std::make_pair(std::ref(res.first), res.second);
    }

    // Add new function info without mapping to FunctionID. It is useful for
    // internal pseudo-functions.
    FunctionInfo &Add(const FunctionInfo::String &name = W(""))
    {
        FunctionInfo &res = this->Base::Add();
        res.executionTrace = m_pExecutionTrace;
        res.name = name;
        return std::ref(res);
    }

protected:
    ExecutionTrace *m_pExecutionTrace;
};

#endif // _FUNCTION_STORAGE_H_
