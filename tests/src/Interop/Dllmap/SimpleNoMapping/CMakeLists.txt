cmake_minimum_required (VERSION 2.6)
project (LibraryNoMapping)
set(SOURCES LibraryNoMapping.cpp )

# add the executable
add_library (LibraryNoMapping SHARED ${SOURCES})
target_link_libraries(LibraryNoMapping ${LINK_LIBRARIES_ADDITIONAL}) 

# add the install targets
install (TARGETS LibraryNoMapping DESTINATION bin)
