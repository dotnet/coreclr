// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.
module FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Core.RecordTypes

#nowarn "9"
#nowarn "44" // deprecation of some APIs on CoreCLR

open System
open System.Reflection
open System.Runtime.InteropServices
open NUnit.Framework
open FsCheck
open FsCheck.PropOperators

#if FX_RESHAPED_REFLECTION
open FSharp.Reflection.FSharpReflectionExtensions

[<AutoOpen>]
module PrimReflectionAdapters =
    
    type System.Type with
        member this.IsValueType = this.GetTypeInfo().IsValueType
#endif

type Record =
    {   A: int
        B: int
    }


let [<Test>] ``can compare records`` () = 
    Check.QuickThrowOnFailure <|
    fun (i1:int) (i2:int) ->
        i1 <> i2 ==>
        let r1 = { A = i1; B = i2 }
        let r2 = { A = i1; B = i2 }
        (r1 = r2)                   |@ "r1 = r2" .&.
        ({ r1 with A = r1.B} <> r2) |@ "{r1 with A = r1.B} <> r2" .&.
        (r1.Equals r2)              |@ "r1.Equals r2"


[<Struct>]
type StructRecord =
    {   C: int
        D: int
    }

let private hasAttribute<'T,'Attr>() =
    typeof<'T>.GetTypeInfo().GetCustomAttributes() |> Seq.exists  (fun x -> x.GetType() = typeof<'Attr>)


let [<Test>] ``struct records hold [<Struct>] metadata`` () =
    Assert.IsTrue (hasAttribute<StructRecord,StructAttribute>())


let [<Test>] ``struct records are comparable`` () =
    Check.QuickThrowOnFailure <|
    fun (i1:int) (i2:int) ->
        i1 <> i2 ==>
        let sr1 = { C = i1; D = i2 }
        let sr2 = { C = i1; D = i2 }
        (sr1 = sr2)                    |@ "sr1 = sr2" .&.
        ({ sr1 with C = sr1.D} <> sr2) |@ "{sr1 with C = sr1.D} <> sr2" .&.
        (sr1.Equals sr2)               |@ "sr1.Equals sr2"


let [<Test>] ``struct records support pattern matching`` () =
    Check.QuickThrowOnFailure <|
    fun (i1:int) (i2:int) ->
        let sr1 = { C = i1; D = i2 }
        (match sr1 with
        | { C = c; D = d } when c = i1 && d = i2 -> true
        | _ -> false) 
        |@ "with pattern match on struct record" .&.
        (sr1 |> function 
        | { C = c; D = d } when c = i1 && d = i2 -> true
        | _ -> false)
        |@ "function pattern match on struct record"


let [<Test>] ``struct records support let binds using `` () =
    Check.QuickThrowOnFailure <|
    fun (i1:int) (i2:int) ->
        let sr1 = { C = i1; D = i2 }
        let { C = c1; D = d2 } as sr2 = sr1
        (sr1 = sr2)          |@ "sr1 = sr2" .&.
        (c1 = i1 && d2 = i2) |@ "c1 = i1 && d2 = i2"


let [<Test>] ``struct records support function argument bindings`` () =
    Check.QuickThrowOnFailure <|
    fun (i1:int) (i2:int) ->
        let sr1 = { C = i1; D = i2 }
        let test sr1 ({ C = c1; D = d2 } as sr2) =
            sr1 = sr2 && c1 = i1 && d2 = i2
        test sr1 sr1      
        
        
[<Struct>]
type MutableStructRecord =
    {   mutable M1: int
        mutable M2: int
    }                  
    

let [<Test>] ``struct recrods fields can be mutated`` () =
    Check.QuickThrowOnFailure <|
    fun (i1:int) (i2:int) (m1:int) (m2:int) ->
        (i1 <> m1 && i2 <> m2) ==>
            let mutable sr1 = { M1 = i1; M2 = i2}
            sr1.M1 <- m1
            sr1.M2 <- m2
            sr1.M1 = m1 && sr1.M2 = m2


[<Struct>]
type StructRecordDefaultValue =
    {   [<DefaultValue (false)>] 
        R1: Record
        R2: StructRecord
    }


let [<Test>] ``struct records have correct behaviour with a [<DefaultValue>] on a ref type field`` () =
    Check.QuickThrowOnFailure <|
    fun (i1:int) (i2:int) ->
        let s = { C = i1; D = i2 }
        let r1 = { R2 = s }
        (obj.ReferenceEquals (r1.R1, null)) |@ "r1.R1 is null" .&.
        (r1.R2 = { C = i1; D = i2 })        |@ "r1.R2 = { C = i1; D = i2 }"


[<Struct>]
type StructRecordDefaultValue2 =
    {   R1: Record
        [<DefaultValue (false)>] 
        R2: StructRecord
    }


