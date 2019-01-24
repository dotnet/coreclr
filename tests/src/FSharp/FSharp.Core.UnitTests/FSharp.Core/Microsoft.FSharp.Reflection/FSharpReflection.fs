// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// Various tests for Microsoft.FSharp.Reflection

namespace FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Reflection

open System
open System.Reflection

open FSharp.Core.UnitTests.LibraryTestFx
open NUnit.Framework

open Microsoft.FSharp.Reflection

(*
[Test Strategy]
Make sure each method works on:
* Generic types
* Null    
* DiscriminatedUnion  
* Delegate    
* Record
* Exception
* Tuple
* Fuction
* Struct versions of the above
*)

#if FX_RESHAPED_REFLECTION
module PrimReflectionAdapters =
    open System.Linq
    
    type System.Type with
        member this.Assembly = this.GetTypeInfo().Assembly
        member this.IsGenericType = this.GetTypeInfo().IsGenericType
        member this.IsValueType = this.GetTypeInfo().IsValueType
        member this.IsAssignableFrom(otherTy : Type) = this.GetTypeInfo().IsAssignableFrom(otherTy.GetTypeInfo())
        member this.GetProperty(name) = this.GetRuntimeProperty(name)
        member this.GetProperties() = this.GetRuntimeProperties() |> Array.ofSeq
        member this.GetMethod(name, parameterTypes) = this.GetRuntimeMethod(name, parameterTypes)
        member this.GetCustomAttributes(attrTy : Type, inherits : bool) : obj[] = 
            unbox (box (CustomAttributeExtensions.GetCustomAttributes(this.GetTypeInfo(), attrTy, inherits).ToArray()))
            
    type System.Reflection.MemberInfo with
        member this.ReflectedType = this.DeclaringType
        
    type System.Reflection.Assembly with
        member this.GetTypes() = this.DefinedTypes |> Seq.map (fun ti -> ti.AsType()) |> Array.ofSeq

open PrimReflectionAdapters
#endif

module IsModule = 
    type IsModuleType () = 
        member __.M = 1

type FSharpDelegate = delegate of int -> string

type RecordType = { field1 : string; field2 : RecordType option; field3 : (unit -> RecordType * string) }
type GenericRecordType<'T, 'U> = { field1 : 'T; field2 : 'U; field3 : (unit -> GenericRecordType<'T, 'U>) }

[<Struct>]
type StructRecordType = { field1 : string; field2 : StructRecordType option; field3 : (unit -> StructRecordType * string) }

type SingleNullaryCaseDiscUnion = SingleNullaryCaseTag
type SingleCaseDiscUnion = SingleCaseTag of float * float * float

[<Struct>]
type SingleNullaryCaseDiscStructUnion = SingleNullaryCaseTagStruct
[<Struct>]
type SingleCaseDiscStructUnion = SingleCaseTagStruct of float * float * float

type DiscUnionType<'T> =
        | A // No data associated with tag
        | B of 'T * DiscUnionType<'T> option
        | C of float * string

type DiscStructUnionType<'T> =
        | A // No data associated with tag
        | B of 'T 
        | C of float * string

exception ExceptionInt of int

exception DatalessException

module FSharpModule = 
    type ModuleType() =
        class
        end

