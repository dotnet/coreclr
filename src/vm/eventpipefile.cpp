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

    m_pSerializer = new FastSerializer(outputFilePath);
    QueryPerformanceCounter(&m_fileOpenTimeStamp);
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

    // TODO: Write it to the event buffer.
}
