static char const * const FMT_LOG_FILE_PATH = "/tmp/912E73AF-F51D-4E80-894D-F4E9E6DD7C2E%d.log";

struct CheckpointData
{
    CheckpointData()
        : stackBuffer(nullptr), stackBufferSize(0), threadId(0)
    {}
    _CONTEXT registerContext;
    PBYTE stackBuffer;
    size_t stackBufferSize;
    DWORD threadId;
    ~CheckpointData() {
        if (stackBuffer)
            delete[] stackBuffer;
    }
};

typedef struct {
    DWORD id;
    _CONTEXT context;
    size_t frameSize;
} FrameHeader;
