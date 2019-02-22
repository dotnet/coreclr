cmake_minimum_required (VERSION 2.6)
project (mscoree)
include_directories( ${INC_PLATFORM_DIR} )
set(MSCOREE_SOURCES mscoree.cpp MscoreeExports.def)

# add the shared library
add_library (mscoree SHARED ${MSCOREE_SOURCES})
target_link_libraries(mscoree ${LINK_LIBRARIES_ADDITIONAL})

# add the install targets
install (TARGETS mscoree DESTINATION bin)