let [<Test>] ``struct records have correct behaviour with a [<DefaultValue>] on a value type field`` () =
    Check.QuickThrowOnFailure <|
    fun (i1:int) (i2:int) ->
        let r = { A = i1; B = i2 }
        let r1 = { R1 = r }
        (r1.R1 = { A = i1; B = i2 }) |@ "r1.R1 = { A = i1; B = i2 }" .&.
        (r1.R2 = { C = 0; D = 0 })   |@ "r1.R2 = { C = 0; D = 0 }"


let [<Test>] ``struct records exhibit correct behaviour for Unchecked.defaultof`` () =
    let x1 = { C = 0; D = 0 }
    let x2 : StructRecordDefaultValue = { R2 = { C = 0; D = 0 } }
    let x3 : StructRecordDefaultValue2 = { R1 = Unchecked.defaultof<Record> }

    let y1 = Unchecked.defaultof<StructRecord>
    let y2 = Unchecked.defaultof<StructRecordDefaultValue>
    let y3 = Unchecked.defaultof<StructRecordDefaultValue2>

    Assert.IsTrue ((x1 = y1))

    Assert.IsTrue (( (obj.ReferenceEquals (x2.R1, null)) = (obj.ReferenceEquals (y2.R1, null)) ))
    Assert.IsTrue ((x2.R2 = x1))
    Assert.IsTrue ((y2.R2 = x1))

    Assert.IsTrue (( (obj.ReferenceEquals (x3.R1, null)) = (obj.ReferenceEquals (y3.R1, null)) ))
    Assert.IsTrue ((x3.R2 = x1))
    Assert.IsTrue ((y3.R2 = x1))
 

[<Struct>]
[<CustomComparison; CustomEquality>]
type ComparisonStructRecord =
    {   C1 :int
        C2: int
    }
    override self.Equals other =
        match other with
        | :? ComparisonStructRecord as o ->  (self.C1 + self.C2) = (o.C1 + o.C2)
        | _ -> false

    override self.GetHashCode() = hash self
    interface IComparable with
        member self.CompareTo other =
            match other with
            | :? ComparisonStructRecord as o -> compare (self.C1 + self.C2) (o.C1 + o.C2)
            | _ -> invalidArg "other" "cannot compare values of different types"


let [<Test>] ``struct records support [<CustomEquality>]`` () =
    Check.QuickThrowOnFailure <|
    fun (i1:int) (i2:int) ->
        let sr1 = { C1 = i1; C2 = i2 }
        let sr2 = { C1 = i1; C2 = i2 }
        (sr1.Equals sr2)      


let [<Test>] ``struct records support [<CustomComparison>]`` () =
    Check.QuickThrowOnFailure <|
    fun (i1:int) (i2:int) (k1:int) (k2:int) ->        
        let sr1 = { C1 = i1; C2 = i2 }
        let sr2 = { C1 = k1; C2 = k2 }
        if   sr1 > sr2 then compare sr1 sr2 = 1
        elif sr1 < sr2 then compare sr1 sr2 = -1
        elif sr1 = sr2 then compare sr1 sr2 = 0
        else false


let [<Test>] ``struct records hold [<CustomComparison>] [<CustomEquality>] metadata`` () =
    Assert.IsTrue (hasAttribute<ComparisonStructRecord,CustomComparisonAttribute>())
    Assert.IsTrue (hasAttribute<ComparisonStructRecord,CustomEqualityAttribute>())


[<Struct>]
[<NoComparison; NoEquality>]
type NoComparisonStructRecord =
    {   N1 : int
        N2 : int
    }


let [<Test>] ``struct records hold [<NoComparison>] [<NoEquality>] metadata`` () =
    Assert.IsTrue (hasAttribute<NoComparisonStructRecord,NoComparisonAttribute>())
    Assert.IsTrue (hasAttribute<NoComparisonStructRecord,NoEqualityAttribute>())


[<Struct>]
[<StructLayout(LayoutKind.Explicit)>]
type ExplicitLayoutStructRecord =
    {   [<FieldOffset 8>] Z : int
        [<FieldOffset 4>] Y : int
        [<FieldOffset 0>] X : int
    }


let [<Test>] ``struct records offset fields correctly with [<StructLayout(LayoutKind.Explicit)>] and [<FieldOffset x>]`` () =
    let checkOffset fieldName offset = 
        offset = int (Marshal.OffsetOf (typeof<ExplicitLayoutStructRecord>, fieldName))
    Assert.IsTrue (checkOffset "X@" 0)
    Assert.IsTrue (checkOffset "Y@" 4)
    Assert.IsTrue (checkOffset "Z@" 8)


[<Struct>]
[<StructLayout(LayoutKind.Explicit)>]
type ExplicitLayoutMutableStructRecord =
    {   [<FieldOffset 8>] mutable Z : int
        [<FieldOffset 4>] mutable Y : int
        [<FieldOffset 0>] mutable X : int
    }


