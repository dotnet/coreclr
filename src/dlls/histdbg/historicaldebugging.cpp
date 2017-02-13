#include "historicaldebugging.h"
#include "log.h"
#include <pal.h>
#include <pthread.h>

#ifdef FEATURE_PAL

static const UINT32 UNINITIALIZED_PATTERN = 0xDADADADA;

HRESULT ProfStackSnapshotCallback(
                FunctionID funcId,
                UINT_PTR ip,
                COR_PRF_FRAME_INFO frameInfo,
                ULONG32 contextSize,
                BYTE context[],
                void *clientData) {
    
    // Is this a run of unmanaged frames?
    if (funcId == 0)
        return S_FALSE;

    BOOL unwindFurther = *((UINT32*)clientData) == UNINITIALIZED_PATTERN;
    *(_CONTEXT*)clientData = *(_CONTEXT*)context;

    return unwindFurther ? S_OK : S_FALSE;
}

void NotifySave(ThreadID threadID, void* info, void* eltInfo)
{
    ICorProfilerInfo3* iCorProfInfo = (ICorProfilerInfo3*)info;

    _CONTEXT context;
    memset(&context, UNINITIALIZED_PATTERN, sizeof(_CONTEXT));

    iCorProfInfo->DoStackSnapshot(threadID, ProfStackSnapshotCallback,
                        COR_PRF_SNAPSHOT_REGISTER_CONTEXT, &context, /* seed context */ NULL, 0);

    // if FP is less than SP, then something is definitely wrong, we don't want to create a checkpoint in such a state.
    if (context.R11 < context.Sp)
        return;

    // now we have a valid frame, record it
    CheckpointData data;
    data.registerContext = context;
    data.stackBufferSize = context.R11 - context.Sp;
    data.stackBuffer = (PBYTE) new BYTE[data.stackBufferSize];
    memcpy(data.stackBuffer, (void*)context.Sp, data.stackBufferSize);
    iCorProfInfo->GetThreadInfo(threadID, &data.threadId);
    LogCheckpointWrite(&data);
}

void NotifyPop(ThreadID threadID, void* info, void* eltInfo)
{
    // nothing to do, we don't delete any checkpoints
}

void NotifyInitialize() {
    LogInitialize();
}
#endif
