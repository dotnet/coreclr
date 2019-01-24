// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// Various tests for the:
// Microsoft.FSharp.Collections.ComparisonIdentity module

namespace FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Collections

open System
open FSharp.Core.UnitTests.LibraryTestFx
open NUnit.Framework

(*
[Test Strategy]
Make sure each method works on:
*  (value type)
*  (reference type)
*  (0 elements)
*  (2 - 7 elements)
*)

[<TestFixture>]
type ComparisonIdentityModule() =
    [<Test>]
    member this.FromFunction() =
        // integer array  
        let intArr = [|1;5;8;2;6;3;7;4|]
        System.Array.Sort(intArr, ComparisonIdentity.FromFunction compare)
        Assert.AreEqual([|1;2;3;4;5;6;7;8|],intArr)

        // string array     
        let strArr = [|"A";"C";"B"|]
        System.Array.Sort(strArr, ComparisonIdentity.FromFunction (compare))
        Assert.AreEqual([|"A";"B";"C"|],strArr)

        // empty array     
        let eptArr = [||]
        System.Array.Sort(eptArr, ComparisonIdentity.FromFunction (compare))
        Assert.AreEqual([||], eptArr)       
        
        ()
        
    [<Test>]
    member this.Structural() =
        // integer array  
        let intArr = [|1;5;8;2;6;3;7;4|]
        System.Array.Sort(intArr, ComparisonIdentity.Structural )
        Assert.AreEqual([|1;2;3;4;5;6;7;8|],intArr)

        // string array     
        let strArr = [|"A";"C";"B"|]
        System.Array.Sort(strArr, ComparisonIdentity.Structural )
        Assert.AreEqual([|"A";"B";"C"|],strArr)

        // empty array     
        let eptArr = [||]
        System.Array.Sort(eptArr, ComparisonIdentity.Structural )
        Assert.AreEqual([||],eptArr)    
        
        ()
        
