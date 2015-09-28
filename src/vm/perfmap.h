//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//
// ===========================================================================
// File: perfmap.h
//
#ifndef PERFPID_H
#define PERFPID_H

#include "sstring.h"
#include "fstream.h"
#include "shash.h"

class PerfMap
{
private:

    class ZapPerfMapMethodTraits : public DefaultSHashTraits<MethodDesc *>
    {
    public:
        typedef MethodDesc * key_t;

        static key_t GetKey(element_t e)
        {
            LIMITED_METHOD_CONTRACT;
            
            return e;
        }
        static BOOL Equals(key_t k1, key_t k2)
        {
            LIMITED_METHOD_CONTRACT;
            
            return k1 == k2;
        }
        static count_t Hash(key_t k)
        {
            LIMITED_METHOD_CONTRACT;

            return (count_t)(size_t)k;
        }
    };
    typedef SHash<ZapPerfMapMethodTraits> ZapPerfMapMethods;

    // The one and only PerfMap for the process.
    static PerfMap * s_Current;

    // Hash table containing the MethodDesc addresses for those pre-compiled
    // methods that have already been written to the map.
    ZapPerfMapMethods m_ZapPerfMapMethods;

    // Lock for the entry table.
    CrstExplicitInit m_ZapPerfMapMethodsCrst;

    // The file stream to write the map to.
    CFileStream * m_FileStream;

    // Set to true if an error is encountered when writing to the file.
    bool m_ErrorEncountered;

    // Construct a new map.
    PerfMap(int pid);

    // Clean-up resources.
    ~PerfMap();

    // Does the actual work to log to the map.
    void Log(MethodDesc * pMethod, PCODE pCode, size_t codeSize);

    // Does the actual work to close and flush the map.
    void Close();

public:
    // Initialize the map for the current process.
    static void Initialize();

    // Log a pre-compiled method to the map.
    static void LogPreCompiledMethod(MethodDesc * pMethod, PCODE pCode);

    // Log a JIT compiled method to the map.
    static void LogJITCompiledMethod(MethodDesc * pMethod, PCODE pCode, size_t codeSize);
   
    // Close the map and flush any remaining data.
    static void Destroy();
};

#endif // PERFPID_H
