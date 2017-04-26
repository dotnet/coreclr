// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


#ifndef __EVENTPIPE_FILE_H__
#define __EVENTPIPE_FILE_H__

#include "eventpipe.h"
#include "eventpipeeventinstance.h"
#include "fastserializer.h"

class EventPipeFile
{
    public:
        EventPipeFile(SString &outputFilePath);
        ~EventPipeFile();

        // Write an event to the file.
        void WriteEvent(EventPipeEventInstance &instance);

    private:
        // The object responsible for serialization.
        FastSerializer *m_pSerializer;

        // The timestamp when the file was opened.  Used for calculating file-relative timestamps.
        LARGE_INTEGER m_fileOpenTimeStamp;
};

#endif // __EVENTPIPE_FILE_H__
