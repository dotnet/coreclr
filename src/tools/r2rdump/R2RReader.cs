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
        /// The assembly code of the runtime functions
        /// </summary>
        public R2RMethod[] R2RMethods { get; }

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
                    throw new System.BadImageFormatException("The file is not a ReadyToRun image");
                }

                Machine = peReader.PEHeaders.CoffHeader.Machine;
                ImageBase = peReader.PEHeaders.PEHeader.ImageBase;

                // initialize the R2RHeader
                DirectoryEntry r2rHeaderDirectory = peReader.PEHeaders.CorHeader.ManagedNativeHeaderDirectory;
                int r2rHeaderOffset = GetOffset(r2rHeaderDirectory.RelativeVirtualAddress);
                R2RHeader = new R2RHeader(_image, r2rHeaderDirectory.RelativeVirtualAddress, r2rHeaderOffset);
                if (r2rHeaderDirectory.Size != R2RHeader.Size)
                {
                    throw new System.BadImageFormatException("The calculated size of the R2RHeader doesn't match the size saved in the ManagedNativeHeaderDirectory");
                }

                if (peReader.HasMetadata)
                {
                    var mdReader = peReader.GetMetadataReader();

                    // initialize R2RMethods with method signatures from MethodDefHandle, and runtime function indices from MethodDefEntryPoints
                    int i = 0;
                    int methodDefEntryPointsRVA = R2RHeader.Sections[R2RSection.SectionType.READYTORUN_SECTION_METHODDEF_ENTRYPOINTS].RelativeVirtualAddress;
                    int methodDefEntryPointsOffset = GetOffset(methodDefEntryPointsRVA);
                    NativeArray methodEntryPoints = new NativeArray(_image, (uint)methodDefEntryPointsOffset);
                    uint nMethods = methodEntryPoints.GetCount();
                    R2RMethods = new R2RMethod[nMethods];
                    for (uint rid = 1; rid <= nMethods; rid++)
                    {
                        uint offset = 0;
                        if (methodEntryPoints.TryGetAt(_image, rid - 1, ref offset))
                        {
                            MethodDefinitionHandle methodDefHandle = MetadataTokens.MethodDefinitionHandle((int)rid);
                            R2RMethods[i++] = new R2RMethod(_image, ref mdReader, methodDefHandle, methodEntryPoints, offset, rid);
                        }
                    }

                    // get the RVAs of the runtime functions for each method
                    int runtimeFunctionSize = 2;
                    if (Machine == Machine.Amd64)
                    {
                        runtimeFunctionSize = 3;
                    }
                    R2RSection runtimeFunctionSection = R2RHeader.Sections[R2RSection.SectionType.READYTORUN_SECTION_RUNTIME_FUNCTIONS];
                    int curOffset = GetOffset(runtimeFunctionSection.RelativeVirtualAddress);
                    for (i = 0; i < nMethods; i++)
                    {
                        uint nRuntimeFunctions; // the number of runtime functions in this method
                        if (i < nMethods - 1) {
                            nRuntimeFunctions = R2RMethods[i + 1].EntryPointRuntimeFunctionId - R2RMethods[i].EntryPointRuntimeFunctionId;
                        }
                        else
                        {
                            uint totalRuntimeFunctions = (uint)(runtimeFunctionSection.Size / (runtimeFunctionSize * sizeof(int)));
                            nRuntimeFunctions = totalRuntimeFunctions - R2RMethods[i].EntryPointRuntimeFunctionId;
                        }

                        R2RMethods[i].NativeCode = new RuntimeFunction[nRuntimeFunctions];
                        for (int j = 0; j < nRuntimeFunctions; j++)
                        {
                            int nativeCodeStartRva = ReadInt32(_image, ref curOffset);
                            int nativeCodeEndRva = -1;
                            if (Machine == Machine.Amd64)
                            {
                                nativeCodeEndRva = ReadInt32(_image, ref curOffset);
                            }
                            int nativeCodeUnwindRva = ReadInt32(_image, ref curOffset);

                            R2RMethods[i].NativeCode[j] = new RuntimeFunction(nativeCodeStartRva, nativeCodeEndRva, nativeCodeUnwindRva);
                        }
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
            return image[start];
        }
    }
}
