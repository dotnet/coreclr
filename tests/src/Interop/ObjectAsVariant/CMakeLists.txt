cmake_minimum_required (VERSION 2.6)
project (ObjectAsVariantNative)
set(SOURCES ObjectAsVariantNative.cpp)

# add the executable
add_library (ObjectAsVariantNative SHARED ${SOURCES})
target_link_libraries(ObjectAsVariantNative ${LINK_LIBRARIES_ADDITIONAL})

# add the install targets
install (TARGETS ObjectAsVariantNative DESTINATION bin)


