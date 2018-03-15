// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace System.Text
{
    public sealed partial class StringBuilder : ISerializable
    {
        private void ShowChunks(int maxChunksToShow = 10)
        {
            Debug.WriteLine('|' + string.Join('|', ShowChunksInOrder(maxChunksToShow)) + '|');
        }

        private IEnumerable<string> ShowChunksInOrder(int maxChunksToShow)
        {
            int numChunksToShow = 0;
            StringBuilder lastChunk = null;
            // Gets numChunksToShow. If numChunksToShow is larger than maxChunksToShow, then returns last chunk
            GetNumChunksToShow(this, ref numChunksToShow, maxChunksToShow, ref lastChunk);
            Span<string> chunkChars = new string[numChunksToShow];
            var sb = lastChunk ?? this;

            while (sb != null && maxChunksToShow > 0)
            {
                chunkChars[numChunksToShow - 1] = string.Create(sb.m_ChunkChars.Length, sb.m_ChunkChars, (Span<char> chars, char[] curChunksChars) =>
                {
                    for (int i = 0; i < curChunksChars.Length; i++)
                    {
                        chars[i] = curChunksChars[i];
                    }
                }).Replace('\0', '.');
                sb = sb.m_ChunkPrevious;
                numChunksToShow--;
            }
            return chunkChars.ToArray();
        }
        
        private void GetNumChunksToShow(StringBuilder sb, ref int numToShow, int maxChunksToShow, ref StringBuilder lastChunk)
        {
            if (sb.m_ChunkPrevious != null)
                GetNumChunksToShow(sb.m_ChunkPrevious, ref numToShow, maxChunksToShow, ref lastChunk);

            if (numToShow < maxChunksToShow)
            {
                numToShow++;
                if (numToShow == maxChunksToShow)
                {
                    lastChunk = sb;
                }
            }
        }
    }
}
