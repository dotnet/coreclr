// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "eventpipefile.h"

EventPipeFile::EventPipeFile(SString &outputFilePath)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    m_pSerializer = new FastSerializer(outputFilePath, *this);
    QueryPerformanceCounter(&m_fileOpenTimeStamp);

    // Write a forward reference to the beginning of the event stream.
    // This also allows readers to know where the event stream ends and skip it if needed.
    m_beginEventsForwardReferenceIndex = m_pSerializer->AllocateForwardReference();
    m_pSerializer->WriteForwardReference(m_beginEventsForwardReferenceIndex);
}

EventPipeFile::~EventPipeFile()
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    // Mark the end of the event stream.
    StreamLabel currentLabel = m_pSerializer->GetStreamLabel();

    // Define the event start forward reference.
    m_pSerializer->DefineForwardReference(m_beginEventsForwardReferenceIndex, currentLabel);

    // Close the serializer.
    if(m_pSerializer != NULL)
    {
        delete(m_pSerializer);
        m_pSerializer = NULL;
    }
}

void EventPipeFile::WriteEvent(EventPipeEventInstance &instance)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    // Write the event to the stream.
    instance.FastSerialize(m_pSerializer);
}
