
# This portion of the tree is for portions of CoreFX libraries which are (1) best
# suited to having been implemented in native code and (2) need to be invoked from
# mscorlib.dll. If mscorlib.dll does not use the library, the code should go in
# the src/Native directory of dotnet/corefx.
#
# The libraries should disable the "lib" prefix, and use an assembly-style naming format,
# such as System.Security.Cryptography.Native (shared across multiple System.Security.Cryptography.*
# assemblies) or System.Diagnostics.Process.Native (a native interop library for
# System.Diagnostics.Process.dll).

remove_definitions(-DAMD64)
remove_definitions(-D_AMD64_)
remove_definitions(-D_WIN64)
remove_definitions(-D_TARGET_AMD64_=1)
remove_definitions(-DDBG_TARGET_64BIT=1)
remove_definitions(-DDBG_TARGET_AMD64=1)
remove_definitions(-DDBG_TARGET_WIN64=1)
remove_definitions(-DDBG_TARGET_AMD64_UNIX=1)


if(CLR_CMAKE_PLATFORM_UNIX)
  add_subdirectory(System.Globalization.Native)
endif(CLR_CMAKE_PLATFORM_UNIX)
