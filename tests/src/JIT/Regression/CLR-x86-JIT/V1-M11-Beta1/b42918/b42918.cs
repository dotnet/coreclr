// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//

namespace Test
{
    using System;

    class AA
    {
        public static int Main()
        {
            try
            {
                try
                {
                    // blah blah blah ...
                }
                finally
                {
                    int[] an = new int[2];
                    an[-1] = 0;
                }
            }
            catch (Exception) { }
            return 100;
        }
    }
}
