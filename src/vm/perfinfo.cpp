#include "common.h"
#include "perfinfo.h"
#include "pal.h"

PerfInfo::PerfInfo(int _pid)
{
    LIMITED_METHOD_CONTRACT;

    WCHAR tempPath[MAX_LONGPATH+1];
    if (!GetTempPathW(MAX_LONGPATH, tempPath))
    {
        return;
    }

    pid = _pid;

    SString path;
    path.Printf("%sperfinfo-%d.map", tempPath, pid);
    OpenFile(path);
}

void PerfInfo::LogNativeImage(object* obj)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_PREEMPTIVE;
        PRECONDITION(obj != NULL);
    } CONTRACTL_END;

    WriteLine("NILoad", "value");

}

void WriteLine(SString& type, SString& value)
{

    STANDARD_VM_CONTRACT;

    if (stream == NULL)
    {
        return;
    }

    SString line;
    line.Printf("%S%c%S", type, delimiter, value);

    EX_TRY
    {
        StackScratchBuffer scratch;
        const char* strLine = line.GetANSI(scratch);
        ULONG inCount = line.GetCount();
        ULONG outCount;

        stream->Write(strLine, inCount, &outCount);

        if (inCount != outCount)
        {
            // error encountered
        }
    }
    EX_CATCH{} EX_END_CATCH(SwallowAllExceptions);
}

void PerfInfo::OpenFile(SString& path)
{
    STANDARD_VM_CONTRACT;

    stream = new (nothrow) CFileStream();

    if (stream != NULL)
    {
        HRESULT hr = stream->OpenForWrite(path.GetUnicode());
        if (FAILED(hr))
        {
            delete stream;
            stream = NULL;
        }
    }
}

PerfInfo::~PerfInfo()
{
    LIMITED_METHOD_CONTRACT;

    delete stream;
    stream = NULL;
}













