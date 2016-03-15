#ifndef LUCA_LOADERWRITER_H
#define LUCA_LOADERWRITER_H


#include "sstring.h"
#include "fstream.h"

class LoaderWriter {
public:
    LoaderWriter(int pid);
    ~LoaderWriter();
    void WriteRecord(SString& command, SString& path, SString& guid); 

private:
    int pid;
    fstream* stream;

    const char delimiter = ';';

    void OpenFile(SString& path);

}


#endif
