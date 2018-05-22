// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace R2RDump
{
    class R2RReader
    {
        /// <summary>
        /// Byte array containing the ReadyToRun image
        /// </summary>
        private readonly byte[] _image;

        private readonly PEReader peReader;

        /// <summary>
        /// Name of the image file
        /// </summary>
        public string Filename { get; }

        /// <summary>
        /// True if the image is ReadyToRun
        /// </summary>
        public bool IsR2R { get; }

        /// <summary>
        /// The type of target machine
        /// </summary>
        public Machine Machine { get; }

        /// <summary>
        /// The preferred address of the first byte of image when loaded into memory; 
        /// must be a multiple of 64K.
        /// </summary>
        public ulong ImageBase { get; }

        /// <summary>
        /// The ReadyToRun header
        /// </summary>
        public R2RHeader R2RHeader { get; }

        /// <summary>
        /// The runtime functions and method signatures of each method
        /// TODO: generic methods
        /// </summary>
        public List<R2RMethod> R2RMethods { get; }

        /// <summary>
        /// Initializes the fields of the R2RHeader
        /// </summary>
        /// <param name="filename">PE image</param>
        /// <exception cref="BadImageFormatException">The Cor header flag must be ILLibrary</exception>
        public unsafe R2RReader(string filename)
        {
            Filename = filename;
            _image = File.ReadAllBytes(filename);

            fixed (byte* p = _image)
            {
                IntPtr ptr = (IntPtr)p;
                peReader = new PEReader(p, _image.Length);

                IsR2R = (peReader.PEHeaders.CorHeader.Flags == CorFlags.ILLibrary);
                if (!IsR2R)
                {
                    throw new BadImageFormatException("The file is not a ReadyToRun image");
                }

                Machine = peReader.PEHeaders.CoffHeader.Machine;
                ImageBase = peReader.PEHeaders.PEHeader.ImageBase;

                // initialize R2RHeader
                DirectoryEntry r2rHeaderDirectory = peReader.PEHeaders.CorHeader.ManagedNativeHeaderDirectory;
                int r2rHeaderOffset = GetOffset(r2rHeaderDirectory.RelativeVirtualAddress);
                R2RHeader = new R2RHeader(_image, r2rHeaderDirectory.RelativeVirtualAddress, r2rHeaderOffset);
                if (r2rHeaderDirectory.Size != R2RHeader.Size)
                {
                    throw new BadImageFormatException("The calculated size of the R2RHeader doesn't match the size saved in the ManagedNativeHeaderDirectory");
                }

                // initialize R2RMethods
                if (peReader.HasMetadata)
                {
                    var mdReader = peReader.GetMetadataReader();

                    int runtimeFunctionSize = 2;
                    if (Machine == Machine.Amd64)
                    {
                        runtimeFunctionSize = 3;
                    }
                    runtimeFunctionSize *= sizeof(int);
                    R2RSection runtimeFunctionSection = R2RHeader.Sections[R2RSection.SectionType.READYTORUN_SECTION_RUNTIME_FUNCTIONS];
                    uint nRuntimeFunctions = (uint)(runtimeFunctionSection.Size / runtimeFunctionSize);
                    int runtimeFunctionOffset = GetOffset(runtimeFunctionSection.RelativeVirtualAddress);
                    bool[] isEntryPoint = new bool[nRuntimeFunctions];
                    for (int i = 0; i < nRuntimeFunctions; i++)
                    {
                        isEntryPoint[i] = false;
                    }

                    // initialize R2RMethods with method signatures from MethodDefHandle, and runtime function indices from MethodDefEntryPoints
                    int methodDefEntryPointsRVA = R2RHeader.Sections[R2RSection.SectionType.READYTORUN_SECTION_METHODDEF_ENTRYPOINTS].RelativeVirtualAddress;
                    int methodDefEntryPointsOffset = GetOffset(methodDefEntryPointsRVA);
                    NativeArray methodEntryPoints = new NativeArray(_image, (uint)methodDefEntryPointsOffset);
                    uint nMethodEntryPoints = methodEntryPoints.GetCount();
                    R2RMethods = new List<R2RMethod>();
                    for (uint rid = 1; rid <= nMethodEntryPoints; rid++)
                    {
                        int offset = 0;
                        if (methodEntryPoints.TryGetAt(_image, rid - 1, ref offset))
                        {
                            R2RMethod method = new R2RMethod(_image, mdReader, methodEntryPoints, rid, offset);

                            if (method.EntryPointRuntimeFunctionId >= 0 && method.EntryPointRuntimeFunctionId < nRuntimeFunctions)
                            {
                                isEntryPoint[method.EntryPointRuntimeFunctionId] = true;
                            }
                            R2RMethods.Add(method);
                        }
                    }

                    // instance method table
                    R2RSection instMethodEntryPointSection = R2RHeader.Sections[R2RSection.SectionType.READYTORUN_SECTION_INSTANCE_METHOD_ENTRYPOINTS];
                    int instMethodEntryPointsOffset = GetOffset(instMethodEntryPointSection.RelativeVirtualAddress);
                    NativeParser parser = new NativeParser(_image, (uint)instMethodEntryPointsOffset);
                    var instMethodEntryPoints = new NativeHashTable(mdReader, _image, parser);
                    int curOffset = instMethodEntryPointsOffset + (int)instMethodEntryPoints.End + 1;
                    while (curOffset < instMethodEntryPointsOffset + instMethodEntryPointSection.Size)
                    {
                        byte methodFlags = ReadByte(_image, ref curOffset);
                        byte rid = ReadByte(_image, ref curOffset);
                        if ((methodFlags & (byte)R2RMethod.EncodeMethodSigFlags.ENCODE_METHOD_SIG_MethodInstantiation) != 0)
                        {
                            byte nArgs = ReadByte(_image, ref curOffset);
                            R2RMethod.GenericElementTypes[] args = new R2RMethod.GenericElementTypes[nArgs];
                            Array.Copy(_image, curOffset, args, 0, nArgs);
                            curOffset += nArgs;

                            uint id = 0;
                            curOffset = (int)DecodeUnsigned(_image, (uint)curOffset, ref id);
                            id = id >> 1;
                            R2RMethod method = new R2RMethod(_image, mdReader, rid, (int)id, args);
                            if (method.EntryPointRuntimeFunctionId >= 0 && method.EntryPointRuntimeFunctionId < nRuntimeFunctions)
                            {
                                isEntryPoint[method.EntryPointRuntimeFunctionId] = true;
                            }
                            R2RMethods.Add(method);
                        }
                    }

                    // get the RVAs of the runtime functions for each method
                    foreach (R2RMethod method in R2RMethods)
                    {
                        int runtimeFunctionId = method.EntryPointRuntimeFunctionId;
                        if (runtimeFunctionId == -1)
                            continue;
                        curOffset = runtimeFunctionOffset + runtimeFunctionId * runtimeFunctionSize;
                        do
                        {
                            int startRva = ReadInt32(_image, ref curOffset);
                            int endRva = -1;
                            if (Machine == Machine.Amd64)
                            {
                                endRva = ReadInt32(_image, ref curOffset);
                            }
                            int unwindRva = ReadInt32(_image, ref curOffset);

                            method.NativeCode.Add(new RuntimeFunction(startRva, endRva, unwindRva));
                            runtimeFunctionId++;
                        }
                        while (runtimeFunctionId < nRuntimeFunctions && !isEntryPoint[runtimeFunctionId]);
                    }
                }
            }
        }

        /// <summary>
        /// Get the index in the image byte array corresponding to the RVA
        /// </summary>
        /// <param name="rva">The relative virtual address</param>
        public int GetOffset(int rva)
        {
            int index = peReader.PEHeaders.GetContainingSectionIndex(rva);
            SectionHeader containingSection = peReader.PEHeaders.SectionHeaders[index];
            return rva - containingSection.VirtualAddress + containingSection.PointerToRawData;
        }

        /// <summary>
        /// Extracts a 64bit value from the image byte array
        /// </summary>
        /// <param name="image">PE image</param>
        /// <param name="start">Starting index of the value</param>
        /// <remarks>
        /// The <paramref name="start"/> gets incremented by the size of the value
        /// </remarks>
        public static long ReadInt64(byte[] image, ref int start)
        {
            int size = sizeof(long);
            byte[] bytes = new byte[size];
            Array.Copy(image, start, bytes, 0, size);
            start += size;
            return BitConverter.ToInt64(bytes, 0);
        }

        // <summary>
        /// Extracts a 32bit value from the image byte array
        /// </summary>
        /// <param name="image">PE image</param>
        /// <param name="start">Starting index of the value</param>
        /// <remarks>
        /// The <paramref name="start"/> gets incremented by the size of the value
        /// </remarks>
        public static int ReadInt32(byte[] image, ref int start)
        {
            int size = sizeof(int);
            byte[] bytes = new byte[size];
            Array.Copy(image, start, bytes, 0, size);
            start += size;
            return BitConverter.ToInt32(bytes, 0);
        }

        // <summary>
        /// Extracts an unsigned 32bit value from the image byte array
        /// </summary>
        /// <param name="image">PE image</param>
        /// <param name="start">Starting index of the value</param>
        /// <remarks>
        /// The <paramref name="start"/> gets incremented by the size of the value
        /// </remarks>
        public static uint ReadUInt32(byte[] image, ref int start)
        {
            int size = sizeof(int);
            byte[] bytes = new byte[size];
            Array.Copy(image, start, bytes, 0, size);
            start += size;
            return (uint)BitConverter.ToInt32(bytes, 0);
        }

        // <summary>
        /// Extracts an unsigned 16bit value from the image byte array
        /// </summary>
        /// <param name="image">PE image</param>
        /// <param name="start">Starting index of the value</param>
        /// <remarks>
        /// The <paramref name="start"/> gets incremented by the size of the value
        /// </remarks>
        public static ushort ReadUInt16(byte[] image, ref int start)
        {
            int size = sizeof(short);
            byte[] bytes = new byte[size];
            Array.Copy(image, start, bytes, 0, size);
            start += size;
            return (ushort)BitConverter.ToInt16(bytes, 0);
        }

        // <summary>
        /// Extracts byte from the image byte array
        /// </summary>
        /// <param name="image">PE image</param>
        /// <param name="start">Start index of the value</param>
        /// /// <remarks>
        /// The <paramref name="start"/> gets incremented by the size of the value
        /// </remarks>
        public static byte ReadByte(byte[] image, ref int start)
        {
            byte val = image[start];
            start += sizeof(byte);
            return val;
        }

        public static uint DecodeUnsigned(byte[] image, uint offset, ref uint pValue)
        {
            if (offset >= image.Length)
                throw new System.BadImageFormatException("offset out of bounds");

            int off = (int)offset;
            uint val = ReadByte(image, ref off);

            if ((val & 1) == 0)
            {
                pValue = (val >> 1);
                offset += 1;
            }
            else if ((val & 2) == 0)
            {
                if (offset + 1 >= image.Length)
                    throw new System.BadImageFormatException("offset out of bounds");

                pValue = (val >> 2) |
                      ((uint)ReadByte(image, ref off) << 6);
                offset += 2;
            }
            else if ((val & 4) == 0)
            {
                if (offset + 2 >= image.Length)
                    throw new System.BadImageFormatException("offset out of bounds");

                pValue = (val >> 3) |
                      ((uint)ReadByte(image, ref off) << 5) |
                      ((uint)ReadByte(image, ref off) << 13);
                offset += 3;
            }
            else if ((val & 8) == 0)
            {
                if (offset + 3 >= image.Length)
                    throw new System.BadImageFormatException("offset out of bounds");

                pValue = (val >> 4) |
                      ((uint)ReadByte(image, ref off) << 4) |
                      ((uint)ReadByte(image, ref off) << 12) |
                      ((uint)ReadByte(image, ref off) << 20);
                offset += 4;
            }
            else if ((val & 16) == 0)
            {
                pValue = ReadUInt32(image, ref off);
                offset += 5;
            }
            else
            {
                throw new System.BadImageFormatException("DecodeUnsigned");
            }

            return offset;
        }

        public static uint DecodeSigned(byte[] image, uint offset, ref int pValue)
        {
            if (offset >= image.Length)
                throw new System.BadImageFormatException("offset out of bounds");

            int off = (int)offset;
            int val = ReadByte(image, ref off);

            if ((val & 1) == 0)
            {
                pValue = (val >> 1);
                offset += 1;
            }
            else if ((val & 2) == 0)
            {
                if (offset + 1 >= image.Length)
                    throw new System.BadImageFormatException("offset out of bounds");

                pValue = (val >> 2) |
                      (ReadByte(image, ref off) << 6);
                offset += 2;
            }
            else if ((val & 4) == 0)
            {
                if (offset + 2 >= image.Length)
                    throw new System.BadImageFormatException("offset out of bounds");

                pValue = (val >> 3) |
                      (ReadByte(image, ref off) << 5) |
                      (ReadByte(image, ref off) << 13);
                offset += 3;
            }
            else if ((val & 8) == 0)
            {
                if (offset + 3 >= image.Length)
                    throw new System.BadImageFormatException("offset out of bounds");

                pValue = (val >> 4) |
                      (ReadByte(image, ref off) << 4) |
                      (ReadByte(image, ref off) << 12) |
                      (ReadByte(image, ref off) << 20);
                offset += 4;
            }
            else if ((val & 16) == 0)
            {
                pValue = ReadInt32(image, ref off);
                offset += 5;
            }
            else
            {
                throw new System.BadImageFormatException("DecodeSigned");
            }

            return offset;
        }
    }
}
