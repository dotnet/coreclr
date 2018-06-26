#
## Licensed to the .NET Foundation under one or more agreements.
## The .NET Foundation licenses this file to you under the MIT license.
## See the LICENSE file in the project root for more information.
#

import os
import xml.dom.minidom as DOM
from utilities import open_for_update
import argparse
import sys

generatedCodeFileHeader="""// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/**********************************************************************

DO NOT MODIFY. AUTOGENERATED FILE.
This file is generated by <root>/src/scripts/genRuntimeEventSources.py

**********************************************************************/
"""

########################################################################
# START CONFIGURATION
########################################################################
manifestsToGenerate = {
    "Microsoft-Windows-DotNETRuntime" : "DotNETRuntimeEventSource.cs"
}

providerNameToClassNameMap = {
    "Microsoft-Windows-DotNETRuntime" : "RuntimeEventSource"
}

manifestTypeToCSharpTypeMap = {
    "win:UInt8" : "byte",
    "win:UInt16" : "UInt16",
    "win:UInt32" : "UInt32",
    "win:UInt64" : "UInt64",
    "win:Int32" : "Int32",
    "win:Pointer" : "IntPtr",
    "win:UnicodeString" : "string",
    "win:Binary" : "byte[]",
    "win:Double" : "double",
    "win:Boolean" : "bool",
    "win:GUID" : "Guid",
}

overrideEnumBackingTypes = {
    "Microsoft-Windows-DotNETRuntime" : {
        "GCSuspendEEReasonMap" : "win:UInt32",
        "GCRootKindMap" : "win:UInt32"
    }
}
########################################################################
# END CONFIGURATION
########################################################################

tabText = ""

def increaseTabLevel():
    global tabText
    tabText += "    "

def decreaseTabLevel():
    global tabText
    tabText = tabText[:-4]

def writeOutput(outputFile, str):
    outputFile.write(tabText + str)

def getCSharpTypeFromManifestType(manifestType):
    return manifestTypeToCSharpTypeMap[manifestType]

def generateEvent(eventNode, providerNode, outputFile, stringTable):

    # Write the event attribute.
    writeOutput(outputFile, "[Event("+ eventNode.getAttribute("value") + ", Version = " + eventNode.getAttribute("version") + ", Level = EventLevel." + eventNode.getAttribute("level")[4:])
    
    # Not all events have keywords specified, and some have multiple keywords specified.
    keywords = eventNode.getAttribute("keywords")
    if keywords:
        if " " not in keywords:
            outputFile.write(", Keywords = Keywords." + keywords)
        else:
            keywords = keywords.split()
            outputFile.write(", Keywords = ")
            for keywordIndex in range(len(keywords)):
                outputFile.write("Keywords." + keywords[keywordIndex])
                if keywordIndex < (len(keywords) - 1):
                    outputFile.write(" | ")

    outputFile.write(")]\n")

    # Get the template for the event.
    templateNode = None
    templateKey = eventNode.getAttribute("template")
    if templateKey is not None:
        for node in providerNode.getElementsByTagName("templates"):
            templatesNode = node
            break
        for node in templatesNode.getElementsByTagName("template"):
            if node.getAttribute("tid") == templateKey:
                templateNode = node
                break

    # Write the beginning of the method signature.
    writeOutput(outputFile, "private void " + eventNode.getAttribute("symbol") + "(")

    # Write the function signature.
    argumentCount = 0
    if templateNode is not None:
        argumentNodes = templateNode.childNodes

        # Calculate the number of arguments.
        for argumentNode in argumentNodes:
            if argumentNode.nodeName == "data":
                if argumentNode.getAttribute("inType") != "win:Binary" and argumentNode.getAttribute("inType") != "win:AnsiString" and argumentNode.getAttribute("count") == "":
                    argumentCount += 1
                else:
                    break
            elif argumentNode.nodeName == "struct":
                break

        argumentsProcessed = 0
        for argumentIndex in range(len(argumentNodes)):
            argumentNode = argumentNodes[argumentIndex]
            if argumentNode.nodeName == "data":
                argumentName = argumentNode.getAttribute("name")
                argumentInType = argumentNode.getAttribute("inType")

                #### Disable enums until they are needed ####
                # argumentMap = argumentNode.getAttribute("map")
                # if not argumentMap:
                #     argumentCSharpType = getCSharpTypeFromManifestType(argumentInType)
                # else:
                #     argumentCSharpType = argumentMap[:-3]
                #### Disable enums until they are needed ####

                argumentCSharpType = getCSharpTypeFromManifestType(argumentInType)
                outputFile.write(argumentCSharpType + " " + argumentName)
                argumentsProcessed += 1
                if argumentsProcessed < argumentCount:
                    outputFile.write(", ")
            if argumentsProcessed == argumentCount:
                break

    outputFile.write(")\n")
    writeOutput(outputFile, "{\n")

    # Write the call to WriteEvent.
    increaseTabLevel()
    writeOutput(outputFile, "WriteEvent(" + eventNode.getAttribute("value"))

    # Add method parameters.
    if argumentCount > 0:
        # A ',' is needed after the event id.
        outputFile.write(", ")

        # Write the parameter names to the method call.
        argumentsProcessed = 0
        argumentNodes = templateNode.getElementsByTagName("data")
        for argumentIndex in range(argumentCount):
            argumentNode = argumentNodes[argumentIndex]
            argumentName = argumentNode.getAttribute("name")
            outputFile.write(argumentName)
            if argumentIndex < (argumentCount - 1):
                outputFile.write(", ")

    outputFile.write(");\n")
    decreaseTabLevel()

    writeOutput(outputFile, "}\n\n")


