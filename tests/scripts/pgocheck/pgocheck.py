#!/usr/bin/env python
#
## Licensed to the .NET Foundation under one or more agreements.
## The .NET Foundation licenses this file to you under the MIT license.
## See the LICENSE file in the project root for more information.
#
##
# Title               :pgocheck.py
#
# A script to check whether or not a particular portable executable
# (e.g. EXE, DLL) was compiled using PGO technology
#
################################################################################

from pefile.pefile import PE
import struct
import argparse

# An dict to decode the type indicator of the debug directory entries
# Taken from winnt.h
debug_type = {
    0x00 : "UNKNOWN",
    0x01 : "COFF",
    0x02 : "CODEVIEW",
    0x03 : "FPO",
    0x04 : "MISC",
    0x05 : "EXCEPTION",
    0x06 : "FIXUP",
    0x07 : "OMAP_TO_SRC",
    0x08 : "OMAP_FROM_SRC",
    0x09 : "BORLAND",
    0x0a : "RESERVED10",
    0x0b : "CLSID",
    0x0c : "VC_FEATURE",
    0x0d : "POGO",
    0x0e : "ILTCG",
    0x0f : "MPX",
    0x10 : "REPRO"
}

# A dict to decode the meaning of the POGO debug directory field
# Taken from `link /dump /headers` output
pgo_type = {
    0x4C544347 : "LTCG",
    0x50475500 : "PGU"
}

# Takes a filename and returns True if the PE header indicates compilation with PGO, otherwise False
def was_compiled_with_pgo(filename):
    pe = PE(filename)

    # Find the POGO entry in the debug directory
    for entry in pe.DIRECTORY_ENTRY_DEBUG:
        entry_dict = entry.struct.dump_dict()
        typ = debug_type[entry_dict["Type"]["Value"]]
        if typ == "POGO":
            addr = entry_dict["PointerToRawData"]["Value"]

            # Go to the location pointed to by the raw data pointer and read the 4-byte code
            raw_int = None
            with open(filename, 'rb') as dll:
                dll.seek(addr)
                raw_int = dll.read(4)

            # Convert the code to an int, then compare it to the dictionary of known codes
            val = int(struct.unpack('I', raw_int)[0])
            try:
                return pgo_type[val] == "PGU"
            except KeyError:
                return False

    # There was either no DIRECTORY_ENTRY_DEBUG or did not include POGO
    return False

if __name__ == "__main__":
    from sys import stdout, stderr

    parser = argparse.ArgumentParser(description="Check if the given PE files were compiled with PGO. Fails if the files were not.")
    parser.add_argument('files', metavar='file', nargs='+', help="the files to check for PGO flags")
    parser.add_argument('--negative', action='store_true', help="fail on PGO flags found")

    args = parser.parse_args()
    # Divide up filenames which are separated by semicolons as well as the ones by spaces. Avoid duplicates
    filenames = set()
    for token in args.files:
        filenames.update(token.split(';'))

    # Check each file and exit immediately if one is found which does not meet expectations
    failed = False
    for filename in filenames:
        stdout.write(filename + " ")
        if was_compiled_with_pgo(filename):
            stdout.write(": compiled with PGO\n")
            if args.negative:
                failed = True

        else:
            stdout.write(": NOT compiled with PGO\n")
            if not args.negative:
                failed = True

    if failed:
        if not args.negative:
            stderr.write("ERROR: The files listed above must be compiled with PGO\n")
        else:
            stderr.write("ERROR: The files listed above must NOT be compiled with PGO\n")
        exit(1)