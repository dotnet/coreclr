// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;

namespace R2RDump
{
    struct UnwindCode
    {
        public byte CodeOffset { get; }
        public byte UnwindOp { get; } //4 bits
        public byte OpInfo { get; } //4 bits

        public byte OffsetLow { get; }
        public byte OffsetHigh { get; } //4 bits

        public ushort FrameOffset { get; }

        public UnwindCode(byte[] image, ref int offset)
        {
            int off = offset;
            CodeOffset = NativeReader.ReadByte(image, ref off);
            byte op = NativeReader.ReadByte(image, ref off);
            UnwindOp = (byte)(op & 15);
            OpInfo = (byte)(op >> 4);

            OffsetLow = CodeOffset;
            OffsetHigh = OpInfo;

            FrameOffset = NativeReader.ReadUInt16(image, ref offset);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"    {{");
            sb.AppendLine($"        CodeOffset: {CodeOffset}");
            sb.AppendLine($"        UnwindOp: {UnwindOp}");
            sb.AppendLine($"        OpInfo: {OpInfo}");
            sb.AppendLine($"    }}");
            sb.AppendLine($"    {{");
            sb.AppendLine($"        OffsetLow: {OffsetLow}");
            sb.AppendLine($"        UnwindOp: {UnwindOp}");
            sb.AppendLine($"        OffsetHigh: {OffsetHigh}");
            sb.AppendLine($"    }}");
            sb.AppendLine($"    FrameOffset: {FrameOffset}");

            return sb.ToString();
        }
    }

    struct UnwindInfo
    {
        private const int _sizeofUnwindCode = 2;
        private const int _offsetofUnwindCode = 4;

        public byte Version { get; } //3 bits
        public byte Flags { get; } //5 bits
        public byte SizeOfProlog { get; }
        public byte CountOfUnwindCodes { get; }
        public byte FrameRegister { get; } //4 bits
        public byte FrameOffset { get; } //4 bits
        public UnwindCode[] UnwindCode { get; }
        public uint PersonalityRoutineRVA { get; }
        public int Size { get; }

        public UnwindInfo(byte[] image, int offset)
        {
            byte versionAndFlags = NativeReader.ReadByte(image, ref offset);
            Version = (byte)(versionAndFlags & 7);
            Flags = (byte)(versionAndFlags >> 3);
            SizeOfProlog = NativeReader.ReadByte(image, ref offset);
            CountOfUnwindCodes = NativeReader.ReadByte(image, ref offset);
            byte frameRegisterAndOffset = NativeReader.ReadByte(image, ref offset);
            FrameRegister = (byte)(frameRegisterAndOffset & 15);
            FrameOffset = (byte)(frameRegisterAndOffset >> 4);

            UnwindCode = new UnwindCode[CountOfUnwindCodes];
            for (int i = 0; i < CountOfUnwindCodes; i++)
            {
                UnwindCode[i] = new UnwindCode(image, ref offset);
            }

            PersonalityRoutineRVA = NativeReader.ReadUInt32(image, ref offset);

            Size = _offsetofUnwindCode + CountOfUnwindCodes * _sizeofUnwindCode + sizeof(uint);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"    Version: {Version}");
            sb.AppendLine($"    Flags: 0x{Flags:X8}");
            sb.AppendLine($"    SizeOfProlog: {SizeOfProlog}");
            sb.AppendLine($"    CountOfUnwindCodes: {CountOfUnwindCodes}");
            sb.AppendLine($"    FrameRegister: {FrameRegister}");
            sb.AppendLine($"    FrameOffset: {FrameOffset}");
            sb.AppendLine("    Unwind Codes");
            for (int i = 0; i < CountOfUnwindCodes; i++)
            {
                sb.AppendLine(UnwindCode[i].ToString());
            }
            sb.AppendLine($"    PersonalityRoutineRVA: 0x{PersonalityRoutineRVA:X8}");
            sb.AppendLine($"    Size: {Size}");

            return sb.ToString();
        }
    }
}