[<TestFixture>]
type FSharpValueTests() =
    
    // global variables
    let rec record1 : RecordType = { field1 = "field1"; field2 = Some(record1); field3 = ( fun () -> (record1, "")  )}
    let record2 : RecordType = { field1 = "field2"; field2 = Some(record1); field3 = ( fun () -> (record1, "")  )}
    
    // global variables
    let rec structRecord1 : StructRecordType = { field1 = "field1"; field2 = None; field3 = ( fun () -> (structRecord1, "")  )}
    let structRecord2 : StructRecordType = { field1 = "field2"; field2 = Some(structRecord1); field3 = ( fun () -> (structRecord1, "")  )}

    let rec genericRecordType1 : GenericRecordType<string, int> = 
        { 
            field1 = "field1"
            field2 = 1
            field3 = (fun () -> genericRecordType1)
        }
    
    let genericRecordType2 : GenericRecordType<string, int> = { field1 = "field1"; field2 = 1; field3 = ( fun () -> genericRecordType1 )}
    
    let nullValue = null
    
    let singleCaseUnion1 = SingleCaseDiscUnion.SingleCaseTag(1.0, 2.0, 3.0)
    let singleCaseUnion2 = SingleCaseDiscUnion.SingleCaseTag(4.0, 5.0, 6.0)
    
    let singleCaseStructUnion1 = SingleCaseDiscStructUnion.SingleCaseTagStruct(1.0, 2.0, 3.0)
    let singleCaseStructUnion2 = SingleCaseDiscStructUnion.SingleCaseTagStruct(4.0, 5.0, 6.0)
    
    let discUnionCaseA = DiscUnionType.A
    let discUnionCaseB = DiscUnionType.B(1, Some(discUnionCaseA))
    let discUnionCaseC = DiscUnionType.C(1.0, "stringparam")
    
    let discUnionRecCaseB = DiscUnionType.B(1, Some(discUnionCaseB))
    
    let discStructUnionCaseA = DiscStructUnionType.A
    let discStructUnionCaseB = DiscStructUnionType.B(1)
    let discStructUnionCaseC = DiscStructUnionType.C(1.0, "stringparam")
    
    
    let fsharpDelegate1 = new FSharpDelegate(fun (x:int) -> "delegate1")
    let fsharpDelegate2 = new FSharpDelegate(fun (x:int) -> "delegate2")
   
    let tuple1 = ( 1, "tuple1")
    let tuple2 = ( 2, "tuple2", (fun x -> x + 1))
    let tuple3 = ( 1, ( 2, "tuple"))
    
    let structTuple1 = struct ( 1, "tuple1")
    let structTuple2 = struct ( 2, "tuple2", (fun x -> x + 1))
    let structTuple3 = struct ( 1, struct ( 2, "tuple"))
    
    let func1  param  = param + 1
    let func2  param  = param + ""
    
    let exInt = ExceptionInt(1)
    let exDataless = DatalessException
 
    [<Test>]
    member __.Equals1() =
        // Record value                
        Assert.IsTrue(FSharpValue.Equals(record1, record1))
        Assert.IsFalse(FSharpValue.Equals(record1, record2))

    [<Test>]
    member __.Equals2() =
        Assert.IsTrue(FSharpValue.Equals(structRecord1, structRecord1))
        Assert.IsFalse(FSharpValue.Equals(structRecord1, structRecord2))
        Assert.IsFalse(FSharpValue.Equals(structRecord1, record2))
        Assert.IsFalse(FSharpValue.Equals(record1, structRecord1))
        Assert.IsFalse(FSharpValue.Equals(record2, structRecord2))

    [<Test>]
    member __.Equals3() =
        // Generic Record value
        Assert.IsTrue(FSharpValue.Equals(genericRecordType1, genericRecordType1))
        Assert.IsFalse(FSharpValue.Equals(genericRecordType1, genericRecordType2))
        
    [<Test>]
    member __.Equals4() =
        // null value
        Assert.IsTrue(FSharpValue.Equals(nullValue, nullValue))
        Assert.IsFalse(FSharpValue.Equals(nullValue, 1))
        
    [<Test>]
    member __.Equals5() =
        // Single Case Union
        Assert.IsTrue(FSharpValue.Equals(singleCaseUnion1, singleCaseUnion1))
        Assert.IsFalse(FSharpValue.Equals(singleCaseUnion1, singleCaseUnion2))
        
    [<Test>]
    member __.Equals6() =
        // Single Case Union
        Assert.IsTrue(FSharpValue.Equals(singleCaseStructUnion1, singleCaseStructUnion1))
        Assert.IsFalse(FSharpValue.Equals(singleCaseStructUnion1, singleCaseStructUnion2))
        
    [<Test>]
    member __.Equals7() =
        // Discriminated Union
        Assert.IsTrue(FSharpValue.Equals(discUnionCaseA, discUnionCaseA))
        Assert.IsFalse(FSharpValue.Equals(discUnionCaseB, discUnionCaseC))
      
    [<Test>]
    member __.Equals8() =
        // Discriminated Union
        Assert.IsTrue(FSharpValue.Equals(discStructUnionCaseA, discStructUnionCaseA))
        Assert.IsFalse(FSharpValue.Equals(discStructUnionCaseB, discStructUnionCaseC))
      
    [<Test>]
    member __.Equals9() =
        // FSharpDelegate
        Assert.IsTrue(FSharpValue.Equals(fsharpDelegate1, fsharpDelegate1))
        Assert.IsFalse(FSharpValue.Equals(fsharpDelegate1, fsharpDelegate2))
        
    [<Test>]
    member __.Equals10() =
        // Tuple
        Assert.IsTrue(FSharpValue.Equals(tuple1, tuple1))
        Assert.IsFalse(FSharpValue.Equals( (1, 2, 3), (4, 5, 6) ))
        
    [<Test>]
    member __.Equals10b() =
        // Tuple
        Assert.IsTrue(FSharpValue.Equals(structTuple1, structTuple1))
        Assert.IsFalse(FSharpValue.Equals( struct (1, 2, 3), struct (4, 5, 6) ))

    [<Test>]
    member __.Equals11() =
        // Tuples of differing types
        Assert.IsFalse(FSharpValue.Equals(tuple1, tuple2))
     
    [<Test>]
    member __.Equals12() =
        // Exception
        Assert.IsTrue(FSharpValue.Equals(exInt, exInt))
        Assert.IsFalse(FSharpValue.Equals(exInt, exDataless))      

    [<Test>]
    member __.GetExceptionFields() =
        
        // int 
        Assert.AreEqual(FSharpValue.GetExceptionFields(exInt), ([|1|] : obj []))
        
        // dataless
        Assert.AreEqual(FSharpValue.GetExceptionFields(exDataless), [||])
        
        // invalid type
        CheckThrowsArgumentException(fun () -> FSharpValue.GetExceptionFields(1) |> ignore)
        CheckThrowsArgumentException(fun () -> FSharpValue.GetExceptionFields( () ) |> ignore)
        
        // System Exception
        CheckThrowsArgumentException(fun () -> FSharpValue.GetExceptionFields(new System.Exception("ex message")) |> ignore)
        
        // null
        CheckThrowsArgumentException(fun () -> FSharpValue.GetExceptionFields(null) |> ignore)
        
    [<Test>]
    member __.GetRecordField() =
         
        // Record
        let propertyinfo1 = (typeof<RecordType>).GetProperty("field1")
        Assert.AreEqual((FSharpValue.GetRecordField(record1, propertyinfo1)), "field1")
        
        // Generic Record value
        let propertyinfo2 = (typeof<GenericRecordType<string, int>>).GetProperty("field2")
        Assert.AreEqual((FSharpValue.GetRecordField(genericRecordType1, propertyinfo2)), 1)
        
        // null value
        CheckThrowsArgumentException(fun () ->FSharpValue.GetRecordField(null, propertyinfo1)|> ignore)
        CheckThrowsArgumentException(fun () ->FSharpValue.GetRecordField( () , propertyinfo1)|> ignore)
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.GetRecordField("invalid", propertyinfo1) |> ignore)
        
        // invalid property info
        let propertyinfoint = (typeof<RecordType>).GetProperty("fieldstring")
        CheckThrowsArgumentException(fun () -> FSharpValue.GetRecordField("invalid", propertyinfoint) |> ignore)
        
    [<Test>]
    member __.GetStructRecordField() =
         
        // Record
        let propertyinfo1 = (typeof<StructRecordType>).GetProperty("field1")
        Assert.AreEqual((FSharpValue.GetRecordField(structRecord1, propertyinfo1)), "field1")
        
        // null value
        CheckThrowsArgumentException(fun () ->FSharpValue.GetRecordField(null, propertyinfo1)|> ignore)
        CheckThrowsArgumentException(fun () ->FSharpValue.GetRecordField( () , propertyinfo1)|> ignore)
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.GetRecordField("invalid", propertyinfo1) |> ignore)
        
        // invalid property info
        let propertyinfoint = (typeof<StructRecordType>).GetProperty("fieldstring")
        CheckThrowsArgumentException(fun () -> FSharpValue.GetRecordField("invalid", propertyinfoint) |> ignore)
        
    [<Test>]
    member __.GetRecordFields() =
        // Record
        let propertyinfo1 = (typeof<RecordType>).GetProperty("field1")
        Assert.AreEqual((FSharpValue.GetRecordFields(record1)).[0], "field1")
        
        // Generic Record value
        let propertyinfo2 = (typeof<GenericRecordType<string, int>>).GetProperty("field1")
        Assert.AreEqual((FSharpValue.GetRecordFields(genericRecordType1)).[0], "field1")
        
        // null value
        CheckThrowsArgumentException(fun () -> FSharpValue.GetRecordFields(null)|> ignore)
        CheckThrowsArgumentException(fun () -> FSharpValue.GetRecordFields( () )|> ignore)
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.GetRecordFields("invalid") |> ignore)
    
    [<Test>]
    member __.GetStructRecordFields() =
        let propertyinfo1 = (typeof<StructRecordType>).GetProperty("field1")
        Assert.AreEqual((FSharpValue.GetRecordFields(structRecord1)).[0], "field1")
        
    [<Test>]
    member __.GetTupleField() =
        // Tuple
        Assert.AreEqual((FSharpValue.GetTupleField(tuple1, 0)), 1)
        
        // Tuple with function element
        Assert.AreEqual( FSharpValue.GetTupleField(tuple2, 1), "tuple2")
        
        // null value
        CheckThrowsArgumentException(fun () -> FSharpValue.GetTupleField(null, 3)|> ignore)
        CheckThrowsArgumentException(fun () -> FSharpValue.GetTupleField( () , 3)|> ignore)
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.GetTupleField("Invalid", 3)|> ignore)
        
        // index out of range
        CheckThrowsArgumentException(fun () -> FSharpValue.GetTupleField(tuple2, 8)|> ignore)
      
    [<Test>]
    member __.GetStructTupleField() =
        // Tuple
        Assert.AreEqual((FSharpValue.GetTupleField(structTuple1, 0)), 1)
        
        // Tuple with function element
        Assert.AreEqual( FSharpValue.GetTupleField(structTuple2, 1), "tuple2")
        
        // index out of range
        CheckThrowsArgumentException(fun () -> FSharpValue.GetTupleField(structTuple2, 8)|> ignore)
      
    [<Test>]
    member __.GetTupleFields() =
        // Tuple
        Assert.AreEqual(FSharpValue.GetTupleFields(tuple1).[0], 1)
        
        // Tuple with function element
        Assert.AreEqual( (FSharpValue.GetTupleFields(tuple2)).[1], "tuple2")
        
        // null value
        CheckThrowsArgumentException(fun () -> FSharpValue.GetTupleFields(null)|> ignore)
        CheckThrowsArgumentException(fun () -> FSharpValue.GetTupleFields( () )|> ignore)
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.GetTupleFields("Invalid")|> ignore)
      
    [<Test>]
    member __.GetStructTupleFields() =
        // Tuple
        Assert.AreEqual(FSharpValue.GetTupleFields(structTuple1).[0], 1)
        
        // Tuple with function element
        Assert.AreEqual( (FSharpValue.GetTupleFields(structTuple2)).[1], "tuple2")
        
    [<Test>]
    member __.GetUnionFields() =
        // single case union  
        let (singlecaseinfo, singlevaluearray) = FSharpValue.GetUnionFields(singleCaseUnion1, typeof<SingleCaseDiscUnion>)
        Assert.AreEqual(singlevaluearray, ([|1.0;2.0;3.0|] : obj []))
        
        // DiscUnionType
        let (duCaseinfo, duValueArray) = FSharpValue.GetUnionFields(discUnionCaseB, typeof<DiscUnionType<int>>)
        Assert.AreEqual(duValueArray.[0], 1)
                
        // null value
        CheckThrowsArgumentException(fun () ->  FSharpValue.GetUnionFields(null, null)|> ignore)
        CheckThrowsArgumentException(fun () ->  FSharpValue.GetUnionFields( () , null)|> ignore)
        
    [<Test>]
    member __.GetStructUnionFields() =
        // single case union  
        let (_singlecaseinfo, singlevaluearray) = FSharpValue.GetUnionFields(singleCaseStructUnion1, typeof<SingleCaseDiscStructUnion>)
        Assert.AreEqual(singlevaluearray, ([|1.0;2.0;3.0|] : obj []))
        
        // DiscUnionType
        let (_duCaseinfo, duValueArray) = FSharpValue.GetUnionFields(discStructUnionCaseB, typeof<DiscStructUnionType<int>>)
        Assert.AreEqual(duValueArray.[0], 1)
                
    [<Test>]
    member __.MakeFunction() =
    
        // Int function
        let implementationInt (x:obj) = box( unbox<int>(x) + 1)
        let resultFuncIntObj  = FSharpValue.MakeFunction(typeof<int -> int>, implementationInt )
        let resultFuncInt = resultFuncIntObj :?> (int -> int)
        Assert.AreEqual(resultFuncInt(5), 6)
        
        // String funcion
        let implementationString (x:obj) = box( unbox<string>(x) + " function")
        let resultFuncStringObj  = FSharpValue.MakeFunction(typeof<string -> string>, implementationString )
        let resultFuncString = resultFuncStringObj :?> (string -> string)
        Assert.AreEqual(resultFuncString("parameter"), "parameter function")
        
    [<Test>]
    member __.MakeRecord() =
        // Record
        let makeRecord = FSharpValue.MakeRecord(typeof<RecordType>, [| box"field1"; box(Some(record1)); box( fun () -> (record1, "")) |])
        Assert.AreEqual(FSharpValue.GetRecordFields(makeRecord).[0], "field1")
        
        // Generic Record value
        let makeRecordGeneric = FSharpValue.MakeRecord(typeof<GenericRecordType<string, int>>, [| box"field1"; box 1; box( fun () -> genericRecordType1) |])
        Assert.AreEqual(FSharpValue.GetRecordFields(makeRecordGeneric).[0], "field1")
        
        // null value
        CheckThrowsArgumentException(fun () ->FSharpValue.MakeRecord(null, null)|> ignore)
        
        // invalid value        
        CheckThrowsArgumentException(fun () ->  FSharpValue.MakeRecord(typeof<GenericRecordType<string, int>>, [| box 1; box("invalid param"); box("invalid param") |])|> ignore)
        
    [<Test>]
    member __.MakeStructRecord() =
        // Record
        let makeRecord = FSharpValue.MakeRecord(typeof<StructRecordType>, [| box"field1"; box(Some(structRecord1)); box( fun () -> (structRecord1, "")) |])
        Assert.AreEqual(FSharpValue.GetRecordFields(makeRecord).[0], "field1")
        
    [<Test>]
    member __.MakeTuple() =
        // Tuple
        let makeTuple = FSharpValue.MakeTuple([| box 1; box("tuple") |], typeof<Tuple<int, string>>)
        Assert.AreEqual(FSharpValue.GetTupleFields(makeTuple).[0], 1)
        
        // Tuple with function
        let makeTuplewithFunc = FSharpValue.MakeTuple([| box 1; box "tuple with func"; box (fun x -> x + 1) |], typeof<Tuple<int, string, (int -> int)>>)
        Assert.AreEqual(FSharpValue.GetTupleFields(makeTuplewithFunc).[1], "tuple with func")
        
        // null value
        CheckThrowsArgumentNullException(fun () ->FSharpValue.MakeTuple(null, null)|> ignore)
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.MakeTuple([| box"invalid param"; box"invalid param"|], typeof<Tuple<int, string>>)  |> ignore)
        
    [<Test>]
    member __.MakeStructTuple() =
        // Tuple
        let makeTuple = FSharpValue.MakeTuple([| box 1; box("tuple") |], typeof<struct (int * string)>)
        Assert.AreEqual(FSharpValue.GetTupleFields(makeTuple).[0], 1)
        Assert.AreEqual(FSharpValue.GetTupleFields(makeTuple).[1], "tuple")
        
        // Tuple with function
        let makeTuplewithFunc = FSharpValue.MakeTuple([| box 1; box "tuple with func"; box (fun x -> x + 1) |], typeof<struct (int * string * (int -> int))>)
        Assert.AreEqual(FSharpValue.GetTupleFields(makeTuplewithFunc).[0], 1)
        Assert.AreEqual(FSharpValue.GetTupleFields(makeTuplewithFunc).[1], "tuple with func")
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.MakeTuple([| box"invalid param"; box"invalid param"|], typeof<struct (int * string)>)  |> ignore)
        
    [<Test>]
    member __.MakeUnion() =
        // single case union  
        let (singlecaseinfo, _singlevaluearray) = FSharpValue.GetUnionFields(singleCaseUnion1, typeof<SingleCaseDiscUnion>)
        let resultSingleCaseUnion=FSharpValue.MakeUnion(singlecaseinfo, [| box 1.0; box 2.0; box 3.0|])
        Assert.AreEqual(resultSingleCaseUnion, singleCaseUnion1)
        
        // DiscUnionType
        let (duCaseinfo, _duValueArray) = FSharpValue.GetUnionFields(discUnionCaseB, typeof<DiscUnionType<int>>)
        let resultDiscUnion=FSharpValue.MakeUnion(duCaseinfo, [| box 1; box (Some discUnionCaseB) |])
        Assert.AreEqual(resultDiscUnion, discUnionRecCaseB)
        
    [<Test>]
    member __.MakeStructUnion() =
        // single case union  
        let (singlecaseinfo, _singlevaluearray) = FSharpValue.GetUnionFields(singleCaseStructUnion1, typeof<SingleCaseDiscStructUnion>)
        let resultSingleCaseUnion=FSharpValue.MakeUnion(singlecaseinfo, [| box 1.0; box 2.0; box 3.0|])
        Assert.AreEqual(resultSingleCaseUnion, singleCaseStructUnion1)
        
        // DiscUnionType
        let (duCaseinfo, duValueArray) = FSharpValue.GetUnionFields(discStructUnionCaseB, typeof<DiscStructUnionType<int>>)
        FSharpValue.MakeUnion(duCaseinfo, [| box 1|]) |> ignore
        
    [<Test>]
    member __.PreComputeRecordConstructor() =
        // Record
        let recCtor = FSharpValue.PreComputeRecordConstructor(typeof<RecordType>)
        let resultRecordType   = recCtor([| box("field1"); box(Some(record1)); box(fun () -> (record1, "")) |])
        Assert.AreEqual( (unbox<RecordType>(resultRecordType)).field1 , record1.field1)
        
        // Generic Record value
        let genericRecCtor = FSharpValue.PreComputeRecordConstructor(typeof<GenericRecordType<string, int>>)
        let resultGenericRecordType = genericRecCtor([| box("field1"); box 2; box( fun () -> genericRecordType1) |])
        Assert.AreEqual( (unbox<GenericRecordType<string, int>>(resultGenericRecordType)).field1, genericRecordType1.field1)
        
        // null value
        CheckThrowsArgumentException(fun () ->FSharpValue.PreComputeRecordConstructor(null)|> ignore)
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.PreComputeRecordConstructor(typeof<DiscUnionType<string>>) |> ignore)        
        
    [<Test>]
    member __.PreComputeStructRecordConstructor() =
        // Record
        let recCtor = FSharpValue.PreComputeRecordConstructor(typeof<StructRecordType>)
        let resultRecordType   = recCtor([| box("field1"); box(Some(structRecord1)); box(fun () -> (structRecord1, "")) |])
        Assert.AreEqual( (unbox<StructRecordType>(resultRecordType)).field1 , structRecord1.field1)
        
       
    [<Test>]
    member __.PreComputeRecordConstructorInfo() =
        // Record
        let recordCtorInfo = FSharpValue.PreComputeRecordConstructorInfo(typeof<RecordType>)
        Assert.AreEqual(recordCtorInfo.ReflectedType, typeof<RecordType> )
        
        // Generic Record value
        let genericrecordCtorInfo = FSharpValue.PreComputeRecordConstructorInfo(typeof<GenericRecordType<string, int>>)
        Assert.AreEqual(genericrecordCtorInfo.ReflectedType, typeof<GenericRecordType<string, int>>)
        
        // null value
        CheckThrowsArgumentException(fun () ->FSharpValue.PreComputeRecordConstructorInfo(null)|> ignore)
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.PreComputeRecordConstructorInfo(typeof<DiscUnionType<string>>) |> ignore)        
        
    [<Test>]
    member __.PreComputeStructRecordConstructorInfo() =
        // Record
        let recordCtorInfo = FSharpValue.PreComputeRecordConstructorInfo(typeof<StructRecordType>)
        Assert.AreEqual(recordCtorInfo.ReflectedType, typeof<StructRecordType> )
        
    [<Test>]
    member __.PreComputeRecordFieldReader() =
        // Record
        let recordFieldReader = FSharpValue.PreComputeRecordFieldReader((typeof<RecordType>).GetProperty("field1"))
        Assert.AreEqual(recordFieldReader(record1), box("field1"))
        
        // Generic Record value
        let recordFieldReader = FSharpValue.PreComputeRecordFieldReader((typeof<GenericRecordType<string, int>>).GetProperty("field1"))
        Assert.AreEqual(recordFieldReader(genericRecordType1), box("field1"))
        
        // null value
        CheckThrowsArgumentException(fun () -> FSharpValue.PreComputeRecordFieldReader(null)|> ignore)    
        
    [<Test>]
    member __.PreComputeStructRecordFieldReader() =
        // Record
        let recordFieldReader = FSharpValue.PreComputeRecordFieldReader((typeof<StructRecordType>).GetProperty("field1"))
        Assert.AreEqual(recordFieldReader(structRecord1), box("field1"))
        
    [<Test>]
    member __.PreComputeRecordReader() =
        // Record
        let recordReader = FSharpValue.PreComputeRecordReader(typeof<RecordType>)
        Assert.AreEqual( (recordReader(record1)).[0], "field1")
        
        // Generic Record value
        let genericrecordReader = FSharpValue.PreComputeRecordReader(typeof<GenericRecordType<string, int>>)
        Assert.AreEqual( (genericrecordReader(genericRecordType1)).[0], "field1")
        
        // null value
        CheckThrowsArgumentException(fun () ->FSharpValue.PreComputeRecordReader(null)|> ignore)
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.PreComputeRecordReader(typeof<DiscUnionType<string>>) |> ignore)        
    
    [<Test>]
    member __.PreComputeStructRecordReader() =
        // Record
        let recordReader = FSharpValue.PreComputeRecordReader(typeof<StructRecordType>)
        Assert.AreEqual( (recordReader(structRecord1)).[0], "field1")
    
    [<Test>]
    member __.PreComputeTupleConstructor() =
        // Tuple
        let tupleCtor = FSharpValue.PreComputeTupleConstructor(tuple1.GetType())    
        Assert.AreEqual( tupleCtor([| box 1; box "tuple1" |]) , box(tuple1))
        
        // Tuple with function member
        let tuplewithFuncCtor = FSharpValue.PreComputeTupleConstructor(tuple2.GetType())  
        let resultTuplewithFunc = tuplewithFuncCtor([| box 2; box "tuple2"; box (fun x -> x + 1) |])
        Assert.AreEqual( FSharpValue.GetTupleFields( box(resultTuplewithFunc)).[1] , "tuple2")
        
        // nested tuple
        let tupleNestedCtor = FSharpValue.PreComputeTupleConstructor(tuple3.GetType())
        Assert.AreEqual( tupleNestedCtor([| box 1; box(2, "tuple") |] ), box(tuple3))
         
        // null value
        CheckThrowsArgumentException(fun () -> FSharpValue.PreComputeTupleConstructor(null)|> ignore)
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.PreComputeTupleConstructor(typeof<DiscUnionType<string>>) |> ignore)        
        CheckThrowsArgumentException(fun () -> FSharpValue.PreComputeTupleConstructor(typeof<unit>) |> ignore)        
    
    [<Test>]
    member __.PreComputeStructTupleConstructor() =
        // Tuple
        let tupleCtor = FSharpValue.PreComputeTupleConstructor(structTuple1.GetType())    
        Assert.AreEqual( tupleCtor([| box 1; box "tuple1" |]) , box(structTuple1))
        
        // Tuple with function member
        let tuplewithFuncCtor = FSharpValue.PreComputeTupleConstructor(structTuple2.GetType())  
        let resultTuplewithFunc = tuplewithFuncCtor([| box 2; box "tuple2"; box (fun x -> x + 1) |])
        Assert.AreEqual( FSharpValue.GetTupleFields( box(resultTuplewithFunc)).[1] , "tuple2")
        
        // nested tuple
        let tupleNestedCtor = FSharpValue.PreComputeTupleConstructor(structTuple3.GetType())
        Assert.AreEqual( tupleNestedCtor([| box 1; box (struct (2, "tuple")) |] ), box(structTuple3))
         
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.PreComputeTupleConstructor(typeof<DiscStructUnionType<string>>) |> ignore)        

    [<Test>]
    member __.PreComputeTupleConstructorInfo() =
        // Tuple
        let (tupleCtorInfo, _tupleType) = FSharpValue.PreComputeTupleConstructorInfo(typeof<Tuple<int, string>>)    
        Assert.AreEqual(tupleCtorInfo.ReflectedType, typeof<Tuple<int, string>> )
        
        // Nested 
        let (nestedTupleCtorInfo, _nestedTupleType) = FSharpValue.PreComputeTupleConstructorInfo(typeof<Tuple<int, Tuple<int, string>>>)    
        Assert.AreEqual(nestedTupleCtorInfo.ReflectedType, typeof<Tuple<int, Tuple<int, string>>>)
        
        // null value
        CheckThrowsArgumentException(fun () ->FSharpValue.PreComputeTupleConstructorInfo(null)|> ignore)
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.PreComputeTupleConstructorInfo(typeof<RecordType>) |> ignore)        
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.PreComputeTupleConstructorInfo(typeof<StructRecordType>) |> ignore)        
        
    [<Test>]
    member __.PreComputeStructTupleConstructorInfo() =
        // Tuple
        let (tupleCtorInfo, _tupleType) = FSharpValue.PreComputeTupleConstructorInfo(typeof<struct (int * string)>)    
        Assert.AreEqual(tupleCtorInfo.ReflectedType, typeof<struct (int * string)> )
        
        // Nested 
        let (nestedTupleCtorInfo, _nestedTupleType) = FSharpValue.PreComputeTupleConstructorInfo(typeof<struct (int * struct (int * string))>)    
        Assert.AreEqual(nestedTupleCtorInfo.ReflectedType, typeof<struct (int * struct (int * string))>)
        
    [<Test>]
    member __.PreComputeTuplePropertyInfo() =
    
        // Tuple
        let (tuplePropInfo, _typeindex) = FSharpValue.PreComputeTuplePropertyInfo(typeof<Tuple<int, string>>, 0)    
        Assert.AreEqual(tuplePropInfo.PropertyType, typeof<int>)
        
        // Nested 
        let (tupleNestedPropInfo, _typeindex) = FSharpValue.PreComputeTuplePropertyInfo(typeof<Tuple<int, Tuple<int, string>>>, 1)    
        Assert.AreEqual(tupleNestedPropInfo.PropertyType, typeof<Tuple<int, string>>)
        
        // null value
        CheckThrowsArgumentException(fun () ->FSharpValue.PreComputeTuplePropertyInfo(null, 0)|> ignore)
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.PreComputeTuplePropertyInfo(typeof<RecordType>, 0) |> ignore)        

        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.PreComputeTuplePropertyInfo(typeof<StructRecordType>, 0) |> ignore)        

    // This fails as no PropertyInfo's actually exist for struct tuple types. 
    //
    //[<Test>]
    //member __.PreComputeStructTuplePropertyInfo() =
    //
    //    // Tuple
    //    let (tuplePropInfo, _typeindex) = FSharpValue.PreComputeTuplePropertyInfo(typeof<struct (int * string)>, 0)    
    //    Assert.AreEqual(tuplePropInfo.PropertyType, typeof<int>)
    //    
    //    // Nested 
    //    let (tupleNestedPropInfo, _typeindex) = FSharpValue.PreComputeTuplePropertyInfo(typeof<struct (int * struct (int * string))>, 1)    
    //    Assert.AreEqual(tupleNestedPropInfo.PropertyType, typeof<struct (int * string)>)
        
    [<Test>]
    member __.PreComputeTupleReader() =
    
        // Tuple
        let tuplereader = FSharpValue.PreComputeTupleReader(typeof<Tuple<int, string>>)    
        Assert.AreEqual(tuplereader(tuple1).[0], 1)
        
        // Nested 
        let nestedtuplereader = FSharpValue.PreComputeTupleReader(typeof<Tuple<int, Tuple<int, string>>>)    
        Assert.AreEqual(nestedtuplereader(tuple3).[1], box(2, "tuple"))
        
        // null value
        CheckThrowsArgumentException(fun () ->FSharpValue.PreComputeTupleReader(null)|> ignore)
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.PreComputeTupleReader(typeof<RecordType>) |> ignore)        

    [<Test>]
    member __.PreComputeStructTupleReader() =
    
        // Tuple
        let tuplereader = FSharpValue.PreComputeTupleReader(typeof<struct (int  * string)>)    
        Assert.AreEqual(tuplereader(structTuple1).[0], 1)
        
        // Nested 
        let nestedtuplereader = FSharpValue.PreComputeTupleReader(typeof<struct (int * struct (int * string))>)    
        Assert.AreEqual(nestedtuplereader(structTuple3).[1], box (struct (2, "tuple")))
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.PreComputeTupleReader(typeof<StructRecordType>) |> ignore)        
        
    [<Test>]
    member __.PreComputeUnionConstructor() =
    
        // SingleCaseUnion
        let (singlecaseinfo, _singlevaluearray) = FSharpValue.GetUnionFields(singleCaseUnion1, typeof<SingleCaseDiscUnion>)
        let singleUnionCtor = FSharpValue.PreComputeUnionConstructor(singlecaseinfo)    
        let resuleSingleCaseUnion = singleUnionCtor([| box 1.0; box 2.0; box 3.0|])
        Assert.AreEqual(resuleSingleCaseUnion, singleCaseUnion1)
        
        // DiscUnion
        let (discunioninfo, _discunionvaluearray) = FSharpValue.GetUnionFields(discUnionCaseB, typeof<DiscUnionType<int>>)
        let discUnionCtor = FSharpValue.PreComputeUnionConstructor(discunioninfo)    
        let resuleDiscUnionB = discUnionCtor([| box 1; box(Some(discUnionCaseB)) |])
        Assert.AreEqual(resuleDiscUnionB, discUnionRecCaseB)

    [<Test>]
    member __.PreComputeStructUnionConstructor() =
    
        // SingleCaseUnion
        let (singlecaseinfo, _singlevaluearray) = FSharpValue.GetUnionFields(singleCaseStructUnion1, typeof<SingleCaseDiscStructUnion>)
        let singleUnionCtor = FSharpValue.PreComputeUnionConstructor(singlecaseinfo)    
        let resuleSingleCaseUnion = singleUnionCtor([| box 1.0; box 2.0; box 3.0|])
        Assert.AreEqual(resuleSingleCaseUnion, singleCaseStructUnion1)
        
        // DiscUnion
        let (discunioninfo, _discunionvaluearray) = FSharpValue.GetUnionFields(discStructUnionCaseB, typeof<DiscStructUnionType<int>>)
        let discUnionCtor = FSharpValue.PreComputeUnionConstructor(discunioninfo)    
        let resuleDiscUnionB = discUnionCtor([| box 1|])
        Assert.AreEqual(resuleDiscUnionB, discStructUnionCaseB)
    
    [<Test>]
    member __.PreComputeUnionConstructorInfo() =
    
        // SingleCaseUnion
        let (singlecaseinfo, _singlevaluearray) = FSharpValue.GetUnionFields(singleCaseUnion1, typeof<SingleCaseDiscUnion>)
        let singlecaseMethodInfo = FSharpValue.PreComputeUnionConstructorInfo(singlecaseinfo)    
        Assert.AreEqual(singlecaseMethodInfo.ReflectedType, typeof<SingleCaseDiscUnion>)
        
        // DiscUnion
        let (discUnionInfo, _discvaluearray) = FSharpValue.GetUnionFields(discUnionCaseB, typeof<DiscUnionType<int>>)
        let discUnionMethodInfo = FSharpValue.PreComputeUnionConstructorInfo(discUnionInfo)    
        Assert.AreEqual(discUnionMethodInfo.ReflectedType, typeof<DiscUnionType<int>>)
    
    [<Test>]
    member __.PreComputeStructUnionConstructorInfo() =
    
        // SingleCaseUnion
        let (singlecaseinfo, _singlevaluearray) = FSharpValue.GetUnionFields(singleCaseStructUnion1, typeof<SingleCaseDiscStructUnion>)
        let singlecaseMethodInfo = FSharpValue.PreComputeUnionConstructorInfo(singlecaseinfo)    
        Assert.AreEqual(singlecaseMethodInfo.ReflectedType, typeof<SingleCaseDiscStructUnion>)
        
        // DiscUnion
        let (discUnionInfo, _discvaluearray) = FSharpValue.GetUnionFields(discStructUnionCaseB, typeof<DiscStructUnionType<int>>)
        let discUnionMethodInfo = FSharpValue.PreComputeUnionConstructorInfo(discUnionInfo)    
        Assert.AreEqual(discUnionMethodInfo.ReflectedType, typeof<DiscStructUnionType<int>>)

    [<Test>]
    member __.PreComputeUnionReader() =
    
        // SingleCaseUnion
        let (singlecaseinfo, singlevaluearray) = FSharpValue.GetUnionFields(singleCaseUnion1, typeof<SingleCaseDiscUnion>)
        let singlecaseUnionReader = FSharpValue.PreComputeUnionReader(singlecaseinfo)    
        Assert.AreEqual(singlecaseUnionReader(box(singleCaseUnion1)), [| box 1.0; box 2.0; box 3.0|])
        
        // DiscUnion
        let (discUnionInfo, discvaluearray) = FSharpValue.GetUnionFields(discUnionRecCaseB, typeof<DiscUnionType<int>>)
        let discUnionReader = FSharpValue.PreComputeUnionReader(discUnionInfo)    
        Assert.AreEqual(discUnionReader(box(discUnionRecCaseB)) , [| box 1; box(Some(discUnionCaseB)) |])
        
    [<Test>]
    member __.PreComputeStructUnionReader() =
    
        // SingleCaseUnion
        let (singlecaseinfo, singlevaluearray) = FSharpValue.GetUnionFields(singleCaseStructUnion1, typeof<SingleCaseDiscStructUnion>)
        let singlecaseUnionReader = FSharpValue.PreComputeUnionReader(singlecaseinfo)    
        Assert.AreEqual(singlecaseUnionReader(box(singleCaseStructUnion1)), [| box 1.0; box 2.0; box 3.0|])
        
        // DiscUnion
        let (discUnionInfo, discvaluearray) = FSharpValue.GetUnionFields(discStructUnionCaseB, typeof<DiscStructUnionType<int>>)
        let discUnionReader = FSharpValue.PreComputeUnionReader(discUnionInfo)    
        Assert.AreEqual(discUnionReader(box(discStructUnionCaseB)) , [| box 1|])
        
    [<Test>]
    member __.PreComputeUnionTagMemberInfo() =
    
        // SingleCaseUnion
        let singlecaseUnionMemberInfo = FSharpValue.PreComputeUnionTagMemberInfo(typeof<SingleCaseDiscUnion>) 
        Assert.AreEqual(singlecaseUnionMemberInfo.ReflectedType, typeof<SingleCaseDiscUnion>)
   
        // DiscUnion
        let discUnionMemberInfo = FSharpValue.PreComputeUnionTagMemberInfo(typeof<DiscUnionType<int>>) 
        Assert.AreEqual(discUnionMemberInfo.ReflectedType, typeof<DiscUnionType<int>>)
        
         // null value
        CheckThrowsArgumentException(fun () ->FSharpValue.PreComputeUnionTagMemberInfo(null)|> ignore)
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.PreComputeUnionTagMemberInfo(typeof<RecordType>) |> ignore)        

    [<Test>]
    member __.PreComputeStructUnionTagMemberInfo() =
    
        // SingleCaseUnion
        let singlecaseUnionMemberInfo = FSharpValue.PreComputeUnionTagMemberInfo(typeof<SingleCaseDiscStructUnion>) 
        Assert.AreEqual(singlecaseUnionMemberInfo.ReflectedType, typeof<SingleCaseDiscStructUnion>)
   
        // DiscUnion
        let discUnionMemberInfo = FSharpValue.PreComputeUnionTagMemberInfo(typeof<DiscStructUnionType<int>>) 
        Assert.AreEqual(discUnionMemberInfo.ReflectedType, typeof<DiscStructUnionType<int>>)
        
    [<Test>]
    member __.PreComputeUnionTagReader() =
    
        // SingleCaseUnion
        let singlecaseUnionTagReader = FSharpValue.PreComputeUnionTagReader(typeof<SingleCaseDiscUnion>) 
        Assert.AreEqual(singlecaseUnionTagReader(box(singleCaseUnion1)), 0)
   
        // DiscUnion
        let discUnionTagReader = FSharpValue.PreComputeUnionTagReader(typeof<DiscUnionType<int>>) 
        Assert.AreEqual(discUnionTagReader(box(discUnionCaseB)), 1)
        
         // null value
        CheckThrowsArgumentException(fun () ->FSharpValue.PreComputeUnionTagReader(null)|> ignore)
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpValue.PreComputeUnionTagReader(typeof<RecordType>) |> ignore)        
        
    [<Test>]
    member __.PreComputeStructUnionTagReader() =
    
        // SingleCaseUnion
        let singlecaseUnionTagReader = FSharpValue.PreComputeUnionTagReader(typeof<SingleCaseDiscStructUnion>) 
        Assert.AreEqual(singlecaseUnionTagReader(box(singleCaseStructUnion1)), 0)
   
        // DiscUnion
        let discUnionTagReader = FSharpValue.PreComputeUnionTagReader(typeof<DiscStructUnionType<int>>) 
        Assert.AreEqual(discUnionTagReader(box(discStructUnionCaseB)), 1)
        
        
