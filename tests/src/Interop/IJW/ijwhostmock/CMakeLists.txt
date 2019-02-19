cmake_minimum_required (VERSION 2.6)
project (mscoree)
include_directories( ${INC_PLATFORM_DIR} )
set(MSCOREE_SOURCES mscoree.cpp MscoreeExports.def)
set(IJWHOST_MOCK_SOURCES ijwhost.cpp IjwHostExports.def)

# add the shared library
add_library(ijwhost SHARED ${IJWHOST_MOCK_SOURCES})
add_library (mscoree SHARED ${MSCOREE_SOURCES})
target_link_libraries(ijwhost ${LINK_LIBRARIES_ADDITIONAL})
target_link_libraries(mscoree ${LINK_LIBRARIES_ADDITIONAL} ijwhost)

# add the install targets
install (TARGETS mscoree ijwhost DESTINATION bin)
