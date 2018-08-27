cmake_minimum_required (VERSION 2.6)
project (Library)
set(SOURCES Library.cpp )

# add the executable
add_library (Library SHARED ${SOURCES})
target_link_libraries(Library ${LINK_LIBRARIES_ADDITIONAL}) 

# add the install targets
install (TARGETS Library DESTINATION bin)