let [<Test>] ``struct records offset mutable fields correctly with [<StructLayout(LayoutKind.Explicit)>] and [<FieldOffset x>]`` () =
    let checkOffset fieldName offset = 
        offset = int (Marshal.OffsetOf (typeof<ExplicitLayoutMutableStructRecord>, fieldName))
    Assert.IsTrue (checkOffset "X@" 0)
    Assert.IsTrue (checkOffset "Y@" 4)
    Assert.IsTrue (checkOffset "Z@" 8)


[<Struct>]
type DefaultLayoutStructRecord =
    {   First   : int
        Second  : float
        Third   : decimal
        Fourth  : int
    }


[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type SequentialLayoutStructRecord =
    {   First   : int
        Second  : float
        Third   : decimal
        Fourth  : int
    }


let [<Test>] ``struct records order fields correctly with [<StructLayout(LayoutKind.Sequential)>]`` () =
    let compareOffsets field1 fn field2 = 
        fn  (Marshal.OffsetOf (typeof<SequentialLayoutStructRecord>, field1))
            (Marshal.OffsetOf (typeof<SequentialLayoutStructRecord>, field2))
    Assert.IsTrue (compareOffsets "First@"  (<) "Second@")
    Assert.IsTrue (compareOffsets "Second@" (<) "Third@")
    Assert.IsTrue (compareOffsets "Third@"  (<) "Fourth@")


let [<Test>] ``struct records default field order matches [<StructLayout(LayoutKind.Sequential)>]`` () =
    let compareOffsets field1 fn field2 = 
        fn  (Marshal.OffsetOf (typeof<DefaultLayoutStructRecord>, field1))
            (Marshal.OffsetOf (typeof<SequentialLayoutStructRecord>, field2))
    Assert.IsTrue (compareOffsets "First@"  (=) "First@")
    Assert.IsTrue (compareOffsets "Second@" (=) "Second@")
    Assert.IsTrue (compareOffsets "Third@"  (=) "Third@")
    Assert.IsTrue (compareOffsets "Fourth@" (=) "Fourth@")


[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type SequentialLayoutMutableStructRecord =
    {   mutable First   : int
        mutable Second  : float
        mutable Third   : decimal
        mutable Fourth  : int
    }


let [<Test>] ``struct records order mutable field correctly with [<StructLayout(LayoutKind.Sequential)>]`` () =
    let compareOffsets field1 fn field2 = 
        fn  (Marshal.OffsetOf (typeof<SequentialLayoutMutableStructRecord>, field1))
            (Marshal.OffsetOf (typeof<SequentialLayoutMutableStructRecord>, field2))
    Assert.IsTrue (compareOffsets "First@"  (<) "Second@")
    Assert.IsTrue (compareOffsets "Second@" (<) "Third@")
    Assert.IsTrue (compareOffsets "Third@"  (<) "Fourth@")

let [<Test>] ``can properly construct a struct record using FSharpValue.MakeRecord, and we get the fields by FSharpValue.GetRecordFields`` () =
    let structRecord = Microsoft.FSharp.Reflection.FSharpValue.MakeRecord (typeof<StructRecord>, [|box 1234;box 999|])

    Assert.IsTrue (structRecord.GetType().IsValueType)

    let fields = Microsoft.FSharp.Reflection.FSharpValue.GetRecordFields structRecord

    let c = (fields.[0] :?> int)
    Assert.AreEqual (1234, c)

    let d = (fields.[1] :?> int)
    Assert.AreEqual (999, d)

type DefaultLayoutMutableRecord =
    {   mutable First   : int
        mutable Second  : float
        mutable Third   : decimal
        mutable Fourth  : int
    }

let inline CX_get_A(x: ^T) = 
    ( (^T : (member A : int) (x)) )

let inline CX_get_C(x: ^T) = 
    ( (^T : (member C : int) (x)) )

let inline CX_set_First(x: ^T, v) = 
    ( (^T : (member First : int with set) (x,v)) )


type Members() = 
    static member CreateMutableStructRecord() = { M1 = 1; M2 = 2 } 


let [<Test>] ``inline constraints resolve correctly`` () =
    let v = CX_get_A ({ A = 1; B = 2 })
    Assert.AreEqual (1, v)

    let v2 = CX_get_C ({ C = 1; D = 2 })
    Assert.AreEqual (1, v2)
    
    let mutable m : DefaultLayoutMutableRecord =
        {   First   = 0xbaad1
            Second  = 0.987654
            Third   = 100.32M
            Fourth  = 0xbaad4 }

    let v3 = CX_set_First (m,1)
    Assert.AreEqual (1, m.First)

let [<Test>] ``member setters resolve correctly`` () =

    let v = Members.CreateMutableStructRecord()
    Assert.AreEqual (1, v.M1)
    
    //let v2 = Members.CreateMutableStructRecord(M1 = 100)
    //Assert.AreEqual (100, v2.M1)
    