def generateEvents(providerNode, outputFile, stringTable):

    # Get the events element.
    for node in providerNode.getElementsByTagName("events"):
        eventsNode = node
        break

    # Get the list of event nodes.
    eventNodes = eventsNode.getElementsByTagName("event")

    # Build a list of events to be emitted.  This is where old versions of events are stripped.
    # key = eventID, value = version
    eventList = dict()
    for eventNode in eventNodes:
        eventID = eventNode.getAttribute("value")
        eventVersion = eventNode.getAttribute("version")
        eventList[eventID] = eventVersion

    # Iterate over each event node and process it.
    # Only emit events for the latest version of the event, otherwise EventSource initialization will fail.
    for eventNode in eventNodes:
        eventID = eventNode.getAttribute("value")
        eventVersion = eventNode.getAttribute("version")
        if eventID in eventList and eventList[eventID] == eventVersion:
            generateEvent(eventNode, providerNode, outputFile, stringTable)
        elif eventID not in eventList:
            raise ValueError("eventID could not be found in the list of events to emit.", eventID)

def generateValueMapEnums(providerNode, outputFile, stringTable, enumTypeMap):

    # Get the maps element.
    for node in providerNode.getElementsByTagName("maps"):
        mapsNode = node
        break

    # Iterate over each map and create an enum out of it.
    for valueMapNode in mapsNode.getElementsByTagName("valueMap"):

        # Get the backing type of the enum.
        typeName = enumTypeMap[valueMapNode.getAttribute("name")]
        if typeName is None:
            raise ValueError("No mapping from mapName to enum backing type.", valueMapNode.getAttribute("name"))

        enumType = getCSharpTypeFromManifestType(typeName)
        writeOutput(outputFile, "public enum " + valueMapNode.getAttribute("name")[:-3] + " : " + enumType + "\n")
        writeOutput(outputFile, "{\n")
        increaseTabLevel()
        for mapNode in valueMapNode.getElementsByTagName("map"):
            # Each map value has a message, which we should use as the enum value.
            messageKey = mapNode.getAttribute("message")[9:-1]
            writeOutput(outputFile, stringTable[messageKey] + " = " + mapNode.getAttribute("value") + ",\n")
        decreaseTabLevel()
        writeOutput(outputFile, "}\n\n")

