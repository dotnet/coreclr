// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Reflection.PortableExecutable;

namespace R2RDump
{
    class R2RReader
    {
        byte[] pe;

        public string Filename { get; }
        public bool IsR2R { get; }
        public Machine Machine { get; }
        public ulong ImageBase { get; }
        public R2RHeader R2RHeader { get; }

        public unsafe R2RReader(string filename)
        {
            Filename = filename;
            pe = File.ReadAllBytes(filename);

            fixed (byte* p = pe)
            {
                IntPtr ptr = (IntPtr)p;
                PEReader peReader = new PEReader(p, pe.Length);

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

                DirectoryEntry corHeader = peReader.PEHeaders.PEHeader.CorHeaderTableDirectory;
                int R2RHeaderRVA = corHeader.RelativeVirtualAddress + corHeader.Size;
                int R2RHeaderOffset = R2RHeaderRVA - textSection.VirtualAddress + textSection.PointerToRawData;
                R2RHeader = new R2RHeader(pe, (uint)R2RHeaderRVA, R2RHeaderOffset);

            }
        }
    }
}
