#ifndef PERFINFO_H
#define PERFINFO_H


#include "sstring.h"
#include "fstream.h"

class PerfInfo {
public:
    PerfInfo(int pid);
    ~PerfInfo();
    void LogNativeImage(object*); 

private:
    int pid;
    fstream* stream;

    const char delimiter = ';';

    void OpenFile(SString& path);

    void WriteLine(SString& type, SString& value);

}


#endif