[<TestFixture>]
type FSharpTypeTests() =    
    
    // instance for member __.ObjectEquals
    let rec recordtype1 : RecordType = { field1 = "field1"; field2 = Some(recordtype1); field3 = ( fun () -> (recordtype1, "")  )}
    let recordtype2 : RecordType = { field1 = "field2"; field2 = Some(recordtype1); field3 = ( fun () -> (recordtype1, "")  )}
    let rec genericRecordType1 : GenericRecordType<string, int> = { field1 = "field1"; field2 = 1; field3 = ( fun () -> genericRecordType1 )}
    let genericRecordType2 : GenericRecordType<string, int> = { field1 = "field1"; field2 = 1; field3 = ( fun () -> genericRecordType1 )}
    
    let nullValue = null
    
    let singlecaseunion1 = SingleCaseDiscUnion.SingleCaseTag(1.0, 2.0, 3.0)
    let singlecaseunion2 = SingleCaseDiscUnion.SingleCaseTag(4.0, 5.0, 6.0)
    
    let discUniontypeA = DiscUnionType.A
    let discUniontypeB = DiscUnionType.B(1, Some(discUniontypeA))
    let discUniontypeC = DiscUnionType.C(1.0, "stringparam")
    
    let fsharpdelegate1 = new FSharpDelegate(fun (x:int) -> "delegate1")
    let fsharpdelegate2 = new FSharpDelegate(fun (x:int) -> "delegate2")
    
    
    let tuple1 = ( 1, "tuple1")
    let tuple2 = ( 2, "tuple2")
    
    let func1  param  = param + 1
    let func2  param  = param + ""
    
    let exInt = ExceptionInt(1)
    let exDataless = DatalessException
    
    // Base class methods
    [<Test>]
    member __.ObjectEquals() =       
        
        // Record value                
        Assert.IsTrue(FSharpValue.Equals(recordtype1, recordtype1))
        Assert.IsFalse(FSharpValue.Equals(recordtype1, recordtype2))

        // Generic Record value
        Assert.IsTrue(FSharpValue.Equals(genericRecordType1, genericRecordType1))
        Assert.IsFalse(FSharpValue.Equals(genericRecordType1, genericRecordType2))
        
        // null value
        Assert.IsTrue(FSharpValue.Equals(nullValue, nullValue))
        Assert.IsFalse(FSharpValue.Equals(nullValue, 1))
        
        // Single Case Union
        Assert.IsTrue(FSharpValue.Equals(singlecaseunion1, singlecaseunion1))
        Assert.IsFalse(FSharpValue.Equals(singlecaseunion1, singlecaseunion2))
        
        // Discriminated Union
        Assert.IsTrue(FSharpValue.Equals(discUniontypeA, discUniontypeA))
        Assert.IsFalse(FSharpValue.Equals(discUniontypeB, discUniontypeC))
      
        // FSharpDelegate
        Assert.IsTrue(FSharpValue.Equals(fsharpdelegate1, fsharpdelegate1))
        Assert.IsFalse(FSharpValue.Equals(fsharpdelegate1, fsharpdelegate2))
        
        // Tuple
        Assert.IsTrue(FSharpValue.Equals(tuple1, tuple1))
        Assert.IsFalse(FSharpValue.Equals(tuple1, tuple2))
     
        // Exception
        Assert.IsTrue(FSharpValue.Equals(exInt, exInt))
        Assert.IsFalse(FSharpValue.Equals(exInt, exDataless))   
       
    
    // Static methods
    [<Test>]
    member __.GetExceptionFields() =        
        
        // positive               
        let forallexistedInt = 
            FSharpType.GetExceptionFields(typeof<ExceptionInt>) 
            |> Array.forall (fun property -> (Array.IndexOf<Reflection.PropertyInfo>(typeof<ExceptionInt>.GetProperties(), property) > -1))

        Assert.IsTrue(forallexistedInt)
        
        let forallexistedDataless = 
            FSharpType.GetExceptionFields(typeof<DatalessException>) 
            |> Array.forall (fun property -> (Array.IndexOf<Reflection.PropertyInfo>(typeof<DatalessException>.GetProperties(), property) > -1))
        Assert.IsTrue(forallexistedDataless)
       
        // Argument Exception
        CheckThrowsArgumentException(fun () ->FSharpType.GetExceptionFields(typeof<RecordType>) |> ignore )
        
        // null
        CheckThrowsArgumentNullException(fun () ->FSharpType.GetExceptionFields(null) |> ignore )
        
        
    [<Test>]
    member __.GetFunctionElements() =    
               
        // positive
        Assert.AreEqual(FSharpType.GetFunctionElements(typeof<int -> string>), (typeof<Int32>, typeof<String>))  
        Assert.AreEqual(FSharpType.GetFunctionElements(typeof<int -> int -> string>), (typeof<Int32>, typeof<Microsoft.FSharp.Core.FSharpFunc<int, string>>))
        
        // argument exception
        CheckThrowsArgumentException(fun () ->FSharpType.GetFunctionElements(typeof<int>) |> ignore )
        
        // null
        CheckThrowsArgumentNullException(fun () ->FSharpType.GetFunctionElements(null) |> ignore )
        
    [<Test>]
    member __.GetRecordFields() =    
               
        // positive
        Assert.AreEqual(FSharpType.GetRecordFields(typeof<RecordType>), (typeof<RecordType>.GetProperties()))        
        Assert.AreEqual(FSharpType.GetRecordFields(typeof<GenericRecordType<int, string>>), (typeof<GenericRecordType<int, string>>.GetProperties()))
        
        // argument exception
        CheckThrowsArgumentException(fun () ->FSharpType.GetRecordFields(typeof<int>) |> ignore )
        
        // null
        CheckThrowsArgumentNullException(fun () ->FSharpType.GetRecordFields(null) |> ignore )
        
        
    [<Test>]
    member __.GetTupleElements() =    
               
        // positive
        Assert.AreEqual(FSharpType.GetTupleElements(typeof<Tuple<string, int>>), [|typeof<System.String>; typeof<System.Int32>|])
        Assert.AreEqual(FSharpType.GetTupleElements(typeof<Tuple<string, int, int>>), [|typeof<System.String>; typeof<System.Int32>;typeof<System.Int32>|])
        
        // argument exception
        CheckThrowsArgumentException(fun () ->FSharpType.GetTupleElements(typeof<int>) |> ignore )
        
        // null
        CheckThrowsArgumentNullException(fun () ->FSharpType.GetTupleElements(null) |> ignore )
        
    [<Test>]
    member __.GetUnionCases() =    
        // SingleCaseUnion
        let singlecaseUnionCaseInfoArray = FSharpType.GetUnionCases(typeof<SingleCaseDiscUnion>)  
        let (expectedSinglecaseinfo, singlevaluearray) = FSharpValue.GetUnionFields(singlecaseunion1, typeof<SingleCaseDiscUnion>)
        Assert.AreEqual(singlecaseUnionCaseInfoArray.[0], expectedSinglecaseinfo)
        
        // DiscUnionType
        let discunionCaseInfoArray = FSharpType.GetUnionCases(typeof<DiscUnionType<int>>) 
        let (expectedDuCaseinfoArray, duValueArray) = FSharpValue.GetUnionFields(discUniontypeB, typeof<DiscUnionType<int>>)
        Assert.AreEqual(discunionCaseInfoArray.[1], expectedDuCaseinfoArray)
        
         // null value
        CheckThrowsArgumentNullException(fun () ->FSharpType.GetUnionCases(null)|> ignore)
        
        // invalid value
        CheckThrowsArgumentException(fun () -> FSharpType.GetUnionCases(typeof<RecordType>) |> ignore)  
        
    [<Test>]
    member __.IsExceptionRepresentation() =    
        
        // positive
        Assert.IsTrue(FSharpType.IsExceptionRepresentation(typeof<ExceptionInt>))
        Assert.IsTrue(FSharpType.IsExceptionRepresentation(typeof<DatalessException>))

        // negative
        Assert.IsFalse(FSharpType.IsExceptionRepresentation(typeof<int>))
        Assert.IsFalse(FSharpType.IsExceptionRepresentation(typeof<unit>))
        
        // null
        CheckThrowsArgumentNullException(fun () -> FSharpType.IsExceptionRepresentation(null) |> ignore )
        
    [<Test>]
    member __.IsFunction() =    
        
        // positive       
        Assert.IsTrue(FSharpType.IsFunction(typeof<string -> int>))
        Assert.IsTrue(FSharpType.IsFunction(typeof<string -> int -> int>))
        
        // negative
        Assert.IsFalse(FSharpType.IsFunction(typeof<string>))
        
        // null
        CheckThrowsArgumentNullException(fun () -> FSharpType.IsFunction(null) |> ignore )

    [<Test>]
    member __.IsModule() =   
    
        let getasm (t : Type) = t.Assembly
    
        // Positive Test
        let assemblyTypesPositive = (getasm (typeof<IsModule.IsModuleType>)).GetTypes()
        
        let moduleType =
            assemblyTypesPositive 
            |> Array.filter (fun ty -> ty.Name = "IsModule")
            |> (fun arr -> arr.[0])
        
        Assert.IsTrue(FSharpType.IsModule(moduleType)) //assemblyTypesPositive.[3] is Microsoft_FSharp_Reflection.FSharpModule which is module type
               
        // Negative Test 
        // FSharp Assembly
        let asmCore = getasm (typeof<Microsoft.FSharp.Collections.List<int>>)
        Assert.IsFalse(FSharpType.IsModule(asmCore.GetTypes().[0]))
        
        // .Net Assembly
        let asmSystem = getasm (typeof<System.DateTime>)
        Assert.IsFalse(FSharpType.IsModule(asmSystem.GetTypes().[0]))
        
        // custom Assembly
        let asmCustom = getasm (typeof<SingleCaseDiscUnion>)
        Assert.IsFalse(FSharpType.IsModule(asmCustom.GetTypes().[0]))
        
        // null
        CheckThrowsArgumentNullException(fun () -> FSharpType.IsModule(null) |> ignore )

    [<Test>]
    member __.IsRecord() =    
        
        // positive       
        Assert.IsTrue(FSharpType.IsRecord(typeof<RecordType>))
        Assert.IsTrue(FSharpType.IsRecord(typeof<StructRecordType>))
        Assert.IsTrue(FSharpType.IsRecord(typeof<GenericRecordType<int, string>>))
        
        // negative
        Assert.IsFalse(FSharpType.IsRecord(typeof<int>))
        
        // null
        CheckThrowsArgumentNullException(fun () ->FSharpType.IsRecord(null) |> ignore )

    // Regression for 5588, Reflection: unit is still treated as a record type, but only if you pass BindingFlags.NonPublic
    [<Test>]
    member __.``IsRecord.Regression5588``() =    
        
        // negative
        Assert.IsFalse(FSharpType.IsRecord(typeof<unit>))
        
