project(unixcorerun)

include_directories(../unixcoreruncommon)

add_compile_options(-fPIE)

set(CORERUN_SOURCES
    corerun.cpp
)

_add_executable(corerun
    ${CORERUN_SOURCES}
)

# ??? set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} -Xlinker -Bsymbolic -Xlinker -Bsymbolic-functions")

set_property(TARGET corerun APPEND_STRING PROPERTY LINK_FLAGS " -Wl,--export-dynamic") # Necessary to get symbols exported from executable

# FreeBSD and NetBSD implement dlopen(3) in libc
#if(NOT CMAKE_SYSTEM_NAME STREQUAL FreeBSD AND NOT CMAKE_SYSTEM_NAME STREQUAL NetBSD)
#    target_link_libraries(corerun
#        dl
#    )
#endif(NOT CMAKE_SYSTEM_NAME STREQUAL FreeBSD AND NOT CMAKE_SYSTEM_NAME STREQUAL NetBSD)


# The following linked options can be inserted into the linker libraries list to
# ensure proper resolving of circular references between a subset of the libraries.
set(START_LIBRARY_GROUP -Wl,--start-group)
set(END_LIBRARY_GROUP -Wl,--end-group)

# These options are used to force every object to be included even if it's unused.
set(START_WHOLE_ARCHIVE -Wl,--whole-archive)
set(END_WHOLE_ARCHIVE -Wl,--no-whole-archive)

find_package(ZLIB REQUIRED)
find_package(CURL)
find_library(LIBGSS NAMES gssapi_krb5) # TODO: this needs to search for various options

set(NETCOREAPP_NATIVEPACKAGEPATH "${CLR_CMAKE_PACKAGES_DIR}/runtime.${CLR_CMAKE_TARGET_OS}-${CLR_CMAKE_TARGET_ARCH}.microsoft.private.corefx.netcoreapp")
set(NETCOREAPP_NATIVEPACKAGEPATH "${NETCOREAPP_NATIVEPACKAGEPATH}/${CLR_CMAKE_COREFXPACKAGE_VERSION}/runtimes/${CLR_CMAKE_TARGET_OS}-${CLR_CMAKE_TARGET_ARCH}/native")
string(TOLOWER "${NETCOREAPP_NATIVEPACKAGEPATH}" NETCOREAPP_NATIVEPACKAGEPATH)

# Libc turns locks into no-ops if pthread was not loaded into process yet. Loading
# pthread by the process executable ensures that all locks are initialized properly.
target_link_libraries(corerun
    ${START_WHOLE_ARCHIVE}
    ${NETCOREAPP_NATIVEPACKAGEPATH}/System.Native.a
    ${NETCOREAPP_NATIVEPACKAGEPATH}/System.IO.Compression.Native.a
    ${NETCOREAPP_NATIVEPACKAGEPATH}/System.Net.Http.Native.a
    ${NETCOREAPP_NATIVEPACKAGEPATH}/System.Net.Security.Native.a
    ${NETCOREAPP_NATIVEPACKAGEPATH}/System.Security.Cryptography.Native.OpenSsl.a
    System.Globalization.Native_Static
    ${END_WHOLE_ARCHIVE}

    ${ZLIB_LIBRARIES}
    ${CURL_LIBRARIES}
    ${LIBGSS}

    ${START_LIBRARY_GROUP} # Start group of libraries that have circular references
    unixcoreruncommon
    coreclr_static
# This is hacky, but the only way I've found to make the library group here work. Unfortunately, cmake emits all the libraries the coreclr_static depends on
# right after coreclr_static and the presence of end group after that somehow screws it.
#    ${END_LIBRARY_GROUP} # End group of libraries that have circular references
# Possible clean solution would be to make coreclr_static not to depend on any libraries and copy the library list from coreclr here. But that would duplicate it
)

# Android implements pthread natively
if(NOT CLR_CMAKE_PLATFORM_ANDROID)
  target_link_libraries(corerun
    pthread
  )
endif()

install_clr(corerun)
