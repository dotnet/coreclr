#ifndef __HISTORICALDEBUGGING_H__
#define __HISTORICALDEBUGGING_H__

#include <pal.h>
#include <ntimage.h>
#include <corhdr.h>
#include <cor.h>
#include <corprof.h>

void NotifySave(ThreadID threadID, void* info, void* eltInfo);
void NotifyPop(ThreadID threadID, void* info, void* eltInfo);
void NotifyInitialize();

#endif