#if FX_RESHAPED_REFLECTION
        Assert.IsFalse( FSharpType.IsRecord(typeof<unit>, true) )
#else 
        Assert.IsFalse( FSharpType.IsRecord(typeof<unit>, System.Reflection.BindingFlags.NonPublic) )
#endif
        ()

        
    [<Test>]
    member __.IsTuple() =    
               
        // positive
        Assert.IsTrue(FSharpType.IsTuple(typeof<Tuple<int, int>>))
        Assert.IsTrue(FSharpType.IsTuple(typeof<Tuple<int, int, string>>))

        Assert.IsTrue(FSharpType.IsTuple(typeof<struct (int * int)>))
        Assert.IsTrue(FSharpType.IsTuple(typeof<struct (int* int * string)>))
        
        // negative
        Assert.IsFalse(FSharpType.IsTuple(typeof<int>))
        Assert.IsFalse(FSharpType.IsTuple(typeof<unit>))
        
        // null
        CheckThrowsArgumentNullException(fun () ->FSharpType.IsTuple(null) |> ignore )
        
    [<Test>]
    member __.IsUnion() =    
        
        // positive       
        Assert.IsTrue(FSharpType.IsUnion(typeof<SingleCaseDiscUnion>))
        Assert.IsTrue(FSharpType.IsUnion(typeof<SingleCaseDiscStructUnion>))
        Assert.IsTrue(FSharpType.IsUnion(typeof<DiscUnionType<int>>))
        Assert.IsTrue(FSharpType.IsUnion(typeof<DiscStructUnionType<int>>))
        
        // negative
        Assert.IsFalse(FSharpType.IsUnion(typeof<int>))
        Assert.IsFalse(FSharpType.IsUnion(typeof<unit>))
        
        // null
        CheckThrowsArgumentNullException(fun () ->FSharpType.IsUnion(null) |> ignore )
        
    [<Test>]
    member __.MakeFunctionType() =    
        
        // positive       
        Assert.AreEqual(FSharpType.MakeFunctionType(typeof<int>, typeof<string>), typeof<int ->string>)       
       
        // negative 
        Assert.AreNotEqual(FSharpType.MakeFunctionType(typeof<int>, typeof<string>), typeof<int ->string->int>)    
        
        // null
        CheckThrowsArgumentNullException(fun () ->FSharpType.MakeFunctionType(null, null) |> ignore )
        
    [<Test>]
    member __.MakeTupleType() =    
               
        // positive
        Assert.AreEqual(FSharpType.MakeTupleType([|typeof<System.String>; typeof<System.Int32>|]), typeof<Tuple<string, int>>)
        
        // negative
        Assert.AreNotEqual(FSharpType.MakeTupleType([|typeof<System.String>; typeof<System.Int32>|]), typeof<Tuple<string, int, string>>)
        
        // null
        CheckThrowsArgumentException(fun () ->FSharpType.MakeTupleType([|null;null|]) |> ignore )

    [<Test>]
    member __.MakeStructTupleType() =    
        let asm = typeof<struct (string * int)>.Assembly
        // positive
        Assert.AreEqual(FSharpType.MakeStructTupleType(asm, [|typeof<System.String>; typeof<System.Int32>|]), typeof<struct (string * int)>)
        
        // negative
        Assert.AreNotEqual(FSharpType.MakeStructTupleType(asm, [|typeof<System.String>; typeof<System.Int32>|]), typeof<struct (string * int * string)>)
        
        // null
        CheckThrowsArgumentException(fun () ->FSharpType.MakeStructTupleType(asm, [|null;null|]) |> ignore )

            
