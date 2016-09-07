#ifndef _THREAD_STORAGE_H_
#define _THREAD_STORAGE_H_

#include "mappedstorage.h"
#include "threadinfo.h"

class ThreadStorage : public MappedStorage<ThreadID, ThreadInfo>
{};

#endif // _THREAD_STORAGE_H_