def generateBitMapEnums(providerNode, outputFile, stringTable, enumTypeMap):

    # Get the maps element.
    for node in providerNode.getElementsByTagName("maps"):
        mapsNode = node
        break

    # Iterate over each map and create an enum out of it.
    for valueMapNode in mapsNode.getElementsByTagName("bitMap"):

        # Get the backing type of the enum.
        typeName = enumTypeMap[valueMapNode.getAttribute("name")]
        if typeName is None:
            raise ValueError("No mapping from mapName to enum backing type.", valueMapNode.getAttribute("name"))

        enumType = getCSharpTypeFromManifestType(typeName)
        writeOutput(outputFile, "[Flags]\n")
        writeOutput(outputFile, "public enum " + valueMapNode.getAttribute("name")[:-3] + " : " + enumType + "\n")
        writeOutput(outputFile, "{\n")
        increaseTabLevel()
        for mapNode in valueMapNode.getElementsByTagName("map"):
            # Each map value has a message, which we should use as the enum value.
            messageKey = mapNode.getAttribute("message")[9:-1]
            writeOutput(outputFile, stringTable[messageKey] + " = " + mapNode.getAttribute("value") + ",\n")
        decreaseTabLevel()
        writeOutput(outputFile, "}\n\n")

def generateEnumTypeMap(providerNode):

    providerName = providerNode.getAttribute("name")
    templatesNodes = providerNode.getElementsByTagName("templates")
    templatesNode = templatesNodes[0]
    mapsNodes = providerNode.getElementsByTagName("maps")

    # Keep a list of mapName -> inType.
    # This map contains the first inType seen for the specified mapName.
    typeMap = dict()

    # There are a couple of maps that are used by multiple events but have different backing types.
    # Because only one of the uses will be consumed by EventSource/EventListener we can hack the backing type here
    # and suppress the warning that we'd otherwise get.
    overrideTypeMap = dict()
    if providerName in overrideEnumBackingTypes:
        overrideTypeMap = overrideEnumBackingTypes[providerName]

    for mapsNode in mapsNodes:
        for valueMapNode in mapsNode.getElementsByTagName("valueMap"):
            mapName = valueMapNode.getAttribute("name")
            dataNodes = templatesNode.getElementsByTagName("data")

            # If we've never seen the map used, save its usage with the inType.
            # If we have seen the map used, make sure that the inType saved previously matches the current inType.
            for dataNode in dataNodes:
                if dataNode.getAttribute("map") == mapName:
                    if mapName in overrideTypeMap:
                        typeMap[mapName] = overrideTypeMap[mapName]
                    elif mapName in typeMap and typeMap[mapName] != dataNode.getAttribute("inType"):
                        print("WARNING: Map " + mapName + " is used multiple times with different types.  This may cause functional bugs in tracing.")
                    elif not mapName in typeMap:
                        typeMap[mapName] = dataNode.getAttribute("inType")
        for bitMapNode in mapsNode.getElementsByTagName("bitMap"):
            mapName = bitMapNode.getAttribute("name")
            dataNodes = templatesNode.getElementsByTagName("data")

            # If we've never seen the map used, save its usage with the inType.
            # If we have seen the map used, make sure that the inType saved previously matches the current inType.
            for dataNode in dataNodes:
                if dataNode.getAttribute("map") == mapName:
                    if mapName in overrideTypeMap:
                        typeMap[mapName] = overrideTypeMap[mapName]
                    elif mapName in typeMap and typeMap[mapName] != dataNode.getAttribute("inType"):
                        print("Map " + mapName + " is used multiple times with different types.")
                    elif not mapName in typeMap:
                        typeMap[mapName] = dataNode.getAttribute("inType")

    return typeMap

def generateKeywordsClass(providerNode, outputFile):

    # Find the keywords element.
    for node in providerNode.getElementsByTagName("keywords"):
        keywordsNode = node
        break;

    writeOutput(outputFile, "public class Keywords\n")
    writeOutput(outputFile, "{\n")
    increaseTabLevel()

    for keywordNode in keywordsNode.getElementsByTagName("keyword"):
        writeOutput(outputFile, "public const EventKeywords " + keywordNode.getAttribute("name") + " = (EventKeywords)" + keywordNode.getAttribute("mask") + ";\n")

    decreaseTabLevel()
    writeOutput(outputFile, "}\n\n")

