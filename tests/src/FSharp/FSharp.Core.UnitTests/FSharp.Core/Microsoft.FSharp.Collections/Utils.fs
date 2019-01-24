// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

module FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Collections.Utils

type Result<'a> = 
| Success of 'a
| Error of string

let run f = 
    try
        Success(f())
    with
    | exn -> Error(exn.Message)

let runAndCheckErrorType f = 
    try
        Success(f())
    with
    | exn -> Error(exn.GetType().ToString())

let runAndCheckIfAnyError f = 
    try
        Success(f())
    with
    | exn -> Error("")


let isStable sorted = sorted |> Seq.pairwise |> Seq.forall (fun ((ia, a),(ib, b)) -> if a = b then ia < ib else true)

let isSorted sorted = sorted |> Seq.pairwise |> Seq.forall (fun (a,b) -> a <= b)

let haveSameElements (xs:seq<_>) (ys:seq<_>) =
    let xsHashSet = new System.Collections.Generic.HashSet<_>(xs)
    let ysHashSet = new System.Collections.Generic.HashSet<_>(ys)
    xsHashSet.SetEquals(ysHashSet)