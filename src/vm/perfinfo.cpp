#include "common.h"
#include "perfinfo.h"
#include "pal.h"

PerfInfo::PerfInfo(int _pid) {
    LIMITED_METHOD_CONTRACT;

    pid = _pid;

    SString path;
    path.Printf("loader-%d.map", pid);
    OpenFile(path);
}

void PerfInfo::WriteRecord(SString& command, SString& path, SString& guid) {

    CONTRACTL{
        THROWS;
        GC_NOTRIGGER;
        MODE_PREEMPTIVE;
    } CONTRACTL_END;

    SString line;
    line.Printf("%S;%S;%S", command, path, guid);

    EX_TRY
    {
        StackScratchBuffer scratch;
        const char* strLine = line.GetANSI(scratch);
        ULONG inCount = line.GetCount();
        ULONG outCount;

        stream->Write(strLine, inCount, &outCount);

        if (inCount != outCount) {
            // error encountered
        }
    }
    EX_CATCH{} EX_END_CATCH(SwallowAllExceptions);
}


void PerfInfo::OpenFile(SString& path) {
    STANDARD_VM_CONTRACT;

    stream = new (nothrow) CFileStream();

    if (stream != NULL) {
        HRESULT hr = stream->OpenForWrite(path.GetUnicode());
        if (FAILED(hr)) {
            delete stream;
            stream = NULL;
        }
    }
}

PerfInfo::~PerfInfo() {
    LIMITED_METHOD_CONTRACT;

    delete stream;
    stream = NULL;
}













