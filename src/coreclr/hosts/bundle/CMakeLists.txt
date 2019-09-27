set(Bundle_Common_SOURCES  
    ${CMAKE_CURRENT_LIST_DIR}/app_bundle.cpp
    ${CMAKE_CURRENT_LIST_DIR}/file_entry.cpp
    ${CMAKE_CURRENT_LIST_DIR}/header.cpp
    ${CMAKE_CURRENT_LIST_DIR}/manifest.cpp
    ${CMAKE_CURRENT_LIST_DIR}/marker.cpp
    ${CMAKE_CURRENT_LIST_DIR}/reader.cpp
    ${CMAKE_CURRENT_LIST_DIR}/runner.cpp
    ${CMAKE_CURRENT_LIST_DIR}/utils.cpp
)

set(Bundle_Windows_SOURCES  
    ${Bundle_Common_SOURCES}
    ${CMAKE_CURRENT_LIST_DIR}/pal_windows.cpp
)

set(Bundle_Unix_SOURCES  
    ${Bundle_Common_SOURCES}
    ${CMAKE_CURRENT_LIST_DIR}/pal_unix.cpp
)

set(Bundle_HEADERS
    ${CMAKE_CURRENT_LIST_DIR}/app_bundle.h
    ${CMAKE_CURRENT_LIST_DIR}/error_codes.h
    ${CMAKE_CURRENT_LIST_DIR}/file_entry.h
    ${CMAKE_CURRENT_LIST_DIR}/file_type.h
    ${CMAKE_CURRENT_LIST_DIR}/header.h
    ${CMAKE_CURRENT_LIST_DIR}/manifest.h
    ${CMAKE_CURRENT_LIST_DIR}/marker.h
    ${CMAKE_CURRENT_LIST_DIR}/pal.h
    ${CMAKE_CURRENT_LIST_DIR}/reader.h
    ${CMAKE_CURRENT_LIST_DIR}/runner.h
    ${CMAKE_CURRENT_LIST_DIR}/utils.h
)
