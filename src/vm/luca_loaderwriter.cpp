#include "common.h"
#include "luca_loaderwriter.h"
#include "pal.h"

LoaderWriter::LoaderWriter(int pid) {
    LIMITED_METHOD_CONTRACT;

    this->pid = pid;

    SString path;
    path.Printf("loader-%d.map", pid);
    this->OpenFile(path);
}

void LoaderWriter::WriteRecord(SString& command, SString& path, SString& guid) {

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

        this->stream->Write(strLine, inCount, &outCount);

        if (inCount != outCount) {
            // error encountered
        }
    }
    EX_CATCH{} EX_END_CATCH(SwallowAllExceptions);
}


void LoaderWriter::OpenFile(SString& path) {
    STANDARD_VM_CONTRACT;

    this->stream = new (nothrow) CFileStream();

    if (this->stream != NULL) {
        HRESULT hr = this->stream->OpenForWrite(path.GetUnicode());
        if (FAILED(hr)) {
            delete this->stream;
            this->stream = NULL;
        }
    }
}

LoaderWriter::~LoaderWriter() {
    LIMITED_METHOD_CONTRACT;

    delete this->stream;
    stream = NULL;
}













