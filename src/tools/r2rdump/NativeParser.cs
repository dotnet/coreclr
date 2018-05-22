// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace R2RDump
{
    class NativeParser
    {
        public uint _offset { get; set; }

        byte[] _image;

        public NativeParser(byte[] image, uint offset)
        {
            _image = image;
            _offset = offset;
        }

        public byte GetUInt8()
        {
            int off = (int)_offset;
            byte val = R2RReader.ReadByte(_image, ref off);
            _offset += 1;
            return val;
        }
    }
}
