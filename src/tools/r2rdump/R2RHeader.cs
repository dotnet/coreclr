// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;

namespace R2RDump
{
    class R2RHeader
    {
        public const uint READYTORUN_SIGNATURE = 0x00525452; // 'RTR'

        uint RelativeVirtualAddress;
        int Offset;
        int Size;

        string SignatureString;
        uint Signature;
        ushort MajorVersion;
        ushort MinorVersion;
        uint Flags;
        public uint NumberOfSections { get; }
        public R2RSection[] Sections { get; }

        public R2RHeader(byte[] pe, uint rva, int curOffset)
        {
            RelativeVirtualAddress = rva;
            Offset = curOffset;

            byte[] signature = new byte[sizeof(uint)];
            GetField(pe, signature, curOffset, sizeof(uint));
            SignatureString = System.Text.Encoding.Default.GetString(signature);
            Signature = (uint)GetField(pe, ref curOffset, sizeof(uint));
            if (Signature != READYTORUN_SIGNATURE)
            {
                throw new System.BadImageFormatException("Incorrect R2R header signature");
            }

            MajorVersion = (ushort)GetField(pe, ref curOffset, sizeof(ushort));
            MinorVersion = (ushort)GetField(pe, ref curOffset, sizeof(ushort));
            Flags = (uint)GetField(pe, ref curOffset, sizeof(uint));
            NumberOfSections = (uint)GetField(pe, ref curOffset, sizeof(uint));

            Sections = new R2RSection[NumberOfSections];
            for (int i = 0; i < NumberOfSections; i++)
            {
                Sections[i] = new R2RSection((uint)GetField(pe, ref curOffset, sizeof(uint)),
                    (uint)GetField(pe, ref curOffset, sizeof(uint)),
                    (uint)GetField(pe, ref curOffset, sizeof(uint)));
            }

            Size = curOffset - Offset;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat($"Signature: 0x{Signature:X8} ({SignatureString})\n");
            sb.AppendFormat($"RelativeVirtualAddress: 0x{RelativeVirtualAddress:X8}\n");
            if (Signature == READYTORUN_SIGNATURE)
            {
                sb.AppendFormat($"Size: {Size} bytes\n");
                sb.AppendFormat($"MajorVersion: 0x{MajorVersion:X4}\n");
                sb.AppendFormat($"MinorVersion: 0x{MinorVersion:X4}\n");
                sb.AppendFormat($"Flags: 0x{Flags:X8}\n");
                sb.AppendFormat($"NumberOfSections: {NumberOfSections}\n");
            }
            return sb.ToString();
        }

        public long GetField(byte[] pe, int start, int size)
        {
            return GetField(pe, ref start, size);
        }

        public long GetField(byte[] pe, ref int start, int size)
        {
            byte[] bytes = new byte[size];
            Array.Copy(pe, start, bytes, 0, size);
            start += size;

            if (size == 8)
            {
                return BitConverter.ToInt64(bytes, 0);
            }
            else if (size == 4)
            {
                return BitConverter.ToInt32(bytes, 0);
            }
            else if (size == 2)
            {
                return BitConverter.ToInt16(bytes, 0);
            }
            throw new System.ArgumentException("Invalid field size");
        }

        public void GetField(byte[] pe, byte[] bytes, int start, int size)
        {
            Array.Copy(pe, start, bytes, 0, size);
        }
    }
}
