// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


#ifndef __EVENTPIPE_FILE_H__
#define __EVENTPIPE_FILE_H__

#include "eventpipe.h"
#include "eventpipeeventinstance.h"
#include "fastserializableobject.h"
#include "fastserializer.h"

class EventPipeFile : public FastSerializableObject
{
    public:
        EventPipeFile(SString &outputFilePath);
        ~EventPipeFile();

        // Write an event to the file.
        void WriteEvent(EventPipeEventInstance &instance);

        // Serialize this object.
        // Not supported - this is the entry object for the trace,
        // which means that the contents hasn't yet been created.
        void FastSerialize(FastSerializer *pSerializer)
        {
            LIMITED_METHOD_CONTRACT;
            _ASSERTE(!"This function should never be called!");
        }

        // Get the type name of this object.
        const char* GetTypeName()
        {
            LIMITED_METHOD_CONTRACT;
            return "Microsoft.DotNet.Runtime.EventPipeFile";
        }

    private:
        // The object responsible for serialization.
        FastSerializer *m_pSerializer;

        // The timestamp when the file was opened.  Used for calculating file-relative timestamps.
        LARGE_INTEGER m_fileOpenTimeStamp;

        // The forward reference index that marks the beginning of the event stream.
        unsigned int m_beginEventsForwardReferenceIndex;
};

#endif // __EVENTPIPE_FILE_H__
