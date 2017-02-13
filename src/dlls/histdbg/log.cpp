#include <pal.h>
#include <ntimage.h>
#include <corhdr.h>
#include <cor.h>
#include <corprof.h>
#include "log.h"
#include <pal_assert.h>
#ifdef FEATURE_PAL

void LogCheckpointWrite(CheckpointData *checkpoint)
{
	// we must open and close the file every time since a reverse command can be issued between calls
	// and that might modify the file as well
	// we don't need to worry about thread safety since we have a separate file for every thread.
	char fileNameBuffer[256];
	sprintf(fileNameBuffer, FMT_LOG_FILE_PATH, checkpoint->threadId);
	FILE* logFile = fopen(fileNameBuffer, "ab");
	FrameHeader header;
	header.id = checkpoint->threadId;
	header.context = checkpoint->registerContext;
	header.frameSize = checkpoint->stackBufferSize;
	size_t elementsWritten = fwrite(checkpoint->stackBuffer, header.frameSize, 1, logFile);
	if (elementsWritten == 0)
		return;
	elementsWritten = fwrite(&header, sizeof(FrameHeader), 1, logFile);
	assert(elementsWritten == 1 && "Checkpoint write failed");
	fclose(logFile);
}

void LogInitialize()
{
	// delete all files in /tmp that match our format
	WIN32_FIND_DATAW fd;
	WCHAR fileNameBuffer[256];
	HANDLE hFind = FindFirstFile((char16_t*)L"/tmp/912E73AF-F51D-4E80-894D-F4E9E6DD7C2E*.log", &fd);
	if (hFind != INVALID_HANDLE_VALUE)
	{
		wsprintfW(fileNameBuffer, (const char16_t*)L"/tmp/%s", fd.cFileName);
	    do
	    {
	        DeleteFile(fileNameBuffer);
	    } while (FindNextFileW(hFind, &fd));
	    FindClose(hFind);
	}
}

void LogFinalize()
{
}

#endif