// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

module FSharp.Core.UnitTests.Tests

open System
open NUnit.Framework

[<TestFixture>]
type TailCallRepros() =

    [<Test>]
    member this.``Tail recursive looping`` () =
        // If .tail doesn't happen will overflow and crash the process
        let mutable str = ""
        let rec addValueToString (y:int) =
            if y > 0 then
                str <- str + (sprintf "%d" y)
                addValueToString (y - 1)
        let _ = addValueToString 65000

        Assert.IsTrue((str.Length = 313894), (sprintf "Length of str should be: %d is actually: %d" 313894 (str.Length)))


    [<Test>]
    member this.``Repro jit .tail bug`` () =
        // Single Case
        let (|ToString|) (x : obj) = x.ToString()

        let sc = (|ToString|)
        let sc42 = sc 42
        Assert.IsTrue((sc42 = "42"), (sprintf "sc should be the string 42 it is actually: %A" sc))
