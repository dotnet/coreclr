//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//
// ===========================================================================
// File: perfmap.cpp
//

#include "common.h"

#ifdef FEATURE_PERFMAP
#include "perfmap.h"
#ifdef PLATFORM_UNIX
#include "pal.h"
#endif

PerfMap * PerfMap::s_Current = NULL;

// Initialize the map for the process - called from EEStartupHelper.
void PerfMap::Initialize()
{
    LIMITED_METHOD_CONTRACT;

    // Only enable the map if requested.
    if (CLRConfig::GetConfigValue(CLRConfig::EXTERNAL_PerfMapEnabled))
    {
        // Get the current process id.
        int currentPid = GetCurrentProcessId();

        // Create the map.
        s_Current = new PerfMap(currentPid);
    }
}

void PerfMap::LogPreCompiledMethod(MethodDesc * pMethod, PCODE pCode)
{
    STANDARD_VM_CONTRACT;

    if (s_Current != NULL)
    {
        // Check to see if we've already logged this one.
        {
            // Take a lock on the table.
            CrstHolder _crst(&s_Current->m_ZapPerfMapMethodsCrst);

            // Check the hash table to see if we've already written this method.
            MethodDesc * pWrittenMD = s_Current->m_ZapPerfMapMethods.Lookup(pMethod);
            if (pWrittenMD != NULL)
            {
                // We've written the method already so just return.
                return;
            }
            else
            {
                // We've not written this method yet.
                // Mark it written under lock and then write it outside of the lock.
                s_Current->m_ZapPerfMapMethods.Add(pMethod);
            }
        }

        // Get information about the NGEN'd method code.
        EECodeInfo codeInfo(pCode);
        _ASSERTE(codeInfo.IsValid());

        IJitManager::MethodRegionInfo methodRegionInfo;
        codeInfo.GetMethodRegionInfo(&methodRegionInfo);

        // NGEN can split code between hot and cold sections which are separate in memory.
        // Emit an entry for each section if it is used.
        if (methodRegionInfo.hotSize > 0)
        {
            s_Current->Log(pMethod, (PCODE)methodRegionInfo.hotStartAddress, methodRegionInfo.hotSize);
        }

        if (methodRegionInfo.coldSize > 0)
        {
            s_Current->Log(pMethod, (PCODE)methodRegionInfo.coldStartAddress, methodRegionInfo.coldSize);
        }
    }
}

// Log a method to the map.
void PerfMap::LogJITCompiledMethod(MethodDesc * pMethod, PCODE pCode, size_t codeSize)
{
    LIMITED_METHOD_CONTRACT;

    if (s_Current != NULL)
    {
        s_Current->Log(pMethod, pCode, codeSize);
    }
}

// Destroy the map for the process - called from EEShutdownHelper.
void PerfMap::Destroy()
{
    if (s_Current != NULL)
    {
        delete s_Current;
        s_Current = NULL;
    }
}

// Construct a new map for the process.
PerfMap::PerfMap(int pid)
{
    LIMITED_METHOD_CONTRACT;

    // Initialize with no failures.
    m_ErrorEncountered = false;

    // Initialize the pre-compiled method list lock.
    if (!m_ZapPerfMapMethodsCrst.InitNoThrow(
        CrstEtwTypeLogHash,
        CRST_UNSAFE_ANYMODE))
    {
        return;
    }

    // Build the path to the map file on disk.
    WCHAR tempPath[MAX_LONGPATH+1];
    if(!GetTempPathW(MAX_LONGPATH, tempPath))
    {
        return;
    }
    
    SString path;
    path.Printf("%Sperf-%d.map", &tempPath, pid);

    // Open the file stream.
    m_FileStream = new (nothrow) CFileStream();
    if(m_FileStream != NULL)
    {
        HRESULT hr = m_FileStream->OpenForWrite(path.GetUnicode());
        if(FAILED(hr))
        {
            delete m_FileStream;
            m_FileStream = NULL;
        }
    }
}

// Clean-up resources.
PerfMap::~PerfMap()
{
    LIMITED_METHOD_CONTRACT;

    delete m_FileStream;
    m_FileStream = NULL;
}

// Log a method to the map.
void PerfMap::Log(MethodDesc * pMethod, PCODE pCode, size_t codeSize)
{
    CONTRACTL{
        THROWS;
        GC_NOTRIGGER;
        MODE_PREEMPTIVE;
        PRECONDITION(pMethod != NULL);
        PRECONDITION(pCode != NULL);
        PRECONDITION(codeSize > 0);
    } CONTRACTL_END;

    if (m_FileStream == NULL || m_ErrorEncountered)
    {
        // A failure occurred, do not log.
        return;
    }

    // Logging failures should not cause any exceptions to flow upstream.
    EX_TRY
    {
        // Get the full method signature.
        SString fullMethodSignature;
        pMethod->GetFullMethodInfo(fullMethodSignature);

        // Build the map file line.
        StackScratchBuffer scratch;
        SString line;
        line.Printf("%p %x %s\n", pCode, codeSize, fullMethodSignature.GetANSI(scratch));

        // Write the line.
        // The PAL already takes a lock when writing, so we don't need to do so here.
        const char * strLine = line.GetANSI(scratch);
        ULONG inCount = line.GetCount();
        ULONG outCount;
        m_FileStream->Write(strLine, inCount, &outCount);

        if (inCount != outCount)
        {
            // This will cause us to stop writing to the file.
            // The file will still remain open until shutdown so that we don't have to take a lock at this levelwhen we touch the file stream.
            m_ErrorEncountered = true;
        }
    }
    EX_CATCH{} EX_END_CATCH(SwallowAllExceptions);
}

#endif // FEATURE_PERFMAP
