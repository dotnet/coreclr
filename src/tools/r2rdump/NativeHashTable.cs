// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection.Metadata;

namespace R2RDump
{
    class NativeHashTable
    {
        MetadataReader _mdReader;
        uint _baseOffset;
        uint _bucketMask;
        byte _entryIndexSize;
        public uint Start { get; }
        public uint End { get; }

        public NativeHashTable(MetadataReader mdReader, byte[] image, NativeParser parser)
        {
            _mdReader = mdReader;

            uint header = parser.GetUInt8();
            _baseOffset = parser._offset;

            int numberOfBucketsShift = (int)(header >> 2);
            _bucketMask = (uint)((1 << numberOfBucketsShift) - 1);

            byte entryIndexSize = (byte)(header & 3);
            _entryIndexSize = entryIndexSize;

            int off = (int)_baseOffset;
            if (_entryIndexSize == 0)
            {

                Start = R2RReader.ReadByte(image, ref off);
                End = R2RReader.ReadByte(image, ref off);
            }
            else if (_entryIndexSize == 1)
            {
                Start = R2RReader.ReadUInt16(image, ref off);
                End = R2RReader.ReadUInt16(image, ref off);
            }
            else
            {
                Start = R2RReader.ReadUInt32(image, ref off);
                End = R2RReader.ReadUInt32(image, ref off);
            }
        }
    }
}

