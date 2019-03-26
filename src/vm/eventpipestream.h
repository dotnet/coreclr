// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __EVENTPIPE_STREAM_H__
#define __EVENTPIPE_STREAM_H__

#ifdef FEATURE_PERFTRACING

#include "eventpipe.h"
#include "eventpipeblock.h"
#include "eventpipeeventinstance.h"
#include "fastserializableobject.h"

class FastSerializer;

class EventPipeStream final : public FastSerializableObject
{
public:
    EventPipeStream(IpcStream *pStream);
    ~EventPipeStream();

    bool WriteEvent(EventPipeEventInstance &instance) override;

    const char *GetTypeName() override
    {
        LIMITED_METHOD_CONTRACT;
        return "Trace";  // TODO: What is this for?
    }

    void FastSerialize(FastSerializer *pSerializer) override
    {
        CONTRACTL
        {
            NOTHROW;
            GC_NOTRIGGER;
            MODE_PREEMPTIVE;
            PRECONDITION(pSerializer != NULL);
        }
        CONTRACTL_END;

        pSerializer->WriteBuffer((BYTE *)&m_fileOpenSystemTime, sizeof(m_fileOpenSystemTime));
        pSerializer->WriteBuffer((BYTE *)&m_fileOpenTimeStamp, sizeof(m_fileOpenTimeStamp));
        pSerializer->WriteBuffer((BYTE *)&m_timeStampFrequency, sizeof(m_timeStampFrequency));
        pSerializer->WriteBuffer((BYTE *)&m_pointerSize, sizeof(m_pointerSize));
        pSerializer->WriteBuffer((BYTE *)&m_currentProcessId, sizeof(m_currentProcessId));
        pSerializer->WriteBuffer((BYTE *)&m_numberOfProcessors, sizeof(m_numberOfProcessors));
        pSerializer->WriteBuffer((BYTE *)&m_samplingRateInNs, sizeof(m_samplingRateInNs));
    }

private:
    void WriteEnd();

    unsigned int GenerateMetadataId();

    unsigned int GetMetadataId(EventPipeEvent &event);

    void SaveMetadataId(EventPipeEvent &event, unsigned int metadataId);

    void WriteToBlock(EventPipeEventInstance &instance, unsigned int metadataId);

    // The object responsible for serialization.
    FastSerializer *m_pSerializer;
    EventPipeBlock *m_pBlock;

    SYSTEMTIME m_fileOpenSystemTime;
    LARGE_INTEGER m_fileOpenTimeStamp;
    LARGE_INTEGER m_timeStampFrequency;
    unsigned int m_pointerSize;
    unsigned int m_currentProcessId;
    unsigned int m_numberOfProcessors;
    unsigned int m_samplingRateInNs;

    // The serialization which is responsible for making sure only a single event
    // or block of events gets written to the file at once.
    SpinLock m_serializationLock;

    // Hashtable of metadata labels.
    MapSHashWithRemove<EventPipeEvent *, unsigned int> *m_pMetadataIds;

    Volatile<LONG> m_metadataIdCounter;
};

#endif // FEATURE_PERFTRACING

#endif // __EVENTPIPE_STREAM_H__
