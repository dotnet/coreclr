// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.PortableExecutable;

namespace R2RDump
{
    class R2RReader
    {
        /// <summary>
        /// Byte array containing the ReadyToRun image
        /// </summary>
        private readonly byte[] _image;

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
        public NativeCode[] NativeCode { get; }

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
                PEReader peReader = new PEReader(p, _image.Length);

                IsR2R = (peReader.PEHeaders.CorHeader.Flags == CorFlags.ILLibrary);
                if (!IsR2R)
                {
                    throw new System.BadImageFormatException("The file is not a ReadyToRun image");
                }

                Machine = peReader.PEHeaders.CoffHeader.Machine;
                ImageBase = peReader.PEHeaders.PEHeader.ImageBase;

                SectionHeader textSection;
                int nSections = peReader.PEHeaders.CoffHeader.NumberOfSections;
                for (int i = 0; i < nSections; i++)
                {
                    SectionHeader section = peReader.PEHeaders.SectionHeaders[i];
                    if (section.Name.Equals(".text"))
                    {
                        textSection = section;
                    }
                }

                DirectoryEntry r2rHeaderDirectory = peReader.PEHeaders.CorHeader.ManagedNativeHeaderDirectory;
                int r2rHeaderOffset = GetOffset(r2rHeaderDirectory.RelativeVirtualAddress, textSection);
                R2RHeader = new R2RHeader(_image, r2rHeaderDirectory.RelativeVirtualAddress, r2rHeaderOffset);
                if (r2rHeaderDirectory.Size != R2RHeader.Size)
                {
                    throw new System.BadImageFormatException("The calculated size of the R2RHeader doesn't match the size saved in the ManagedNativeHeaderDirectory");
                }

                R2RSection runtimeFunctions = R2RHeader.Sections[R2RSection.SectionType.READYTORUN_SECTION_RUNTIME_FUNCTIONS];
                int curOffset = GetOffset((int)runtimeFunctions.RelativeVirtualAddress, textSection);
                int nNativeCode = runtimeFunctions.Size / (3*sizeof(int));
                NativeCode = new NativeCode[nNativeCode];
                for (int i=0; i<nNativeCode; i++)
                {
                    int nativeCodeStartRva = GetInt32(_image, ref curOffset);
                    int nativeCodeEndRva = GetInt32(_image, ref curOffset);
                    int nativeCodeUnwindRva = GetInt32(_image, ref curOffset);
                    NativeCode[i] = new NativeCode(_image, nativeCodeStartRva, nativeCodeEndRva, nativeCodeUnwindRva);
                }
            }
        }

        public int GetOffset(int rva, SectionHeader textSection)
        {
            return rva - textSection.VirtualAddress + textSection.PointerToRawData;
        }

        /// <summary>
        /// Extracts a value from the image byte array
        /// </summary>
        /// <param name="image">PE image</param>
        /// <param name="start">Starting index of the value</param>
        /// <remarks>
        /// The <paramref name="start"/> gets incremented to the end of the value
        /// </remarks>
        public static long GetInt64(byte[] image, ref int start)
        {
            int size = sizeof(long);
            byte[] bytes = new byte[size];
            Array.Copy(image, start, bytes, 0, size);
            start += size;
            return BitConverter.ToInt64(bytes, 0);
        }

        public static int GetInt32(byte[] image, ref int start)
        {
            int size = sizeof(int);
            byte[] bytes = new byte[size];
            Array.Copy(image, start, bytes, 0, size);
            start += size;
            return BitConverter.ToInt32(bytes, 0);
        }

        public static short GetInt16(byte[] image, ref int start)
        {
            int size = sizeof(short);
            byte[] bytes = new byte[size];
            Array.Copy(image, start, bytes, 0, size);
            start += size;
            return BitConverter.ToInt16(bytes, 0);
        }
    }
}
