// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// <Area> Nullable - Box-Unbox </Area>
// <Title> Nullable type with unbox box expr  </Title>
// <Description>  
// checking type of NotEmptyStruct using is operator
// </Description> 
// <RelatedBugs> </RelatedBugs>  
//<Expects Status=success></Expects>
// <Code> 


using System.Runtime.InteropServices;
using System;

internal class NullableTest
{
    private static bool BoxUnboxToNQ(object o)
    {
        return Helper.Compare((NotEmptyStruct)o, Helper.Create(default(NotEmptyStruct)));
    }

    private static bool BoxUnboxToQ(object o)
    {
        return Helper.Compare((NotEmptyStruct?)o, Helper.Create(default(NotEmptyStruct)));
    }

    private static int Main()
    {
        NotEmptyStruct? s = Helper.Create(default(NotEmptyStruct));

        if (BoxUnboxToNQ(s) && BoxUnboxToQ(s))
            return ExitCode.Passed;
        else
            return ExitCode.Failed;
    }
}


