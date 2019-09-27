// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <stdio.h>
#include <windows.h>
#include "TPA.h"
#include "palclr.h"

bool TPA::HasFile(_In_z_ char* fileNameWithoutExtension)
{
    if (!m_tpa.CStr())
    {
        return false;
    }

    for (int iExtension = 0; iExtension < countExtensions; iExtension++)
    {
        char fileName[MAX_LONGPATH];
        strcpy_s(fileName, MAX_LONGPATH, "\\"); // So that we don't match other files that end with the current file name
        strcpy_s(fileName, MAX_LONGPATH, fileNameWithoutExtension);
        strcpy_s(fileName, MAX_LONGPATH, rgTPAExtensions[iExtension] + 1);
        strcpy_s(fileName, MAX_LONGPATH, ";"); // So that we don't match other files that begin with the current file name

        if (strstr(m_tpa.CStr(), fileName))
        {
            return true;
        }
    }
    return false;
}

static void RemoveExtensionAndNi(_In_z_ char* fileName)
{
    // Remove extension, if it exists
    char* extension = strchr(fileName, '.');
    if (extension != NULL)
    {
        extension[0] = '\0';

        // Check for .ni
        size_t len = strlen(fileName);
        if (len > 3 &&
            fileName[len - 1] == 'i' &&
            fileName[len - 2] == 'n' &&
            fileName[len - 3] == '.')
        {
            fileName[len - 3] = '\0';
        }
    }
}

void TPA::Compute(const char* baseDir)
{
    char assemblyPath[MAX_LONGPATH];

    for (int iExtension = 0; iExtension < countExtensions; iExtension++)
    {
        strcpy_s(assemblyPath, MAX_LONGPATH, baseDir);

        const size_t dirLength = strlen(baseDir);
        char* const fileNameBuffer = assemblyPath + dirLength;
        const size_t fileNameBufferSize = MAX_LONGPATH - dirLength;

        strcat_s(assemblyPath, rgTPAExtensions[iExtension]);
        WIN32_FIND_DATAA data;
        HANDLE findHandle = FindFirstFileA(assemblyPath, &data);

        if (findHandle != INVALID_HANDLE_VALUE) {
            do {
                if (!(data.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)) {
                    // It seems that CoreCLR doesn't always use the first instance of an assembly on the TPA list (ni's may be preferred
                    // over il, even if they appear later). So, only include the first instance of a simple assembly name to allow
                    // users the opportunity to override Framework assemblies by placing dlls in %CORE_LIBRARIES%

                    // ToLower for case-insensitive comparisons
                    char* fileNameChar = data.cFileName;
                    while (*fileNameChar)
                    {
                        *fileNameChar = (char)towlower(*fileNameChar);
                        fileNameChar++;
                    }

                    // Remove extension
                    char fileNameWithoutExtension[MAX_LONGPATH];
                    strcpy_s(fileNameWithoutExtension, MAX_LONGPATH, data.cFileName);

                    RemoveExtensionAndNi(fileNameWithoutExtension);

                    // Add to the list if not already on it
                    if (!HasFile(fileNameWithoutExtension))
                    {
                        const size_t fileLength = strlen(data.cFileName);
                        const size_t assemblyPathLength = dirLength + fileLength;
                        strncpy_s(fileNameBuffer, fileNameBufferSize, data.cFileName, fileLength);
                        m_tpa.Append(assemblyPath, assemblyPathLength);
                        m_tpa.Append(";", 1);
                    }
                }
            } while (0 != FindNextFileA(findHandle, &data));

            FindClose(findHandle);
        }
    }
}
