// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

module FSharp.Core.UnitTests.LibraryTestFx

open System
open System.Collections.Generic

open NUnit.Framework

// Workaround for bug 3601, we are issuing an unnecessary warning
#nowarn "0004"

/// Check that the lambda throws an exception of the given type. Otherwise
/// calls Assert.Fail()
let CheckThrowsExn<'a when 'a :> exn> (f : unit -> unit) =
    try
        let _ = f ()
        sprintf "Expected %O exception, got no exception" typeof<'a> |> Assert.Fail 
    with
    | :? 'a -> ()
    | e -> sprintf "Expected %O exception, got: %O" typeof<'a> e |> Assert.Fail

let private CheckThrowsExn2<'a when 'a :> exn> s (f : unit -> unit) =
    let funcThrowsAsExpected =
        try
            let _ = f ()
            false // Did not throw!
        with
        | :? 'a
            -> true   // Thew null ref, OK
        | _ -> false  // Did now throw a null ref exception!
    if funcThrowsAsExpected
    then ()
    else Assert.Fail(s)

// Illegitimate exceptions. Once we've scrubbed the library, we should add an
// attribute to flag these exception's usage as a bug.
let CheckThrowsNullRefException      f = CheckThrowsExn<NullReferenceException>   f
let CheckThrowsIndexOutRangException f = CheckThrowsExn<IndexOutOfRangeException> f

// Legit exceptions
let CheckThrowsNotSupportedException f = CheckThrowsExn<NotSupportedException>    f
let CheckThrowsArgumentException     f = CheckThrowsExn<ArgumentException>        f
let CheckThrowsArgumentNullException f = CheckThrowsExn<ArgumentNullException>    f
let CheckThrowsArgumentNullException2 s f  = CheckThrowsExn2<ArgumentNullException>  s  f
let CheckThrowsArgumentOutOfRangeException f = CheckThrowsExn<ArgumentOutOfRangeException>    f
let CheckThrowsKeyNotFoundException  f = CheckThrowsExn<KeyNotFoundException>     f
let CheckThrowsDivideByZeroException f = CheckThrowsExn<DivideByZeroException>    f
let CheckThrowsOverflowException     f = CheckThrowsExn<OverflowException>        f
let CheckThrowsInvalidOperationExn   f = CheckThrowsExn<InvalidOperationException> f
let CheckThrowsFormatException       f = CheckThrowsExn<FormatException>           f

// Verifies two sequences are equal (same length, equiv elements)
let VerifySeqsEqual (seq1 : seq<'T>) (seq2 : seq<'T>) =
    CollectionAssert.AreEqual(seq1, seq2)

let sleep(n : int32) =        
    System.Threading.Thread.Sleep(n)
