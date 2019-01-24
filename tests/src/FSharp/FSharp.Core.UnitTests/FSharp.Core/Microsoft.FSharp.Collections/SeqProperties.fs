// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.
[<NUnit.Framework.Category "Collections.Seq">][<NUnit.Framework.Category "FSharp.Core.Collections">]
module FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Collections.SeqProperties

open System
open System.Collections.Generic
open NUnit.Framework
open FsCheck
open Utils

let sortByStable<'a when 'a : comparison> (xs : 'a []) =
    let indexed = xs |> Seq.indexed
    let sorted = indexed |> Seq.sortBy snd
    isStable sorted

[<Test>]
let ``Seq.sortBy is stable`` () =
    Check.QuickThrowOnFailure sortByStable<int>
    Check.QuickThrowOnFailure sortByStable<string>

let sortWithStable<'a when 'a : comparison> (xs : 'a []) =
    let indexed = xs |> Seq.indexed |> Seq.toList
    let sorted = indexed |> Seq.sortWith (fun x y -> compare (snd x) (snd y))
    isStable sorted
    
[<Test>]
let ``Seq.sortWithStable is stable`` () =
    Check.QuickThrowOnFailure sortWithStable<int>
    Check.QuickThrowOnFailure sortWithStable<string>
    
let distinctByStable<'a when 'a : comparison> (xs : 'a []) =
    let indexed = xs |> Seq.indexed
    let sorted = indexed |> Seq.distinctBy snd
    isStable sorted
    
[<Test>]
let ``Seq.distinctBy is stable`` () =
    Check.QuickThrowOnFailure distinctByStable<int>
    Check.QuickThrowOnFailure distinctByStable<string>
