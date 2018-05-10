using System;
using System.Collections.Generic;
using System.Text;

namespace R2RDump
{
    struct R2RSection
    {
        uint Type;
        uint RelativeVirtualAddress;
        uint Size;

        public R2RSection(uint type, uint rva, uint size)
        {
            Type = type;
            RelativeVirtualAddress = rva;
            Size = size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat($"Type:  0x{Type:X8} ({Type:D})\n");
            sb.AppendFormat($"RelativeVirtualAddress: 0x{RelativeVirtualAddress:X8}\n");
            sb.AppendFormat($"Size: {Size} bytes\n");
            return sb.ToString();
        }
    }
}