def loadStringTable(manifest):

    # Create the string table dictionary.
    stringTable = dict()

    # Get the string table element.
    for node in manifest.getElementsByTagName("stringTable"):
        stringTableNode = node
        break

    # Iterate through each string and save it.
    for stringElem in stringTableNode.getElementsByTagName("string"):
        stringTable[stringElem.getAttribute("id")] = stringElem.getAttribute("value")

    return stringTable

def generateEventSources(manifestFullPath, intermediatesDirFullPath):

    # Open the manifest for reading.
    manifest = DOM.parse(manifestFullPath)

    # Load the string table.
    stringTable = loadStringTable(manifest)

    # Iterate over each provider that we want to generate an EventSource for.
    for providerName, outputFileName in manifestsToGenerate.items():
        for node in manifest.getElementsByTagName("provider"):
            if node.getAttribute("name") == providerName:
                providerNode = node
                break

        if providerNode is None:
            raise ValueError("Unable to find provider node.", providerName)

        # Generate a full path to the output file and open the file for open_for_update.
        outputFilePath = os.path.join(intermediatesDirFullPath, outputFileName)
        with open_for_update(outputFilePath) as outputFile:

            # Write the license header.
            writeOutput(outputFile, generatedCodeFileHeader)

            # Write the class header.
            header = """
using System;

namespace System.Diagnostics.Tracing
{
"""
            writeOutput(outputFile, header)
            increaseTabLevel()

            className = providerNameToClassNameMap[providerName]
            writeOutput(outputFile, "[EventSource(Name = \"" + providerName + "\", Guid = \"" + providerNode.getAttribute("guid") + "\")]\n")
            writeOutput(outputFile, "internal sealed partial class " + className + " : EventSource\n")
            writeOutput(outputFile, "{\n")
            increaseTabLevel()

            # Create a static property for the EventSource name so that we don't have to initialize the EventSource to get its name.
            writeOutput(outputFile, "internal const string EventSourceName = \"" + providerName + "\";\n")

            # Write the static Log property.
            writeOutput(outputFile, "internal static " + className + " Log = new " + className + "();\n\n")

            # Write the keywords class.
            generateKeywordsClass(providerNode, outputFile)

            #### Disable enums until they are needed ####
            # Generate the enum type map.
            # This determines what the backing type for each enum should be.
            # enumTypeMap = generateEnumTypeMap(providerNode)

            # Generate enums for value maps.
            # generateValueMapEnums(providerNode, outputFile, stringTable, enumTypeMap)

            # Generate enums for bit maps.
            # generateBitMapEnums(providerNode, outputFile, stringTable, enumTypeMap)
            #### Disable enums until they are needed ####

            # Generate events.
            generateEvents(providerNode, outputFile, stringTable)

            # Write the class footer.
            decreaseTabLevel()
            writeOutput(outputFile, "}\n")
            decreaseTabLevel()
            writeOutput(outputFile, "}")

def main(argv):

    # Parse command line arguments.
    parser = argparse.ArgumentParser(
        description="Generates C# EventSource classes that represent the runtime's native event providers.")

    required = parser.add_argument_group('required arguments')
    required.add_argument('--man', type=str, required=True,
                          help='full path to manifest containig the description of events')
    required.add_argument('--intermediate', type=str, required=True,
                          help='full path to eventprovider intermediate directory')
    args, unknown = parser.parse_known_args(argv)
    if unknown:
        print('Unknown argument(s): ', ', '.join(unknown))
        return 1

    manifestFullPath = args.man
    intermediatesDirFullPath = args.intermediate

    # Ensure the intermediates directory exists.
    try:
        os.makedirs(intermediatesDirFullPath)
    except OSError:
        if not os.path.isdir(intermediatesDirFullPath):
            raise

    # Generate event sources.
    generateEventSources(manifestFullPath, intermediatesDirFullPath)
    return 0

if __name__ == '__main__':
    return_code = main(sys.argv[1:])
    sys.exit(return_code)
