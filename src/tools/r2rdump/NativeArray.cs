﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace R2RDump
{
    class NativeArray
    {
        private const int _blockSize = 16;
        private uint _baseOffset;
        private uint _nElements;
        private byte _entryIndexSize;

        public NativeArray(byte[] image, uint offset)
        {
            uint val = 0;
            _baseOffset = NativeReader.DecodeUnsigned(image, offset, ref val);
            _nElements = (val >> 2);
            _entryIndexSize = (byte)(val & 3);
        }

        public uint GetCount()
        {
            return _nElements;
        }

        public bool TryGetAt(byte[] image, uint index, ref int pOffset)
        {
            if (index >= _nElements)
                return false;

            uint offset = 0;
            if (_entryIndexSize == 0)
            {
                int i = (int)(_baseOffset + (index / _blockSize));
                offset = NativeReader.ReadByte(image, ref i);
            }
            else if (_entryIndexSize == 1)
            {
                int i = (int)(_baseOffset + 2 * (index / _blockSize));
                offset = NativeReader.ReadUInt16(image, ref i);
            }
            else
            {
                int i = (int)(_baseOffset + 4 * (index / _blockSize));
                offset = NativeReader.ReadUInt32(image, ref i);
            }
            offset += _baseOffset;

            for (uint bit = _blockSize >> 1; bit > 0; bit >>= 1)
            {
                uint val = 0;
                uint offset2 = NativeReader.DecodeUnsigned(image, offset, ref val);
                if ((index & bit) != 0)
                {
                    if ((val & 2) != 0)
                    {
                        offset = offset + (val >> 2);
                        continue;
                    }
                }
                else
                {
                    if ((val & 1) != 0)
                    {
                        offset = offset2;
                        continue;
                    }
                }

                // Not found
                if ((val & 3) == 0)
                {
                    // Matching special leaf node?
                    if ((val >> 2) == (index & (_blockSize - 1)))
                    {
                        offset = offset2;
                        break;
                    }
                }
                return false;
            }
            pOffset = (int)offset;
            return true;
        }
    }
}
