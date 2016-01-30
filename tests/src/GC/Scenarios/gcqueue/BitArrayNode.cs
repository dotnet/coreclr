// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace DefaultNamespace
{
    using System;
    using System.Collections;

    internal class BitArrayNode
    {
        public BitArrayNode(int num)
        {
            int[] temp = new int[num];
            for (int i = 0; i < num; i++)
            {
                temp[i] = i;
            }
            BitArray L_Node = new BitArray(temp);
        }
    }
}