[<TestFixture>]
type UnionCaseInfoTests() =    
    
    let singlenullarycaseunion = SingleNullaryCaseDiscUnion.SingleNullaryCaseTag

    let singlecaseunion1 = SingleCaseDiscUnion.SingleCaseTag(1.0, 2.0, 3.0)
    let singlecaseunion2 = SingleCaseDiscUnion.SingleCaseTag(4.0, 5.0, 6.0)
    
    let discUniontypeA = DiscUnionType<int>.A
    let discUniontypeB = DiscUnionType<int>.B(1, Some(discUniontypeA))
    let discUniontypeC = DiscUnionType<float>.C(1.0, "stringparam")
    
    let recDiscUniontypeB = DiscUnionType<int>.B(1, Some(discUniontypeB))
    
    let ((singlenullarycaseinfo:UnionCaseInfo), singlenullaryvaluearray) = FSharpValue.GetUnionFields(singlenullarycaseunion, typeof<SingleNullaryCaseDiscUnion>)

    let ((singlecaseinfo:UnionCaseInfo), singlevaluearray) = FSharpValue.GetUnionFields(singlecaseunion1, typeof<SingleCaseDiscUnion>)
    
    let ((discUnionInfoA:UnionCaseInfo), discvaluearray) = FSharpValue.GetUnionFields(discUniontypeA, typeof<DiscUnionType<int>>)
    let ((discUnionInfoB:UnionCaseInfo), discvaluearray) = FSharpValue.GetUnionFields(discUniontypeB, typeof<DiscUnionType<int>>)
    let ((discUnionInfoC:UnionCaseInfo), discvaluearray) = FSharpValue.GetUnionFields(discUniontypeC, typeof<DiscUnionType<float>>)
    
    let ((recDiscCaseinfo:UnionCaseInfo), recDiscCasevaluearray) = FSharpValue.GetUnionFields(recDiscUniontypeB, typeof<DiscUnionType<int>>)
    
    [<Test>]
    member __.Equals() =   
        //positive
        // single case
        Assert.IsTrue(singlecaseinfo.Equals(singlecaseinfo))
        
        // disc union
        Assert.IsTrue(discUnionInfoA.Equals(discUnionInfoA))
        
        // rec disc union
        Assert.IsTrue(recDiscCaseinfo.Equals(recDiscCaseinfo))
                
        // negative
        // single case
        Assert.IsFalse(singlecaseinfo.Equals(discUnionInfoA))
        
        // disc union
        Assert.IsFalse(discUnionInfoA.Equals(discUnionInfoB))
        
        // rec disc union
        Assert.IsFalse(recDiscCaseinfo.Equals(discUnionInfoA))
        
        // null
        Assert.IsFalse(singlecaseinfo.Equals(null))
        
    [<Test>]
    member __.GetCustomAttributes() =   
        
        // single case
        let singlecaseAttribute  = (singlecaseinfo.GetCustomAttributes()).[0] :?> Attribute
        Assert.AreEqual(singlecaseAttribute.ToString(), "Microsoft.FSharp.Core.CompilationMappingAttribute" )
        
        // disc union
        let discunionAttribute  = (discUnionInfoA.GetCustomAttributes()).[0] :?> Attribute
        Assert.AreEqual(discunionAttribute.ToString(), "Microsoft.FSharp.Core.CompilationMappingAttribute" )
        
        // rec disc union
        let recdiscAttribute  = (recDiscCaseinfo.GetCustomAttributes()).[0] :?> Attribute
        Assert.AreEqual(recdiscAttribute.ToString(), "Microsoft.FSharp.Core.CompilationMappingAttribute" )
        
        // null
        CheckThrowsArgumentNullException(fun () -> singlecaseinfo.GetCustomAttributes(null) |> ignore )    
        
    [<Test>]
    member __.GetFields() =   
        
        // single case
        let singlecaseFieldInfo  = (singlecaseinfo.GetFields()).[0]
        Assert.AreEqual(singlecaseFieldInfo.PropertyType , typeof<(float)>)
        
        // disc union null empty
        let discunionFieldInfoEpt  = discUnionInfoA.GetFields()
        Assert.AreEqual(discunionFieldInfoEpt.Length , 0)
        
        // disc union int
        let discunionFieldInfo  = (discUnionInfoB.GetFields()).[0] 
        Assert.AreEqual(discunionFieldInfo.PropertyType , typeof<int>)
        
        // rec disc union
        let recdiscFieldInfo  = (recDiscCaseinfo.GetFields()).[0] 
        Assert.AreEqual(recdiscFieldInfo.PropertyType , typeof<int>)
        
    [<Test>]
    member __.GetHashCode() =   
        
        // positive
        // single case
        Assert.AreEqual(singlecaseinfo.GetHashCode(), singlecaseinfo.GetHashCode())
        
        // disc union
   
        Assert.AreEqual(discUnionInfoA.GetHashCode(), discUnionInfoA.GetHashCode())
        
        // rec disc union
  
        Assert.AreEqual(recDiscCaseinfo.GetHashCode(), recDiscCaseinfo.GetHashCode())
        
        // negative
        // disc union
        Assert.AreNotEqual(discUnionInfoA.GetHashCode(), discUnionInfoB.GetHashCode())
        
    [<Test>]
    member __.GetType() =   
  
        // single case
        Assert.AreEqual(singlecaseinfo.GetType(), typeof<UnionCaseInfo> )
        
        // disc union
        Assert.AreEqual(discUnionInfoA.GetType(), typeof<UnionCaseInfo> )
        
        // rec disc union
        Assert.AreEqual(recDiscCaseinfo.GetType(), typeof<UnionCaseInfo> )  
    
    [<Test>]
    member __.ToString() =   
        
        // single case
        Assert.AreEqual(singlenullarycaseinfo.ToString(), "SingleNullaryCaseDiscUnion.SingleNullaryCaseTag")

        // single case
        Assert.AreEqual(singlecaseinfo.ToString(), "SingleCaseDiscUnion.SingleCaseTag")
        
        // disc union
        Assert.IsTrue((discUnionInfoA.ToString()).Contains("DiscUnionType") )
        
        // rec disc union
        Assert.IsTrue((recDiscCaseinfo.ToString()).Contains("DiscUnionType"))
