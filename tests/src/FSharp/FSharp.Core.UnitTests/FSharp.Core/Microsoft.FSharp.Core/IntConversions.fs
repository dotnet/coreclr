// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

namespace FSharp.Core.UnitTests.FSharp_Core.FSharp.Core
open System
open NUnit.Framework
open FSharp.Core.UnitTests.LibraryTestFx

[<TestFixture>]
type IntConversions() =

    [<Test>]
    member this.``Unchecked.SignedToUInt64`` () =
        let d = System.Int32.MinValue
        let e = uint64 d
        let f = uint64 (uint32 d)
        Assert.IsTrue (e <> f)                 
        ()
        
    [<Test>]
    member this.``Unchecked.SignedToUInt32`` () =
        let d = System.Int16.MinValue
        let e = uint32 d
        let f = uint32 (uint16 d)
        Assert.IsTrue (e <> f)                 
        ()
    
    [<Test>]
    member this.``Checked.UnsignedToSignedInt32``() =
        let d = System.UInt16.MaxValue
        CheckThrowsExn<OverflowException>(fun() -> Checked.int16 d |> ignore)
