project(corebundle)

include_directories(../bundle)
include_directories(../unixcoreruncommon)

include(../bundle/CMakeLists.txt)

add_compile_options(-fPIE)

include(configure.cmake)

add_definitions(-DCOREBUNDLE_BUILD)

set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} -Wl,--gc-sections")

set(COREBUNDLE_SOURCES
    corebundle.cpp
    ../unixcoreruncommon/coreruncommon.cpp
    ${Bundle_Unix_SOURCES}
)

set(HEADERS
    ../unixcoreruncommon/coreruncommon.h
    ${Bundle_HEADERS}
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

if (HAVE_GSSFW_HEADERS)
   find_library(LIBGSS NAMES GSS)
   if(LIBGSS STREQUAL LIBGSS-NOTFOUND)
      message(FATAL_ERROR "Cannot find GSS.Framework and System.Net.Security.Native cannot build without it. Try installing GSS.Framework (or the appropriate package for your platform)")
   endif()
elseif(HAVE_HEIMDAL_HEADERS)
   find_library(LIBGSS NAMES gssapi)
   if(LIBGSS STREQUAL LIBGSS-NOTFOUND)
      message(FATAL_ERROR "Cannot find libgssapi and System.Net.Security.Native cannot build without it. Try installing heimdal (or the appropriate package for your platform)")
   endif()
else()
   find_library(LIBGSS NAMES gssapi_krb5)
   if(LIBGSS STREQUAL LIBGSS-NOTFOUND)
      message(FATAL_ERROR "Cannot find libgssapi_krb5 and System.Net.Security.Native cannot build without it. Try installing libkrb5-dev (or the appropriate package for your platform)")
   endif()
endif()

find_package(OpenSSL)
if(NOT OPENSSL_FOUND)
    message(FATAL_ERROR "!!! Cannot find libssl and System.Security.Cryptography.Native cannot build without it. Try installing libssl-dev (or the appropriate package for your platform) !!!")
endif()

set(NETCOREAPP_NATIVEPACKAGEPATH "${CLR_CMAKE_COREFXPACKAGE_PATH}")
set(NETCOREAPP_NATIVEPACKAGEPATH "${NETCOREAPP_NATIVEPACKAGEPATH}/runtimes/${CLR_CMAKE_DISTRORID}/native")
string(TOLOWER "${NETCOREAPP_NATIVEPACKAGEPATH}" NETCOREAPP_NATIVEPACKAGEPATH)

# We intentionally do not link in:
#   System.IO.Ports.Native.a - not part of netcoreapp
#   System.Net.Http.Native.a - only for legacy curl HTTP networking
target_link_libraries(corebundle
    ${START_WHOLE_ARCHIVE}
    ${NETCOREAPP_NATIVEPACKAGEPATH}/System.Native.a
    ${NETCOREAPP_NATIVEPACKAGEPATH}/System.IO.Compression.Native.a
    ${NETCOREAPP_NATIVEPACKAGEPATH}/System.Net.Security.Native.a
    ${NETCOREAPP_NATIVEPACKAGEPATH}/System.Security.Cryptography.Native.OpenSsl.a

    System.Globalization.Native_Static
    ${END_WHOLE_ARCHIVE}

    ${ZLIB_LIBRARIES}
    ${CURL_LIBRARIES}
    ${LIBGSS}
    ${OPENSSL_CRYPTO_LIBRARY}
    ${OPENSSL_SSL_LIBRARY}

    ${START_LIBRARY_GROUP} # Start group of libraries that have circular references
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
