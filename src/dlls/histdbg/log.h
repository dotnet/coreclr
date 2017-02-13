#include <stdarg.h>
#include "historicaldebugging.inc"

void LogInitialize();
void LogFinalize();
void LogProfilerActivity(const char *format, ...);
void LogCheckpointWrite(CheckpointData *checkpoint);
