#include <xplatform.h>
#include <stdarg.h>

extern "C" DLL_EXPORT void TestVarArgs(LPWSTR formattedString, SIZE_T bufferSize, LPCWSTR format, ...)
{
    va_list args;
    va_start(args, format);

    vswprintf_s(formattedString, bufferSize, format, args);
}

extern "C" DLL_EXPORT void TestArgIterator(LPWSTR formattedString, SIZE_T bufferSize, LPCWSTR format, va_list args)
{
    vswprintf_s(formattedString, bufferSize, format, args);
}
