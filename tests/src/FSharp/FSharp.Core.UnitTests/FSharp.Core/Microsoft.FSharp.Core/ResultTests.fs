// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// Various tests for:
// Microsoft.FSharp.Core.Result

namespace FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Core

open System
open FSharp.Core.UnitTests.LibraryTestFx
open NUnit.Framework

type EmailValidation=
    | Empty
    | NoAt

open Result

[<TestFixture>]
type ResultTests() =

    let fail_if_empty email=
        if String.IsNullOrEmpty(email) then Error Empty else Ok email

    let fail_if_not_at (email:string)=
        if (email.Contains("@")) then Ok email else Error NoAt

    let validate_email =
        fail_if_empty
        >> bind fail_if_not_at

    let test_validate_email email (expected:Result<string,EmailValidation>) =
        let actual = validate_email email
        Assert.AreEqual(expected, actual)

    let toUpper (v:string) = v.ToUpper()

    let shouldBeOkWithValue expected maybeOk = match maybeOk with | Error e-> failwith "Expected Ok, got Error!" | Ok v->Assert.AreEqual(expected, v)

    let shouldBeErrorWithValue expected maybeError = match maybeError with | Error e-> Assert.AreEqual(expected, e) | Ok v-> failwith "Expected Error, got Ok!"

    let addOneOk (v:int) = Ok (v+1)

    [<Test>]
    member this.CanChainTogetherSuccessiveValidations() =
        test_validate_email "" (Error Empty)
        test_validate_email "something_else" (Error NoAt)
        test_validate_email "some@email.com" (Ok "some@email.com")

    [<Test>]
    member this.MapWillTransformOkValues() =
        Ok "some@email.com" 
        |> map toUpper
        |> shouldBeOkWithValue "SOME@EMAIL.COM"

    [<Test>]
    member this.MapWillNotTransformErrorValues() =
        Error "my error" 
        |> map toUpper
        |> shouldBeErrorWithValue "my error"

    [<Test>]
    member this.MapErrorWillTransformErrorValues() =
        Error "my error" 
        |> mapError toUpper
        |> shouldBeErrorWithValue "MY ERROR"

    [<Test>]
    member this.MapErrorWillNotTransformOkValues() =
        Ok "some@email.com" 
        |> mapError toUpper
        |> shouldBeOkWithValue "some@email.com"

    [<Test>]
    member this.BindShouldModifyOkValue() =
        Ok 42
        |> bind addOneOk
        |> shouldBeOkWithValue 43

    [<Test>]
    member this.BindErrorShouldNotModifyError() =
        Error "Error"
        |> bind addOneOk
        |> shouldBeErrorWithValue "Error"


