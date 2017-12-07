cmake_minimum_required(VERSION 2.8.12.2)

project(xmlparser)

include(../../clrfeatures.cmake)

add_definitions(-DUNICODE)
add_definitions(-D_UNICODE)
add_definitions(-DFEATURE_NO_HOST)

set(xmlparser_SOURCES
    bufferedstream.cpp
    charencoder.cpp
    encodingstream.cpp
    xmlhelper.cpp
    xmlparser.cpp
    xmlstream.cpp
    _rawstack.cpp
    _reference.cpp
)

add_compile_options(-fPIC)

add_library(xmlparser STATIC ${xmlparser_SOURCES})