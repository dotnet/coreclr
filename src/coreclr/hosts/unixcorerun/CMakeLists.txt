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

set(DEF_SOURCES ${CMAKE_SOURCE_DIR}/src/dlls/mscoree/mscorwks_unixexports.src)
message("DEF_SOURCES=${DEF_SOURCES}")
set(EXPORTS_FILE ${CMAKE_CURRENT_BINARY_DIR}/corerun.exports)
generate_exports_file(${DEF_SOURCES} ${EXPORTS_FILE})

set(EXPORTS_LINKER_OPTION -Wl,--version-script=${EXPORTS_FILE})

add_custom_target(corerun_exports DEPENDS ${EXPORTS_FILE})
add_dependencies(corerun corerun_exports)

#set_property(TARGET corerun APPEND_STRING PROPERTY LINK_FLAGS ${EXPORTS_LINKER_OPTION})
set_property(TARGET corerun APPEND_STRING PROPERTY LINK_FLAGS " -Wl,--export-dynamic") # Necessary to get symbols exported from executable
set_property(TARGET corerun APPEND_STRING PROPERTY LINK_DEPENDS ${EXPORTS_FILE})

# FreeBSD and NetBSD implement dlopen(3) in libc
if(NOT CMAKE_SYSTEM_NAME STREQUAL FreeBSD AND NOT CMAKE_SYSTEM_NAME STREQUAL NetBSD)
    target_link_libraries(corerun 
        dl
    )
endif(NOT CMAKE_SYSTEM_NAME STREQUAL FreeBSD AND NOT CMAKE_SYSTEM_NAME STREQUAL NetBSD)


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

# Libc turns locks into no-ops if pthread was not loaded into process yet. Loading
# pthread by the process executable ensures that all locks are initialized properly.
target_link_libraries(corerun
    ${START_WHOLE_ARCHIVE}
    /home/janvorli/git/corefx/artifacts/bin/native/Linux-x64-Release/System.Native.a
    /home/janvorli/git/corefx/artifacts/bin/native/Linux-x64-Release/System.IO.Compression.Native.a
    /home/janvorli/git/corefx/artifacts/bin/native/Linux-x64-Release/System.IO.Ports.Native.a
    /home/janvorli/git/corefx/artifacts/bin/native/Linux-x64-Release/System.Net.Http.Native.a
    /home/janvorli/git/corefx/artifacts/bin/native/Linux-x64-Release/System.Net.Security.Native.a
    /home/janvorli/git/corefx/artifacts/bin/native/Linux-x64-Release/System.Security.Cryptography.Native.OpenSsl.a
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
