##
## Licensed to the .NET Foundation under one or more agreements.
## The .NET Foundation licenses this file to you under the MIT license.
## See the LICENSE file in the project root for more information.
##
##  This script exists to create a dummy implementaion of the EtX
##  interface from a manifest file
##
##  The intended use if for platforms which support event pipe
##  but do not have a an eventing platform to recieve report ecents

import os
from genXplatEventing import *

stdprolog="""
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/******************************************************************

DO NOT MODIFY. AUTOGENERATED FILE.
This file is generated using the logic from <root>/src/scripts/genXplatDummy.py

******************************************************************/
"""
stdprolog_cmake="""
#
#
#******************************************************************

#DO NOT MODIFY. AUTOGENERATED FILE.
#This file is generated using the logic from <root>/src/scripts/genXplatDummy.py

#******************************************************************
"""

def trimProvName(name):
    name = name.replace("Windows-",'')
    name = name.replace("Microsoft-",'')
    name = name.replace('-','_')
    return name

def escapeProvFilename(name):
    name = name.replace('_','')
    name = name.lower()
    return name

def generateDummyProvider(providerName, eventNodes, allTemplates):
    impl = []
    for eventNode in eventNodes:
        eventName    = eventNode.getAttribute('symbol')
        templateName = eventNode.getAttribute('template')
        #generate EventXplatEnabled
        impl.append("extern \"C\" BOOL  EventXplatEnabled%s(){ return FALSE; }\n\n" % (eventName,))
        #generate FireEtw functions
        fnptype = []
        linefnptype = []
        fnptype.append("extern \"C\" ULONG  FireEtXplat")
        fnptype.append(eventName)
        fnptype.append("(\n")


        if templateName:
            template = allTemplates[templateName]
        else:
            template = None

        if template:
            fnSig   = template.signature
            for paramName in fnSig.paramlist:
                fnparam     = fnSig.getParam(paramName)
                wintypeName = fnparam.winType
                typewName   = palDataTypeMapping[wintypeName]
                winCount    = fnparam.count
                countw      = palDataTypeMapping[winCount]

                if paramName in template.structs:
                    linefnptype.append("%sint %s_ElementSize,\n" % (lindent, paramName))

                linefnptype.append(lindent)
                linefnptype.append(typewName)
                if countw != " ":
                    linefnptype.append(countw)

                linefnptype.append(" ")
                linefnptype.append(fnparam.name)
                linefnptype.append(",\n")

            if len(linefnptype) > 0 :
                del linefnptype[-1]

        fnptype.extend(linefnptype)
        fnptype.append(")\n{\n")
        impl.extend(fnptype)

        #start of fn body
        impl.append("    return ERROR_SUCCESS;\n")
        impl.append("}\n\n")

    return ''.join(impl)

def generateDummyFiles(etwmanifest,eventprovider_directory):

    eventprovider_directory = eventprovider_directory + "/"
    tree                    = DOM.parse(etwmanifest)

    #keep these relative
    dummy_directory              =  "dummy"

    dummyevntprovPre             = dummy_directory + "/eventprov"

    if not os.path.exists(eventprovider_directory):
        os.makedirs(eventprovider_directory)

    if not os.path.exists(eventprovider_directory + dummy_directory):
        os.makedirs(eventprovider_directory + dummy_directory)

    # Top level Cmake
    with open(eventprovider_directory + "CMakeLists.txt", 'w') as topCmake:
        topCmake.write(stdprolog_cmake + "\n")
        topCmake.write("""cmake_minimum_required(VERSION 2.8.12.2)

        project(eventprovider)

        set(CMAKE_INCLUDE_CURRENT_DIR ON)

        add_definitions(-DPAL_STDCPP_COMPAT=1)
        include_directories(${COREPAL_SOURCE_DIR}/inc/rt)
        include_directories(dummy)

        add_library(eventprovider
            STATIC
    """)

        for providerNode in tree.getElementsByTagName('provider'):
            providerName = trimProvName(providerNode.getAttribute('name'))
            providerName_File = escapeProvFilename(providerName)

            topCmake.write('        "%s%s.cpp"\n' % (dummyevntprovPre, providerName_File))

        topCmake.write(""")

        # Install the static eventprovider library
        install(TARGETS eventprovider DESTINATION lib)
        """)

    # Dummy Specific Instrumentation
    for providerNode in tree.getElementsByTagName('provider'):
        providerName = trimProvName(providerNode.getAttribute('name'))
        providerName_File = escapeProvFilename(providerName)

        dummyevntprov = eventprovider_directory + dummyevntprovPre + providerName_File + ".cpp"

        impl = open(dummyevntprov, 'w')

        impl.write(stdprolog + "\n")

        impl.write("""
#include "stdlib.h"
#include "pal_mstypes.h"
#include "pal_error.h"
#include "pal.h"
#include "pal/stackstring.hpp"


""")

        templateNodes = providerNode.getElementsByTagName('template')
        eventNodes = providerNode.getElementsByTagName('event')

        allTemplates = parseTemplateNodes(templateNodes)

        #create the implementation of eventing functions : dummyeventprov*.cp
        impl.write(generateDummyProvider(providerName,eventNodes,allTemplates) + "\n")

        impl.close()

def main(argv):

    #parse the command line
    parser = argparse.ArgumentParser(description="Generates the Code required to instrument LTTtng logging mechanism")

    required = parser.add_argument_group('required arguments')
    required.add_argument('--man',  type=str, required=True,
                                    help='full path to manifest containig the description of events')
    required.add_argument('--intermediate', type=str, required=True,
                                    help='full path to eventprovider  intermediate directory')
    args, unknown = parser.parse_known_args(argv)
    if unknown:
        print('Unknown argument(s): ', ', '.join(unknown))
        return const.UnknownArguments

    sClrEtwAllMan     = args.man
    intermediate      = args.intermediate

    generateDummyFiles(sClrEtwAllMan,intermediate)

if __name__ == '__main__':
    return_code = main(sys.argv[1:])
    sys.exit(return_code)
