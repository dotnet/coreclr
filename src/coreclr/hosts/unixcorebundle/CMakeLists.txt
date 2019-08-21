project(corebundle)

include_directories(../unixcoreruncommon)
include_directories(./bundle)

add_compile_options(-fPIE)

set(COREBUNDLE_SOURCES
    corebundle.cpp
    ./bundle/dir_utils.cpp
    ./bundle/extractor.cpp
    ./bundle/file_entry.cpp
    ./bundle/header.cpp
    ./bundle/manifest.cpp
    ./bundle/marker.cpp
    ./bundle/pal.cpp
    ./bundle/reader.cpp
    ./bundle/runner.cpp
    ./bundle/trace.cpp
    ./bundle/utils.cpp
)

set(HEADERS
    ./bundle/dir_utils.h
    ./bundle/error_codes.h
    ./bundle/extractor.h
    ./bundle/file_entry.h
    ./bundle/file_type.h
    ./bundle/header.h
    ./bundle/manifest.h
    ./bundle/marker.h
    ./bundle/pal.h
    ./bundle/reader.h
    ./bundle/runner.h
    ./bundle/trace.h
    ./bundle/utils.h
)

_add_executable(corebundle
    ${COREBUNDLE_SOURCES}
)

set_property(TARGET corebundle APPEND_STRING PROPERTY LINK_FLAGS " -Wl,--export-dynamic") # Necessary to get symbols exported from executable

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

set(NETCOREAPP_NATIVEPACKAGEPATH "${CLR_CMAKE_PACKAGES_DIR}/runtime.${CLR_CMAKE_DISTRORID}.microsoft.private.corefx.netcoreapp")
set(NETCOREAPP_NATIVEPACKAGEPATH "${NETCOREAPP_NATIVEPACKAGEPATH}/${CLR_CMAKE_COREFXPACKAGE_VERSION}/runtimes/${CLR_CMAKE_DISTRORID}/native")
string(TOLOWER "${NETCOREAPP_NATIVEPACKAGEPATH}" NETCOREAPP_NATIVEPACKAGEPATH)

target_link_libraries(corebundle
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

# Libc turns locks into no-ops if pthread was not loaded into process yet. Loading
# pthread by the process executable ensures that all locks are initialized properly.
if(NOT CLR_CMAKE_PLATFORM_ANDROID)
  target_link_libraries(corebundle
    pthread
  )
endif()

install_clr(corebundle)
