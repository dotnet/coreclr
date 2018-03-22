// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace System.Text
{
    public sealed partial class StringBuilder
    {
        private void ShowChunks(int maxChunksToShow = 10)
        {
            Debug.WriteLine('|' + string.Join('|', ShowChunksInOrder(maxChunksToShow)) + '|');
        }

        private IEnumerable<string> ShowChunksInOrder(int maxChunksToShow)
        {
            (int count, StringBuilder head) chunksToShow = GetChunksToShow(maxChunksToShow);
            string[] chunks = new string[chunksToShow.count];
            StringBuilder current = chunksToShow.head;
            for (int i = chunksToShow.count; i > 0; i--)
            {
                chunks[i - 1] = new string(current.m_ChunkChars).Replace('\0', '.');
                current = current.m_ChunkPrevious;
            }
            return chunks;
        }

        private (int count, StringBuilder head) GetChunksToShow(int maxChunksToShow)
        {
            int numChunks = 0;
            StringBuilder current = this;
            while (current != null)
            {
                numChunks++;
                current = current.m_ChunkPrevious;
            }
            current = this;
            int numChunksToShow = numChunks;
            for (int skipCount = numChunks - maxChunksToShow; skipCount > 0; skipCount--)
            {
                current = current.m_ChunkPrevious;
                numChunksToShow--;
            }
            return (numChunksToShow, current);
        }
    }
}
