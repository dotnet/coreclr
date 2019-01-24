// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

#nowarn "44" // This construct is deprecated. please use Seq.item
namespace FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Collections

open System
open NUnit.Framework

open FSharp.Core.UnitTests.LibraryTestFx

[<TestFixture>][<Category "Collections.Seq">][<Category "FSharp.Core.Collections">]
type ObsoleteSeqFunctions() =

    [<Test>]
    member this.Nth() =
         
        // Negative index
        for i = -1 downto -10 do
           CheckThrowsArgumentException (fun () -> Seq.nth i { 10 .. 20 } |> ignore)
            
        // Out of range
        for i = 11 to 20 do
           CheckThrowsArgumentException (fun () -> Seq.nth i { 10 .. 20 } |> ignore)
         
         // integer Seq
        let resultInt = Seq.nth 3 { 10..20 } 
        Assert.AreEqual(13, resultInt)
        
        // string Seq
        let resultStr = Seq.nth 3 (seq ["Lists"; "Are";  "nthString" ; "List" ])
        Assert.AreEqual("List",resultStr)
          
        // empty Seq
        CheckThrowsArgumentException(fun () -> Seq.nth 0 (Seq.empty : seq<decimal>) |> ignore)
       
        // null Seq
        let nullSeq:seq<'a> = null 
        CheckThrowsArgumentNullException (fun () ->Seq.nth 3 nullSeq |> ignore)
        
        ()