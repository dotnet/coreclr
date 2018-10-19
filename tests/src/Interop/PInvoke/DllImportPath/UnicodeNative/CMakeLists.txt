cmake_minimum_required (VERSION 2.6)
project (DllImportPath_U�n�i�c�o�d�e)
include ("${CLR_INTEROP_TEST_ROOT}/Interop.cmake")
include_directories(${INC_PLATFORM_DIR})
set(SOURCES
    ../DllImportPathNative.cpp
)
# Additional files to reference:
# add the executable
add_library (DllImportPath_U�n�i�c�o�d�e SHARED ${SOURCES})
target_link_libraries(DllImportPath_U�n�i�c�o�d�e ${LINK_LIBRARIES_ADDITIONAL})
# add the install targets
install (TARGETS DllImportPath_U�n�i�c�o�d�e DESTINATION bin)
