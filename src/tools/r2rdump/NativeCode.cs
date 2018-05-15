// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;

namespace R2RDump
{
    class NativeCode
    {
        /// <summary>
        /// The relative virtual address to the start of the code block
        /// </summary>
        public int StartRVA { get; }

        /// <summary>
        /// The relative virtual address to the end of the code block
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// The relative virtual address to the unwind info
        /// </summary>
        public int UnwindRVA { get; }

        public NativeCode(byte[] image, int startRva, int endRva, int unwindRva)
        {
            StartRVA = startRva;
            Size = endRva - StartRVA;
            UnwindRVA = unwindRva;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat($"StartRVA: 0x{StartRVA:X8}\n");
            sb.AppendFormat($"Size: {Size} bytes\n");
            return sb.ToString();
        }
    }
}
