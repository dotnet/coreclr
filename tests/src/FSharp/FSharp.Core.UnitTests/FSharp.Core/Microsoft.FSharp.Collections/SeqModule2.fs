// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

namespace FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Collections

open System
open NUnit.Framework

open FSharp.Core.UnitTests.LibraryTestFx

type SeqWindowedTestInput<'t> =
    {
        InputSeq : seq<'t>
        WindowSize : int
        ExpectedSeq : seq<'t[]>
        Exception : Type option
    }

[<TestFixture>][<Category "Collections.Seq">][<Category "FSharp.Core.Collections">]
type SeqModule2() =

    [<Test>]
    member this.Hd() =
             
        let IntSeq =
            seq { for i in 0 .. 9 do
                    yield i }
                    
        if Seq.head IntSeq <> 0 then Assert.Fail()
                 
        // string Seq
        let strSeq = seq ["first"; "second";  "third"]
        if Seq.head strSeq <> "first" then Assert.Fail()
         
        // Empty Seq
        let emptySeq = Seq.empty
        CheckThrowsArgumentException ( fun() -> Seq.head emptySeq)
      
        // null Seq
        let nullSeq:seq<'a> = null
        CheckThrowsArgumentNullException (fun () ->Seq.head nullSeq) 
        () 

    [<Test>]
    member this.TryHead() =
        // int Seq     
        let IntSeq =
            seq { for i in 0 .. 9 -> i }
                    
        let intResult = Seq.tryHead IntSeq

        // string Seq
        let strResult = Seq.tryHead (seq ["first"; "second";  "third"])
        Assert.AreEqual("first", strResult.Value)
         
        // Empty Seq
        let emptyResult = Seq.tryHead Seq.empty
        Assert.AreEqual(None, emptyResult)
      
        // null Seq
        let nullSeq:seq<'a> = null
        CheckThrowsArgumentNullException (fun () ->Seq.head nullSeq) 
        () 
        
    [<Test>]
    member this.Tl() =
        // integer seq  
        let resultInt = Seq.tail <| seq { 1..10 }        
        Assert.AreEqual(Array.ofSeq (seq { 2..10 }), Array.ofSeq resultInt)
        
        // string seq
        let resultStr = Seq.tail <| seq { yield "a"; yield "b"; yield "c"; yield "d" }      
        Assert.AreEqual(Array.ofSeq (seq { yield "b";  yield "c" ; yield "d" }), Array.ofSeq resultStr)
        
        // 1-element seq
        let resultStr2 = Seq.tail <| seq { yield "a" }      
        Assert.AreEqual(Array.ofSeq (Seq.empty : seq<string>), Array.ofSeq resultStr2)

        CheckThrowsArgumentNullException(fun () -> Seq.tail null |> ignore)
        CheckThrowsArgumentException(fun () -> Seq.tail Seq.empty |> Seq.iter (fun _ -> failwith "Should not be reached"))
        ()

    [<Test>]
    member this.Last() =
             
        let IntSeq =
            seq { for i in 0 .. 9 do
                    yield i }
                    
        if Seq.last IntSeq <> 9 then Assert.Fail()
                 
        // string Seq
        let strSeq = seq ["first"; "second";  "third"]
        if Seq.last strSeq <> "third" then Assert.Fail()
         
        // Empty Seq
        let emptySeq = Seq.empty
        CheckThrowsArgumentException ( fun() -> Seq.last emptySeq)
      
        // null Seq
        let nullSeq:seq<'a> = null
        CheckThrowsArgumentNullException (fun () ->Seq.last nullSeq) 
        () 

    [<Test>]
    member this.TryLast() =
             
        let IntSeq =
            seq { for i in 0 .. 9 -> i }
                    
        let intResult = Seq.tryLast IntSeq
        Assert.AreEqual(9, intResult.Value)
                 
        // string Seq
        let strResult = Seq.tryLast (seq ["first"; "second";  "third"])
        Assert.AreEqual("third", strResult.Value)
         
        // Empty Seq
        let emptyResult = Seq.tryLast Seq.empty
        Assert.IsTrue(emptyResult.IsNone)
      
        // null Seq
        let nullSeq:seq<'a> = null
        CheckThrowsArgumentNullException (fun () ->Seq.tryLast nullSeq |> ignore) 
        () 
        
    [<Test>]
    member this.ExactlyOne() =
             
        let IntSeq =
            seq { for i in 7 .. 7 do
                    yield i }
                    
        if Seq.exactlyOne IntSeq <> 7 then Assert.Fail()
                 
        // string Seq
        let strSeq = seq ["second"]
        if Seq.exactlyOne strSeq <> "second" then Assert.Fail()
         
        // Empty Seq
        let emptySeq = Seq.empty
        CheckThrowsArgumentException ( fun() -> Seq.exactlyOne emptySeq)

        // non-singleton Seq
        let nonSingletonSeq = [ 0 .. 1 ]
        CheckThrowsArgumentException ( fun() -> Seq.exactlyOne nonSingletonSeq |> ignore )

        // null Seq
        let nullSeq:seq<'a> = null
        CheckThrowsArgumentNullException (fun () -> Seq.exactlyOne nullSeq) 
        ()

    [<Test>]
    member this.TryExactlyOne() =
        let IntSeq =
            seq { for i in 7 .. 7 do
                    yield i }

        Assert.AreEqual(Some 7, Seq.tryExactlyOne IntSeq)

        // string Seq
        let strSeq = seq ["second"]
        Assert.AreEqual(Some "second", Seq.tryExactlyOne strSeq)

        // Empty Seq
        let emptySeq = Seq.empty
        Assert.AreEqual(None, Seq.tryExactlyOne emptySeq)

        // non-singleton Seq
        let nonSingletonSeq = [ 0 .. 1 ]
        Assert.AreEqual(None, Seq.tryExactlyOne nonSingletonSeq)

        // null Seq
        let nullSeq:seq<'a> = null
        CheckThrowsArgumentNullException (fun () -> Seq.tryExactlyOne nullSeq |> ignore)
        ()

    [<Test>]
    member this.Init() =

        let funcInt x = x
        let init_finiteInt = Seq.init 9 funcInt
        let expectedIntSeq = seq [ 0..8]
      
        VerifySeqsEqual expectedIntSeq  init_finiteInt
        
             
        // string Seq
        let funcStr x = x.ToString()
        let init_finiteStr = Seq.init 5  funcStr
        let expectedStrSeq = seq ["0";"1";"2";"3";"4"]

        VerifySeqsEqual expectedStrSeq init_finiteStr
        
        // null Seq
        let funcNull x = null
        let init_finiteNull = Seq.init 3 funcNull
        let expectedNullSeq = seq [ null;null;null]
        
        VerifySeqsEqual expectedNullSeq init_finiteNull
        () 
        
    [<Test>]
    member this.InitInfinite() =

        let funcInt x = x
        let init_infiniteInt = Seq.initInfinite funcInt
        let resultint = Seq.find (fun x -> x =100) init_infiniteInt
        
        Assert.AreEqual(100,resultint)
        
             
        // string Seq
        let funcStr x = x.ToString()
        let init_infiniteStr = Seq.initInfinite  funcStr
        let resultstr = Seq.find (fun x -> x = "100") init_infiniteStr
        
        Assert.AreEqual("100",resultstr)
       
       
    [<Test>]
    member this.IsEmpty() =
        
        //seq int
        let seqint = seq [1;2;3]
        let is_emptyInt = Seq.isEmpty seqint
        
        Assert.IsFalse(is_emptyInt)
              
        //seq str
        let seqStr = seq["first";"second"]
        let is_emptyStr = Seq.isEmpty  seqStr

        Assert.IsFalse(is_emptyInt)
        
        //seq empty
        let seqEmpty = Seq.empty
        let is_emptyEmpty = Seq.isEmpty  seqEmpty
        Assert.IsTrue(is_emptyEmpty) 
        
        //seq null
        let seqnull:seq<'a> = null
        CheckThrowsArgumentNullException (fun () -> Seq.isEmpty seqnull |> ignore)
        ()
        
    [<Test>]
    member this.Iter() =
        //seq int
        let seqint =  seq [ 1..3]
        let cacheint = ref 0
       
        let funcint x = cacheint := !cacheint + x
        Seq.iter funcint seqint
        Assert.AreEqual(6,!cacheint)
              
        //seq str
        let seqStr = seq ["first";"second"]
        let cachestr =ref ""
        let funcstr x = cachestr := !cachestr+x
        Seq.iter funcstr seqStr
         
        Assert.AreEqual("firstsecond",!cachestr)
        
         // empty array    
        let emptyseq = Seq.empty
        let resultEpt = ref 0
        Seq.iter (fun x -> Assert.Fail()) emptyseq   

        // null seqay
        let nullseq:seq<'a> =  null
        
        CheckThrowsArgumentNullException (fun () -> Seq.iter funcint nullseq |> ignore)  
        ()
        
    [<Test>]
    member this.Iter2() =
    
        //seq int
        let seqint =  seq [ 1..3]
        let cacheint = ref 0
       
        let funcint x y = cacheint := !cacheint + x+y
        Seq.iter2 funcint seqint seqint
        Assert.AreEqual(12,!cacheint)
              
        //seq str
        let seqStr = seq ["first";"second"]
        let cachestr =ref ""
        let funcstr x y = cachestr := !cachestr+x+y
        Seq.iter2 funcstr seqStr seqStr
         
        Assert.AreEqual("firstfirstsecondsecond",!cachestr)
        
         // empty array    
        let emptyseq = Seq.empty
        let resultEpt = ref 0
        Seq.iter2 (fun x y-> Assert.Fail()) emptyseq  emptyseq 

        // null seqay
        let nullseq:seq<'a> =  null
        CheckThrowsArgumentNullException (fun () -> Seq.iter2 funcint nullseq nullseq |> ignore)  
        
        ()
        
    [<Test>]
    member this.Iteri() =
    
        // seq int
        let seqint =  seq [ 1..10]
        let cacheint = ref 0
       
        let funcint x y = cacheint := !cacheint + x+y
        Seq.iteri funcint seqint
        Assert.AreEqual(100,!cacheint)
              
        // seq str
        let seqStr = seq ["first";"second"]
        let cachestr =ref 0
        let funcstr (x:int) (y:string) = cachestr := !cachestr+ x + y.Length
        Seq.iteri funcstr seqStr
         
        Assert.AreEqual(12,!cachestr)
        
         // empty array    
        let emptyseq = Seq.empty
        let resultEpt = ref 0
        Seq.iteri funcint emptyseq
        Assert.AreEqual(0,!resultEpt)

        // null seqay
        let nullseq:seq<'a> =  null
        CheckThrowsArgumentNullException (fun () -> Seq.iteri funcint nullseq |> ignore)  
        ()

    [<Test>]
    member this.Iteri2() =

        //seq int
        let seqint = seq [ 1..3]
        let cacheint = ref 0
       
        let funcint x y z = cacheint := !cacheint + x + y + z
        Seq.iteri2 funcint seqint seqint
        Assert.AreEqual(15,!cacheint)
              
        //seq str
        let seqStr = seq ["first";"second"]
        let cachestr = ref 0
        let funcstr (x:int) (y:string) (z:string) = cachestr := !cachestr + x + y.Length + z.Length
        Seq.iteri2 funcstr seqStr seqStr
         
        Assert.AreEqual(23,!cachestr)
        
        // empty seq
        let emptyseq = Seq.empty
        let resultEpt = ref 0
        Seq.iteri2 (fun x y z -> Assert.Fail()) emptyseq emptyseq 

        // null seq
        let nullseq:seq<'a> =  null
        CheckThrowsArgumentNullException (fun () -> Seq.iteri2 funcint nullseq nullseq |> ignore)  
        
        // len1 <> len2
        let shorterSeq = seq { 1..3 }
        let longerSeq = seq { 2..2..100 }

        let testSeqLengths seq1 seq2 =
            let cache = ref 0
            let f x y z = cache := !cache + x + y + z
            Seq.iteri2 f seq1 seq2
            !cache

        Assert.AreEqual(21, testSeqLengths shorterSeq longerSeq)
        Assert.AreEqual(21, testSeqLengths longerSeq shorterSeq)

        ()
        
    [<Test>]
    member this.Length() =

         // integer seq  
        let resultInt = Seq.length {1..8}
        if resultInt <> 8 then Assert.Fail()
        
        // string Seq    
        let resultStr = Seq.length (seq ["Lists"; "are";  "commonly" ; "list" ])
        if resultStr <> 4 then Assert.Fail()
        
        // empty Seq     
        let resultEpt = Seq.length Seq.empty
        if resultEpt <> 0 then Assert.Fail()

        // null Seq
        let nullSeq:seq<'a> = null     
        CheckThrowsArgumentNullException (fun () -> Seq.length  nullSeq |> ignore)  
        
        ()
        
    [<Test>]
    member this.Map() =

         // integer Seq
        let funcInt x = 
                match x with
                | _ when x % 2 = 0 -> 10*x            
                | _ -> x
       
        let resultInt = Seq.map funcInt { 1..10 }
        let expectedint = seq [1;20;3;40;5;60;7;80;9;100]
        
        VerifySeqsEqual expectedint resultInt
        
        // string Seq
        let funcStr (x:string) = x.ToLower()
        let resultStr = Seq.map funcStr (seq ["Lists"; "Are";  "Commonly" ; "List" ])
        let expectedSeq = seq ["lists"; "are";  "commonly" ; "list"]
        
        VerifySeqsEqual expectedSeq resultStr
        
        // empty Seq
        let resultEpt = Seq.map funcInt Seq.empty
        VerifySeqsEqual Seq.empty resultEpt

        // null Seq
        let nullSeq:seq<'a> = null 
        CheckThrowsArgumentNullException (fun () -> Seq.map funcStr nullSeq |> ignore)
        
        ()
        
    [<Test>]
    member this.Map2() =
         // integer Seq
        let funcInt x y = x+y
        let resultInt = Seq.map2 funcInt { 1..10 } {2..2..20} 
        let expectedint = seq [3;6;9;12;15;18;21;24;27;30]
        
        VerifySeqsEqual expectedint resultInt
        
        // string Seq
        let funcStr (x:int) (y:string) = x+y.Length
        let resultStr = Seq.map2 funcStr (seq[3;6;9;11]) (seq ["Lists"; "Are";  "Commonly" ; "List" ])
        let expectedSeq = seq [8;9;17;15]
        
        VerifySeqsEqual expectedSeq resultStr
        
        // empty Seq
        let resultEpt = Seq.map2 funcInt Seq.empty Seq.empty
        VerifySeqsEqual Seq.empty resultEpt

        // null Seq
        let nullSeq:seq<'a> = null 
        let validSeq = seq [1]
        CheckThrowsArgumentNullException (fun () -> Seq.map2 funcInt nullSeq validSeq |> ignore)
        
        ()

    [<Test>]
    member this.Map3() = 
        // Integer seq
        let funcInt a b c = (a + b) * c
        let resultInt = Seq.map3 funcInt { 1..8 } { 2..9 } { 3..10 }
        let expectedInt = seq [9; 20; 35; 54; 77; 104; 135; 170]
        VerifySeqsEqual expectedInt resultInt

        // First seq is shorter
        VerifySeqsEqual (seq [9; 20]) (Seq.map3 funcInt { 1..2 } { 2..9 } { 3..10 })
        // Second seq is shorter
        VerifySeqsEqual (seq [9; 20; 35]) (Seq.map3 funcInt { 1..8 } { 2..4 } { 3..10 })
        // Third seq is shorter
        VerifySeqsEqual (seq [9; 20; 35; 54]) (Seq.map3 funcInt { 1..8 } { 2..6 } { 3..6 })

        // String seq
        let funcStr a b c = a + b + c
        let resultStr = Seq.map3 funcStr ["A";"B";"C";"D"] ["a";"b";"c";"d"] ["1";"2";"3";"4"]
        let expectedStr = seq ["Aa1";"Bb2";"Cc3";"Dd4"]
        VerifySeqsEqual expectedStr resultStr

        // Empty seq
        let resultEmpty = Seq.map3 funcStr Seq.empty Seq.empty Seq.empty
        VerifySeqsEqual Seq.empty resultEmpty

        // Null seq
        let nullSeq = null : seq<_>
        let nonNullSeq = seq [1]
        CheckThrowsArgumentNullException (fun () -> Seq.map3 funcInt nullSeq nonNullSeq nullSeq |> ignore)

        ()

    [<Test>]
    member this.MapFold() =
        // integer Seq
        let funcInt acc x = if x % 2 = 0 then 10*x, acc + 1 else x, acc
        let resultInt,resultIntAcc = Seq.mapFold funcInt 100 <| seq { 1..10 }
        VerifySeqsEqual (seq [ 1;20;3;40;5;60;7;80;9;100 ]) resultInt
        Assert.AreEqual(105, resultIntAcc)

        // string Seq
        let funcStr acc (x:string) = match x.Length with 0 -> "empty", acc | _ -> x.ToLower(), sprintf "%s%s" acc x
        let resultStr,resultStrAcc = Seq.mapFold funcStr "" <| seq [ "";"BB";"C";"" ]
        VerifySeqsEqual (seq [ "empty";"bb";"c";"empty" ]) resultStr
        Assert.AreEqual("BBC", resultStrAcc)

        // empty Seq
        let resultEpt,resultEptAcc = Seq.mapFold funcInt 100 Seq.empty
        VerifySeqsEqual Seq.empty resultEpt
        Assert.AreEqual(100, resultEptAcc)

        // null Seq
        let nullArr = null:seq<string>
        CheckThrowsArgumentNullException (fun () -> Seq.mapFold funcStr "" nullArr |> ignore)

        ()

    [<Test>]
    member this.MapFoldBack() =
        // integer Seq
        let funcInt x acc = if acc < 105 then 10*x, acc + 2 else x, acc
        let resultInt,resultIntAcc = Seq.mapFoldBack funcInt (seq { 1..10 }) 100
        VerifySeqsEqual (seq [ 1;2;3;4;5;6;7;80;90;100 ]) resultInt
        Assert.AreEqual(106, resultIntAcc)

        // string Seq
        let funcStr (x:string) acc = match x.Length with 0 -> "empty", acc | _ -> x.ToLower(), sprintf "%s%s" acc x
        let resultStr,resultStrAcc = Seq.mapFoldBack funcStr (seq [ "";"BB";"C";"" ]) ""
        VerifySeqsEqual (seq [ "empty";"bb";"c";"empty" ]) resultStr
        Assert.AreEqual("CBB", resultStrAcc)

        // empty Seq
        let resultEpt,resultEptAcc = Seq.mapFoldBack funcInt Seq.empty 100
        VerifySeqsEqual Seq.empty resultEpt
        Assert.AreEqual(100, resultEptAcc)

        // null Seq
        let nullArr = null:seq<string>
        CheckThrowsArgumentNullException (fun () -> Seq.mapFoldBack funcStr nullArr "" |> ignore)

        ()

    member private this.MapWithSideEffectsTester (map : (int -> int) -> seq<int> -> seq<int>) expectExceptions =
        let i = ref 0
        let f x = i := !i + 1; x*x
        let e = ([1;2] |> map f).GetEnumerator()
        
        if expectExceptions then
            CheckThrowsInvalidOperationExn  (fun _ -> e.Current|>ignore)
            Assert.AreEqual(0, !i)
        if not (e.MoveNext()) then Assert.Fail()
        Assert.AreEqual(1, !i)
        let _ = e.Current
        Assert.AreEqual(1, !i)
        let _ = e.Current
        Assert.AreEqual(1, !i)
        
        if not (e.MoveNext()) then Assert.Fail()
        Assert.AreEqual(2, !i)
        let _ = e.Current
        Assert.AreEqual(2, !i)
        let _ = e.Current
        Assert.AreEqual(2, !i)

        if e.MoveNext() then Assert.Fail()
        Assert.AreEqual(2, !i)
        if expectExceptions then
            CheckThrowsInvalidOperationExn (fun _ -> e.Current |> ignore)
            Assert.AreEqual(2, !i)

        
        i := 0
        let e = ([] |> map f).GetEnumerator()
        if e.MoveNext() then Assert.Fail()
        Assert.AreEqual(0,!i)
        if e.MoveNext() then Assert.Fail()
        Assert.AreEqual(0,!i)
        
        
    member private this.MapWithExceptionTester (map : (int -> int) -> seq<int> -> seq<int>) =
        let raiser x = if x > 0 then raise(NotSupportedException()) else x
        let e = (map raiser [0; 1]).GetEnumerator()
        Assert.IsTrue(e.MoveNext()) // should not throw
        Assert.AreEqual(0, e.Current)
        CheckThrowsNotSupportedException(fun _ -> e.MoveNext() |> ignore)
        Assert.AreEqual(0, e.Current) // should not throw

    [<Test>]
    member this.MapWithSideEffects () =
        this.MapWithSideEffectsTester Seq.map true
        
    [<Test>]
    member this.MapWithException () =
        this.MapWithExceptionTester Seq.map

        
    [<Test>]
    member this.SingletonCollectWithSideEffects () =
        this.MapWithSideEffectsTester (fun f-> Seq.collect (f >> Seq.singleton)) true
        
    [<Test>]
    member this.SingletonCollectWithException () =
        this.MapWithExceptionTester (fun f-> Seq.collect (f >> Seq.singleton))

    [<Test>]
    member this.SystemLinqSelectWithSideEffects () =
        this.MapWithSideEffectsTester (fun f s -> System.Linq.Enumerable.Select(s, Func<_,_>(f))) false
        
    [<Test>]
    member this.SystemLinqSelectWithException () =
        this.MapWithExceptionTester (fun f s -> System.Linq.Enumerable.Select(s, Func<_,_>(f)))
        
    [<Test>]
    member this.MapiWithSideEffects () =
        let i = ref 0
        let f _ x = i := !i + 1; x*x
        let e = ([1;2] |> Seq.mapi f).GetEnumerator()
        
        CheckThrowsInvalidOperationExn  (fun _ -> e.Current|>ignore)
        Assert.AreEqual(0, !i)
        if not (e.MoveNext()) then Assert.Fail()
        Assert.AreEqual(1, !i)
        let _ = e.Current
        Assert.AreEqual(1, !i)
        let _ = e.Current
        Assert.AreEqual(1, !i)
        
        if not (e.MoveNext()) then Assert.Fail()
        Assert.AreEqual(2, !i)
        let _ = e.Current
        Assert.AreEqual(2, !i)
        let _ = e.Current
        Assert.AreEqual(2, !i)
        
        if e.MoveNext() then Assert.Fail()
        Assert.AreEqual(2, !i)
        CheckThrowsInvalidOperationExn  (fun _ -> e.Current|>ignore)
        Assert.AreEqual(2, !i)
        
        i := 0
        let e = ([] |> Seq.mapi f).GetEnumerator()
        if e.MoveNext() then Assert.Fail()
        Assert.AreEqual(0,!i)
        if e.MoveNext() then Assert.Fail()
        Assert.AreEqual(0,!i)
        
    [<Test>]
    member this.Map2WithSideEffects () =
        let i = ref 0
        let f x y = i := !i + 1; x*x
        let e = (Seq.map2 f [1;2] [1;2]).GetEnumerator()
        
        CheckThrowsInvalidOperationExn  (fun _ -> e.Current|>ignore)
        Assert.AreEqual(0, !i)
        if not (e.MoveNext()) then Assert.Fail()
        Assert.AreEqual(1, !i)
        let _ = e.Current
        Assert.AreEqual(1, !i)
        let _ = e.Current
        Assert.AreEqual(1, !i)
        
        if not (e.MoveNext()) then Assert.Fail()
        Assert.AreEqual(2, !i)
        let _ = e.Current
        Assert.AreEqual(2, !i)
        let _ = e.Current
        Assert.AreEqual(2, !i)

        if e.MoveNext() then Assert.Fail()
        Assert.AreEqual(2,!i)
        CheckThrowsInvalidOperationExn  (fun _ -> e.Current|>ignore)
        Assert.AreEqual(2, !i)
        
        i := 0
        let e = (Seq.map2 f [] []).GetEnumerator()
        if e.MoveNext() then Assert.Fail()
        Assert.AreEqual(0,!i)
        if e.MoveNext() then Assert.Fail()
        Assert.AreEqual(0,!i)
        
    [<Test>]
    member this.Mapi2WithSideEffects () =
        let i = ref 0
        let f _ x y = i := !i + 1; x*x
        let e = (Seq.mapi2 f [1;2] [1;2]).GetEnumerator()

        CheckThrowsInvalidOperationExn  (fun _ -> e.Current|>ignore)
        Assert.AreEqual(0, !i)
        if not (e.MoveNext()) then Assert.Fail()
        Assert.AreEqual(1, !i)
        let _ = e.Current
        Assert.AreEqual(1, !i)
        let _ = e.Current
        Assert.AreEqual(1, !i)

        if not (e.MoveNext()) then Assert.Fail()
        Assert.AreEqual(2, !i)
        let _ = e.Current
        Assert.AreEqual(2, !i)
        let _ = e.Current
        Assert.AreEqual(2, !i)

        if e.MoveNext() then Assert.Fail()
        Assert.AreEqual(2,!i)
        CheckThrowsInvalidOperationExn  (fun _ -> e.Current|>ignore)
        Assert.AreEqual(2, !i)

        i := 0
        let e = (Seq.mapi2 f [] []).GetEnumerator()
        if e.MoveNext() then Assert.Fail()
        Assert.AreEqual(0,!i)
        if e.MoveNext() then Assert.Fail()
        Assert.AreEqual(0,!i)

    [<Test>]
    member this.Collect() =
         // integer Seq
        let funcInt x = seq [x+1]
        let resultInt = Seq.collect funcInt { 1..10 } 
       
        let expectedint = seq {2..11}
        
        VerifySeqsEqual expectedint resultInt

        // string Seq
        let funcStr (y:string) = y+"ist"

        let resultStr = Seq.collect funcStr (seq ["L"])

        let expectedSeq = seq ['L';'i';'s';'t']

        VerifySeqsEqual expectedSeq resultStr

        // empty Seq
        let resultEpt = Seq.collect funcInt Seq.empty
        VerifySeqsEqual Seq.empty resultEpt

        // null Seq
        let nullSeq:seq<'a> = null 

        CheckThrowsArgumentNullException (fun () -> Seq.collect funcInt nullSeq |> ignore)

        ()

    [<Test>]
    member this.Mapi() =

         // integer Seq
        let funcInt x y = x+y
        let resultInt = Seq.mapi funcInt { 10..2..20 } 
        let expectedint = seq [10;13;16;19;22;25]
        
        VerifySeqsEqual expectedint resultInt
        
        // string Seq
        let funcStr (x:int) (y:string) =x+y.Length
       
        let resultStr = Seq.mapi funcStr (seq ["Lists"; "Are";  "Commonly" ; "List" ])
        let expectedStr = seq [5;4;10;7]
         
        VerifySeqsEqual expectedStr resultStr
        
        // empty Seq
        let resultEpt = Seq.mapi funcInt Seq.empty
        VerifySeqsEqual Seq.empty resultEpt

        // null Seq
        let nullSeq:seq<'a> = null 
       
        CheckThrowsArgumentNullException (fun () -> Seq.mapi funcInt nullSeq |> ignore)
        
        ()
        
    [<Test>]
    member this.Mapi2() =
         // integer Seq
        let funcInt x y z = x+y+z
        let resultInt = Seq.mapi2 funcInt { 1..10 } {2..2..20}
        let expectedint = seq [3;7;11;15;19;23;27;31;35;39]

        VerifySeqsEqual expectedint resultInt

        // string Seq
        let funcStr (x:int) (y:int) (z:string) = x+y+z.Length
        let resultStr = Seq.mapi2 funcStr (seq[3;6;9;11]) (seq ["Lists"; "Are";  "Commonly" ; "List" ])
        let expectedSeq = seq [8;10;19;18]

        VerifySeqsEqual expectedSeq resultStr

        // empty Seq
        let resultEpt = Seq.mapi2 funcInt Seq.empty Seq.empty
        VerifySeqsEqual Seq.empty resultEpt

        // null Seq
        let nullSeq:seq<'a> = null
        let validSeq = seq [1]
        CheckThrowsArgumentNullException (fun () -> Seq.mapi2 funcInt nullSeq validSeq |> ignore)

        // len1 <> len2
        let shorterSeq = seq { 1..10 }
        let longerSeq = seq { 2..20 }

        let testSeqLengths seq1 seq2 =
            let f x y z = x + y + z
            Seq.mapi2 f seq1 seq2

        VerifySeqsEqual (seq [3;6;9;12;15;18;21;24;27;30]) (testSeqLengths shorterSeq longerSeq)
        VerifySeqsEqual (seq [3;6;9;12;15;18;21;24;27;30]) (testSeqLengths longerSeq shorterSeq)

    [<Test>]
    member this.Indexed() =

         // integer Seq
        let resultInt = Seq.indexed { 10..2..20 }
        let expectedint = seq [(0,10);(1,12);(2,14);(3,16);(4,18);(5,20)]

        VerifySeqsEqual expectedint resultInt

        // string Seq
        let resultStr = Seq.indexed (seq ["Lists"; "Are"; "Commonly"; "List" ])
        let expectedStr = seq [(0,"Lists");(1,"Are");(2,"Commonly");(3,"List")]

        VerifySeqsEqual expectedStr resultStr

        // empty Seq
        let resultEpt = Seq.indexed Seq.empty
        VerifySeqsEqual Seq.empty resultEpt

        // null Seq
        let nullSeq:seq<'a> = null
        CheckThrowsArgumentNullException (fun () -> Seq.indexed nullSeq |> ignore)

        ()

    [<Test>]
    member this.Max() =
         // integer Seq
        let resultInt = Seq.max { 10..20 } 
        
        Assert.AreEqual(20,resultInt)
        
        // string Seq
       
        let resultStr = Seq.max (seq ["Lists"; "Are";  "MaxString" ; "List" ])
        Assert.AreEqual("MaxString",resultStr)
          
        // empty Seq
        CheckThrowsArgumentException(fun () -> Seq.max ( Seq.empty : seq<float>) |> ignore)
        
        // null Seq
        let nullSeq:seq<'a> = null 
        CheckThrowsArgumentNullException (fun () -> Seq.max nullSeq |> ignore)
        
        ()
        
    [<Test>]
    member this.MaxBy() =
    
        // integer Seq
        let funcInt x = x % 8
        let resultInt = Seq.maxBy funcInt { 2..2..20 } 
        Assert.AreEqual(6,resultInt)
        
        // string Seq
        let funcStr (x:string)  =x.Length 
        let resultStr = Seq.maxBy funcStr (seq ["Lists"; "Are";  "Commonly" ; "List" ])
        Assert.AreEqual("Commonly",resultStr)
          
        // empty Seq
        CheckThrowsArgumentException (fun () -> Seq.maxBy funcInt (Seq.empty : seq<int>) |> ignore)
        
        // null Seq
        let nullSeq:seq<'a> = null 
        CheckThrowsArgumentNullException (fun () ->Seq.maxBy funcInt nullSeq |> ignore)
        
        ()
        
    [<Test>]
    member this.MinBy() =
    
        // integer Seq
        let funcInt x = x % 8
        let resultInt = Seq.minBy funcInt { 2..2..20 } 
        Assert.AreEqual(8,resultInt)
        
        // string Seq
        let funcStr (x:string)  =x.Length 
        let resultStr = Seq.minBy funcStr (seq ["Lists"; "Are";  "Commonly" ; "List" ])
        Assert.AreEqual("Are",resultStr)
          
        // empty Seq
        CheckThrowsArgumentException (fun () -> Seq.minBy funcInt (Seq.empty : seq<int>) |> ignore) 
        
        // null Seq
        let nullSeq:seq<'a> = null 
        CheckThrowsArgumentNullException (fun () ->Seq.minBy funcInt nullSeq |> ignore)
        
        ()
        
          
    [<Test>]
    member this.Min() =

         // integer Seq
        let resultInt = Seq.min { 10..20 } 
        Assert.AreEqual(10,resultInt)
        
        // string Seq
        let resultStr = Seq.min (seq ["Lists"; "Are";  "minString" ; "List" ])
        Assert.AreEqual("Are",resultStr)
          
        // empty Seq
        CheckThrowsArgumentException (fun () -> Seq.min (Seq.empty : seq<int>) |> ignore) 
        
        // null Seq
        let nullSeq:seq<'a> = null 
        CheckThrowsArgumentNullException (fun () -> Seq.min nullSeq |> ignore)
        
        ()

    [<Test>]
    member this.Item() =
         // integer Seq
        let resultInt = Seq.item 3 { 10..20 }
        Assert.AreEqual(13, resultInt)

        // string Seq
        let resultStr = Seq.item 2 (seq ["Lists"; "Are"; "Cool" ; "List" ])
        Assert.AreEqual("Cool", resultStr)

        // empty Seq
        CheckThrowsArgumentException(fun () -> Seq.item 0 (Seq.empty : seq<decimal>) |> ignore)

        // null Seq
        let nullSeq:seq<'a> = null
        CheckThrowsArgumentNullException (fun () ->Seq.item 3 nullSeq |> ignore)

        // Negative index
        for i = -1 downto -10 do
           CheckThrowsArgumentException (fun () -> Seq.item i { 10 .. 20 } |> ignore)

        // Out of range
        for i = 11 to 20 do
           CheckThrowsArgumentException (fun () -> Seq.item i { 10 .. 20 } |> ignore)

    [<Test>]
    member this.``item should fail with correct number of missing elements``() =
        try
            Seq.item 0 (Array.zeroCreate<int> 0) |> ignore
            failwith "error expected"
        with
        | exn when exn.Message.Contains("seq was short by 1 element") -> ()

        try
            Seq.item 2 (Array.zeroCreate<int> 0) |> ignore
            failwith "error expected"
        with
        | exn when exn.Message.Contains("seq was short by 3 elements") -> ()

    [<Test>]
    member this.Of_Array() =
         // integer Seq
        let resultInt = Seq.ofArray [|1..10|]
        let expectedInt = {1..10}
         
        VerifySeqsEqual expectedInt resultInt
        
        // string Seq
        let resultStr = Seq.ofArray [|"Lists"; "Are";  "ofArrayString" ; "List" |]
        let expectedStr = seq ["Lists"; "Are";  "ofArrayString" ; "List" ]
        VerifySeqsEqual expectedStr resultStr
          
        // empty Seq 
        let resultEpt = Seq.ofArray [| |] 
        VerifySeqsEqual resultEpt Seq.empty
       
        ()
        
    [<Test>]
    member this.Of_List() =
         // integer Seq
        let resultInt = Seq.ofList [1..10]
        let expectedInt = {1..10}
         
        VerifySeqsEqual expectedInt resultInt
        
        // string Seq
       
        let resultStr =Seq.ofList ["Lists"; "Are";  "ofListString" ; "List" ]
        let expectedStr = seq ["Lists"; "Are";  "ofListString" ; "List" ]
        VerifySeqsEqual expectedStr resultStr
          
        // empty Seq 
        let resultEpt = Seq.ofList [] 
        VerifySeqsEqual resultEpt Seq.empty
        ()
        
          
    [<Test>]
    member this.Pairwise() =
         // integer Seq
        let resultInt = Seq.pairwise {1..3}
       
        let expectedInt = seq [1,2;2,3]
         
        VerifySeqsEqual expectedInt resultInt
        
        // string Seq
        let resultStr =Seq.pairwise ["str1"; "str2";"str3" ]
        let expectedStr = seq ["str1","str2";"str2","str3"]
        VerifySeqsEqual expectedStr resultStr
          
        // empty Seq 
        let resultEpt = Seq.pairwise [] 
        VerifySeqsEqual resultEpt Seq.empty
       
        ()
        
    [<Test>]
    member this.Reduce() =
         
        // integer Seq
        let resultInt = Seq.reduce (fun x y -> x/y) (seq [5*4*3*2; 4;3;2;1])
        Assert.AreEqual(5,resultInt)
        
        // string Seq
        let resultStr = Seq.reduce (fun (x:string) (y:string) -> x.Remove(0,y.Length)) (seq ["ABCDE";"A"; "B";  "C" ; "D" ])
        Assert.AreEqual("E",resultStr) 
       
        // empty Seq 
        CheckThrowsArgumentException (fun () -> Seq.reduce (fun x y -> x/y)  Seq.empty |> ignore)
        
        // null Seq
        let nullSeq : seq<'a> = null
        CheckThrowsArgumentNullException (fun () -> Seq.reduce (fun (x:string) (y:string) -> x.Remove(0,y.Length))  nullSeq  |> ignore)   
        ()

    [<Test>]
    member this.ReduceBack() =
        // int Seq
        let funcInt x y = x - y
        let IntSeq = seq { 1..4 }
        let reduceInt = Seq.reduceBack funcInt IntSeq
        Assert.AreEqual((1-(2-(3-4))), reduceInt)

        // string Seq
        let funcStr (x:string) (y:string) = y.Remove(0,x.Length)
        let strSeq = seq [ "A"; "B"; "C"; "D" ; "ABCDE" ]
        let reduceStr = Seq.reduceBack  funcStr strSeq
        Assert.AreEqual("E", reduceStr)
        
        // string Seq
        let funcStr2 elem acc = sprintf "%s%s" elem acc
        let strSeq2 = seq [ "A" ]
        let reduceStr2 = Seq.reduceBack  funcStr2 strSeq2
        Assert.AreEqual("A", reduceStr2)

        // Empty Seq
        CheckThrowsArgumentException (fun () -> Seq.reduceBack funcInt Seq.empty |> ignore)

        // null Seq
        let nullSeq:seq<'a> = null
        CheckThrowsArgumentNullException (fun () -> Seq.reduceBack funcInt nullSeq |> ignore)

        ()

    [<Test>]
    member this.Rev() =
        // integer Seq
        let resultInt = Seq.rev (seq [5;4;3;2;1])
        VerifySeqsEqual (seq[1;2;3;4;5]) resultInt

        // string Seq
        let resultStr = Seq.rev (seq ["A"; "B";  "C" ; "D" ])
        VerifySeqsEqual (seq["D";"C";"B";"A"]) resultStr

        // empty Seq
        VerifySeqsEqual Seq.empty (Seq.rev Seq.empty)

        // null Seq
        let nullSeq : seq<'a> = null
        CheckThrowsArgumentNullException (fun () -> Seq.rev nullSeq  |> ignore)
        ()

    [<Test>]
    member this.Scan() =
        // integer Seq
        let funcInt x y = x+y
        let resultInt = Seq.scan funcInt 9 {1..10}
        let expectedInt = seq [9;10;12;15;19;24;30;37;45;54;64]
        VerifySeqsEqual expectedInt resultInt
        
        // string Seq
        let funcStr x y = x+y
        let resultStr =Seq.scan funcStr "x" ["str1"; "str2";"str3" ]
        
        let expectedStr = seq ["x";"xstr1"; "xstr1str2";"xstr1str2str3"]
        VerifySeqsEqual expectedStr resultStr
          
        // empty Seq 
        let resultEpt = Seq.scan funcInt 5 Seq.empty 
       
        VerifySeqsEqual resultEpt (seq [ 5])
       
        // null Seq
        let seqNull:seq<'a> = null
        CheckThrowsArgumentNullException(fun() -> Seq.scan funcInt 5 seqNull |> ignore)
        ()
        
    [<Test>]
    member this.ScanBack() =
        // integer Seq
        let funcInt x y = x+y
        let resultInt = Seq.scanBack funcInt { 1..10 } 9
        let expectedInt = seq [64;63;61;58;54;49;43;36;28;19;9]
        VerifySeqsEqual expectedInt resultInt

        // string Seq
        let funcStr x y = x+y
        let resultStr = Seq.scanBack funcStr (seq ["A";"B";"C";"D"]) "X"
        let expectedStr = seq ["ABCDX";"BCDX";"CDX";"DX";"X"]
        VerifySeqsEqual expectedStr resultStr

        // empty Seq
        let resultEpt = Seq.scanBack funcInt Seq.empty 5
        let expectedEpt = seq [5]
        VerifySeqsEqual expectedEpt resultEpt

        // null Seq
        let seqNull:seq<'a> = null
        CheckThrowsArgumentNullException(fun() -> Seq.scanBack funcInt seqNull 5 |> ignore)

        // exception cases
        let funcEx x (s:'State) = raise <| new System.FormatException() : 'State
        // calling scanBack with funcEx does not throw
        let resultEx = Seq.scanBack funcEx (seq {1..10}) 0
        // reading from resultEx throws
        CheckThrowsFormatException(fun() -> Seq.head resultEx |> ignore)

        // Result consumes entire input sequence as soon as it is accesses an element
        let i = ref 0
        let funcState x s = (i := !i + x); x+s
        let resultState = Seq.scanBack funcState (seq {1..3}) 0
        Assert.AreEqual(0, !i)
        use e = resultState.GetEnumerator()
        Assert.AreEqual(6, !i)

        ()

    [<Test>]
    member this.Singleton() =
        // integer Seq
        let resultInt = Seq.singleton 1
       
        let expectedInt = seq [1]
        VerifySeqsEqual expectedInt resultInt
        
        // string Seq
        let resultStr =Seq.singleton "str1"
        let expectedStr = seq ["str1"]
        VerifySeqsEqual expectedStr resultStr
         
        // null Seq
        let resultNull = Seq.singleton null
        let expectedNull = seq [null]
        VerifySeqsEqual expectedNull resultNull
        ()
    
        
    [<Test>]
    member this.Skip() =
    
        // integer Seq
        let resultInt = Seq.skip 2 (seq [1;2;3;4])
        let expectedInt = seq [3;4]
        VerifySeqsEqual expectedInt resultInt
        
        // string Seq
        let resultStr =Seq.skip 2 (seq ["str1";"str2";"str3";"str4"])
        let expectedStr = seq ["str3";"str4"]
        VerifySeqsEqual expectedStr resultStr
        
        // empty Seq 
        let resultEpt = Seq.skip 0 Seq.empty 
        VerifySeqsEqual resultEpt Seq.empty
        
         
        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.skip 1 null |> ignore)
        ()
       
    [<Test>]
    member this.Skip_While() =
    
        // integer Seq
        let funcInt x = (x < 3)
        let resultInt = Seq.skipWhile funcInt (seq [1;2;3;4;5;6])
        let expectedInt = seq [3;4;5;6]
        VerifySeqsEqual expectedInt resultInt
        
        // string Seq
        let funcStr (x:string) = x.Contains(".")
        let resultStr =Seq.skipWhile funcStr (seq [".";"asdfasdf.asdfasdf";"";"";"";"";"";"";"";"";""])
        let expectedStr = seq ["";"";"";"";"";"";"";"";""]
        VerifySeqsEqual expectedStr resultStr
        
        // empty Seq 
        let resultEpt = Seq.skipWhile funcInt Seq.empty 
        VerifySeqsEqual resultEpt Seq.empty
        
        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.skipWhile funcInt null |> ignore)
        ()
       
    [<Test>]
    member this.Sort() =

        // integer Seq
        let resultInt = Seq.sort (seq [1;3;2;4;6;5;7])
        let expectedInt = {1..7}
        VerifySeqsEqual expectedInt resultInt
        
        // string Seq
       
        let resultStr =Seq.sort (seq ["str1";"str3";"str2";"str4"])
        let expectedStr = seq ["str1";"str2";"str3";"str4"]
        VerifySeqsEqual expectedStr resultStr
        
        // empty Seq 
        let resultEpt = Seq.sort Seq.empty 
        VerifySeqsEqual resultEpt Seq.empty
         
        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.sort null  |> ignore)
        ()
        
    [<Test>]
    member this.SortBy() =

        // integer Seq
        let funcInt x = Math.Abs(x-5)
        let resultInt = Seq.sortBy funcInt (seq [1;2;4;5;7])
        let expectedInt = seq [5;4;7;2;1]
        VerifySeqsEqual expectedInt resultInt
        
        // string Seq
        let funcStr (x:string) = x.IndexOf("key")
        let resultStr =Seq.sortBy funcStr (seq ["st(key)r";"str(key)";"s(key)tr";"(key)str"])
        
        let expectedStr = seq ["(key)str";"s(key)tr";"st(key)r";"str(key)"]
        VerifySeqsEqual expectedStr resultStr
        
        // empty Seq 
        let resultEpt = Seq.sortBy funcInt Seq.empty 
        VerifySeqsEqual resultEpt Seq.empty
         
        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.sortBy funcInt null  |> ignore)
        ()

    [<Test>]
    member this.SortDescending() =

        // integer Seq
        let resultInt = Seq.sortDescending (seq [1;3;2;Int32.MaxValue;4;6;Int32.MinValue;5;7;0])
        let expectedInt = seq{
            yield Int32.MaxValue;
            yield! seq{ 7..-1..0 }
            yield Int32.MinValue
        }
        VerifySeqsEqual expectedInt resultInt
        
        // string Seq
       
        let resultStr = Seq.sortDescending (seq ["str1";null;"str3";"";"Str1";"str2";"str4"])
        let expectedStr = seq ["str4";"str3";"str2";"str1";"Str1";"";null]
        VerifySeqsEqual expectedStr resultStr
        
        // empty Seq 
        let resultEpt = Seq.sortDescending Seq.empty 
        VerifySeqsEqual resultEpt Seq.empty

        // tuple Seq
        let tupSeq = (seq[(2,"a");(1,"d");(1,"b");(1,"a");(2,"x");(2,"b");(1,"x")])
        let resultTup = Seq.sortDescending tupSeq
        let expectedTup = (seq[(2,"x");(2,"b");(2,"a");(1,"x");(1,"d");(1,"b");(1,"a")])   
        VerifySeqsEqual  expectedTup resultTup
         
        // float Seq
        let minFloat,maxFloat,epsilon = System.Double.MinValue,System.Double.MaxValue,System.Double.Epsilon
        let floatSeq = seq [0.0; 0.5; 2.0; 1.5; 1.0; minFloat;maxFloat;epsilon;-epsilon]
        let resultFloat = Seq.sortDescending floatSeq
        let expectedFloat = seq [maxFloat; 2.0; 1.5; 1.0; 0.5; epsilon; 0.0; -epsilon; minFloat; ]
        VerifySeqsEqual expectedFloat resultFloat

        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.sort null  |> ignore)
        ()
        
    [<Test>]
    member this.SortByDescending() =

        // integer Seq
        let funcInt x = Math.Abs(x-5)
        let resultInt = Seq.sortByDescending funcInt (seq [1;2;4;5;7])
        let expectedInt = seq [1;2;7;4;5]
        VerifySeqsEqual expectedInt resultInt
        
        // string Seq
        let funcStr (x:string) = x.IndexOf("key")
        let resultStr =Seq.sortByDescending funcStr (seq ["st(key)r";"str(key)";"s(key)tr";"(key)str"])
        
        let expectedStr = seq ["str(key)";"st(key)r";"s(key)tr";"(key)str"]
        VerifySeqsEqual expectedStr resultStr
        
        // empty Seq 
        let resultEpt = Seq.sortByDescending funcInt Seq.empty 
        VerifySeqsEqual resultEpt Seq.empty

        // tuple Seq
        let tupSeq = (seq[(2,"a");(1,"d");(1,"b");(1,"a");(2,"x");(2,"b");(1,"x")])
        let resultTup = Seq.sortByDescending snd tupSeq         
        let expectedTup = (seq[(2,"x");(1,"x");(1,"d");(1,"b");(2,"b");(2,"a");(1,"a")])
        VerifySeqsEqual  expectedTup resultTup
         
        // float Seq
        let minFloat,maxFloat,epsilon = System.Double.MinValue,System.Double.MaxValue,System.Double.Epsilon
        let floatSeq = seq [0.0; 0.5; 2.0; 1.5; 1.0; minFloat;maxFloat;epsilon;-epsilon]
        let resultFloat = Seq.sortByDescending id floatSeq
        let expectedFloat = seq [maxFloat; 2.0; 1.5; 1.0; 0.5; epsilon; 0.0; -epsilon; minFloat; ]
        VerifySeqsEqual expectedFloat resultFloat

        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.sortByDescending funcInt null  |> ignore)
        ()
        
    member this.SortWith() =

        // integer Seq
        let intComparer a b = compare (a%3) (b%3)
        let resultInt = Seq.sortWith intComparer (seq {0..10})
        let expectedInt = seq [0;3;6;9;1;4;7;10;2;5;8]
        VerifySeqsEqual expectedInt resultInt

        // string Seq
        let resultStr = Seq.sortWith compare (seq ["str1";"str3";"str2";"str4"])
        let expectedStr = seq ["str1";"str2";"str3";"str4"]
        VerifySeqsEqual expectedStr resultStr

        // empty Seq
        let resultEpt = Seq.sortWith intComparer Seq.empty
        VerifySeqsEqual resultEpt Seq.empty

        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.sortWith intComparer null  |> ignore)

        ()

    [<Test>]
    member this.Sum() =
    
        // integer Seq
        let resultInt = Seq.sum (seq [1..10])
        Assert.AreEqual(55,resultInt)
        
        // float32 Seq
        let floatSeq = (seq [ 1.2f;3.5f;6.7f ])
        let resultFloat = Seq.sum floatSeq
        if resultFloat <> 11.4f then Assert.Fail()
        
        // double Seq
        let doubleSeq = (seq [ 1.0;8.0 ])
        let resultDouble = Seq.sum doubleSeq
        if resultDouble <> 9.0 then Assert.Fail()
        
        // decimal Seq
        let decimalSeq = (seq [ 0M;19M;19.03M ])
        let resultDecimal = Seq.sum decimalSeq
        if resultDecimal <> 38.03M then Assert.Fail()      
          
      
        // empty float32 Seq
        let emptyFloatSeq = Seq.empty<System.Single> 
        let resultEptFloat = Seq.sum emptyFloatSeq 
        if resultEptFloat <> 0.0f then Assert.Fail()
        
        // empty double Seq
        let emptyDoubleSeq = Seq.empty<System.Double> 
        let resultDouEmp = Seq.sum emptyDoubleSeq 
        if resultDouEmp <> 0.0 then Assert.Fail()
        
        // empty decimal Seq
        let emptyDecimalSeq = Seq.empty<System.Decimal> 
        let resultDecEmp = Seq.sum emptyDecimalSeq 
        if resultDecEmp <> 0M then Assert.Fail()
       
        ()
        
    [<Test>]
    member this.SumBy() =

        // integer Seq
        let resultInt = Seq.sumBy int (seq [1..10])
        Assert.AreEqual(55,resultInt)
        
        // float32 Seq
        let floatSeq = (seq [ 1.2f;3.5f;6.7f ])
        let resultFloat = Seq.sumBy float32 floatSeq
        if resultFloat <> 11.4f then Assert.Fail()
        
        // double Seq
        let doubleSeq = (seq [ 1.0;8.0 ])
        let resultDouble = Seq.sumBy double doubleSeq
        if resultDouble <> 9.0 then Assert.Fail()
        
        // decimal Seq
        let decimalSeq = (seq [ 0M;19M;19.03M ])
        let resultDecimal = Seq.sumBy decimal decimalSeq
        if resultDecimal <> 38.03M then Assert.Fail()      

        // empty float32 Seq
        let emptyFloatSeq = Seq.empty<System.Single> 
        let resultEptFloat = Seq.sumBy float32 emptyFloatSeq 
        if resultEptFloat <> 0.0f then Assert.Fail()
        
        // empty double Seq
        let emptyDoubleSeq = Seq.empty<System.Double> 
        let resultDouEmp = Seq.sumBy double emptyDoubleSeq 
        if resultDouEmp <> 0.0 then Assert.Fail()
        
        // empty decimal Seq
        let emptyDecimalSeq = Seq.empty<System.Decimal> 
        let resultDecEmp = Seq.sumBy decimal emptyDecimalSeq 
        if resultDecEmp <> 0M then Assert.Fail()
       
        ()
        
    [<Test>]
    member this.Take() =
        // integer Seq
        
        let resultInt = Seq.take 3 (seq [1;2;4;5;7])
       
        let expectedInt = seq [1;2;4]
        VerifySeqsEqual expectedInt resultInt
        
        // string Seq
       
        let resultStr =Seq.take 2(seq ["str1";"str2";"str3";"str4"])
     
        let expectedStr = seq ["str1";"str2"]
        VerifySeqsEqual expectedStr resultStr
        
        // empty Seq 
        let resultEpt = Seq.take 0 Seq.empty 
      
        VerifySeqsEqual resultEpt Seq.empty
        
         
        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.take 1 null |> ignore)
        ()
        
    [<Test>]
    member this.takeWhile() =
        // integer Seq
        let funcInt x = (x < 6)
        let resultInt = Seq.takeWhile funcInt (seq [1;2;4;5;6;7])
      
        let expectedInt = seq [1;2;4;5]
        VerifySeqsEqual expectedInt resultInt
        
        // string Seq
        let funcStr (x:string) = (x.Length < 4)
        let resultStr =Seq.takeWhile funcStr (seq ["a"; "ab"; "abc"; "abcd"; "abcde"])
      
        let expectedStr = seq ["a"; "ab"; "abc"]
        VerifySeqsEqual expectedStr resultStr
        
        // empty Seq 
        let resultEpt = Seq.takeWhile funcInt Seq.empty 
        VerifySeqsEqual resultEpt Seq.empty
        
        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.takeWhile funcInt null |> ignore)
        ()
        
    [<Test>]
    member this.ToArray() =
        // integer Seq
        let resultInt = Seq.toArray(seq [1;2;4;5;7])
     
        let expectedInt = [|1;2;4;5;7|]
        Assert.AreEqual(expectedInt,resultInt)

        // string Seq
        let resultStr =Seq.toArray (seq ["str1";"str2";"str3"])
    
        let expectedStr =  [|"str1";"str2";"str3"|]
        Assert.AreEqual(expectedStr,resultStr)
        
        // empty Seq 
        let resultEpt = Seq.toArray Seq.empty 
        Assert.AreEqual([||],resultEpt)
        
        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.toArray null |> ignore)
        ()
        
    [<Test>]    
    member this.ToArrayFromICollection() =
        let inputCollection = ResizeArray(seq [1;2;4;5;7])
        let resultInt = Seq.toArray(inputCollection)
        let expectedInt = [|1;2;4;5;7|]
        Assert.AreEqual(expectedInt,resultInt)        
    
    [<Test>]    
    member this.ToArrayEmptyInput() =
        let resultInt = Seq.toArray(Seq.empty<int>)
        let expectedInt = Array.empty<int>
        Assert.AreEqual(expectedInt,resultInt)        

    [<Test>]    
    member this.ToArrayFromArray() =
        let resultInt = Seq.toArray([|1;2;4;5;7|])
        let expectedInt = [|1;2;4;5;7|]
        Assert.AreEqual(expectedInt,resultInt)        
    
    [<Test>]    
    member this.ToArrayFromList() =
        let resultInt = Seq.toArray([1;2;4;5;7])
        let expectedInt = [|1;2;4;5;7|]
        Assert.AreEqual(expectedInt,resultInt)        

    [<Test>]
    member this.ToList() =
        // integer Seq
        let resultInt = Seq.toList (seq [1;2;4;5;7])
        let expectedInt = [1;2;4;5;7]
        Assert.AreEqual(expectedInt,resultInt)
        
        // string Seq
        let resultStr =Seq.toList (seq ["str1";"str2";"str3"])
        let expectedStr =  ["str1";"str2";"str3"]
        Assert.AreEqual(expectedStr,resultStr)
        
        // empty Seq 
        let resultEpt = Seq.toList Seq.empty 
        Assert.AreEqual([],resultEpt)
         
        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.toList null |> ignore)
        ()

    [<Test>]
    member this.Transpose() =
        // integer seq
        VerifySeqsEqual [seq [1; 4]; seq [2; 5]; seq [3; 6]] <| Seq.transpose (seq [seq {1..3}; seq {4..6}])
        VerifySeqsEqual [seq [1]; seq [2]; seq [3]] <| Seq.transpose (seq [seq {1..3}])
        VerifySeqsEqual [seq {1..2}] <| Seq.transpose (seq [seq [1]; seq [2]])

        // string seq
        VerifySeqsEqual [seq ["a";"d"]; seq ["b";"e"]; seq ["c";"f"]] <| Seq.transpose (seq [seq ["a";"b";"c"]; seq ["d";"e";"f"]])

        // empty seq
        VerifySeqsEqual Seq.empty <| Seq.transpose Seq.empty

        // seq of empty seqs - m x 0 seq transposes to 0 x m (i.e. empty)
        VerifySeqsEqual Seq.empty <| Seq.transpose (seq [Seq.empty])
        VerifySeqsEqual Seq.empty <| Seq.transpose (seq [Seq.empty; Seq.empty])

        // null seq
        let nullSeq = null : seq<seq<string>>
        CheckThrowsArgumentNullException (fun () -> Seq.transpose nullSeq |> ignore)

        // sequences of lists
        VerifySeqsEqual [seq ["a";"c"]; seq ["b";"d"]] <| Seq.transpose [["a";"b"]; ["c";"d"]]
        VerifySeqsEqual [seq ["a";"c"]; seq ["b";"d"]] <| Seq.transpose (seq { yield ["a";"b"]; yield ["c";"d"] })

    [<Test>]
    member this.Truncate() =
        // integer Seq
        let resultInt = Seq.truncate 3 (seq [1;2;4;5;7])
        let expectedInt = [1;2;4]
        VerifySeqsEqual expectedInt resultInt
        
        // string Seq
        let resultStr =Seq.truncate 2 (seq ["str1";"str2";"str3"])
        let expectedStr =  ["str1";"str2"]
        VerifySeqsEqual expectedStr resultStr
        
        // empty Seq 
        let resultEpt = Seq.truncate 0 Seq.empty
        VerifySeqsEqual Seq.empty resultEpt
        
        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.truncate 1 null |> ignore)

        // negative count
        VerifySeqsEqual Seq.empty <| Seq.truncate -1 (seq [1;2;4;5;7])
        VerifySeqsEqual Seq.empty <| Seq.truncate System.Int32.MinValue (seq [1;2;4;5;7])

        ()
        
    [<Test>]
    member this.tryFind() =
        // integer Seq
        let resultInt = Seq.tryFind (fun x -> (x%2=0)) (seq [1;2;4;5;7])
        Assert.AreEqual(Some(2), resultInt)
        
         // integer Seq - None
        let resultInt = Seq.tryFind (fun x -> (x%2=0)) (seq [1;3;5;7])
        Assert.AreEqual(None, resultInt)
        
        // string Seq
        let resultStr = Seq.tryFind (fun (x:string) -> x.Contains("2")) (seq ["str1";"str2";"str3"])
        Assert.AreEqual(Some("str2"),resultStr)
        
         // string Seq - None
        let resultStr = Seq.tryFind (fun (x:string) -> x.Contains("2")) (seq ["str1";"str4";"str3"])
        Assert.AreEqual(None,resultStr)
       
        
        // empty Seq 
        let resultEpt = Seq.tryFind (fun x -> (x%2=0)) Seq.empty
        Assert.AreEqual(None,resultEpt)

        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.tryFind (fun x -> (x%2=0))  null |> ignore)
        ()
        
    [<Test>]
    member this.TryFindBack() =
        // integer Seq
        let resultInt = Seq.tryFindBack (fun x -> (x%2=0)) (seq [1;2;4;5;7])
        Assert.AreEqual(Some 4, resultInt)

        // integer Seq - None
        let resultInt = Seq.tryFindBack (fun x -> (x%2=0)) (seq [1;3;5;7])
        Assert.AreEqual(None, resultInt)

        // string Seq
        let resultStr = Seq.tryFindBack (fun (x:string) -> x.Contains("2")) (seq ["str1";"str2";"str2x";"str3"])
        Assert.AreEqual(Some "str2x", resultStr)

        // string Seq - None
        let resultStr = Seq.tryFindBack (fun (x:string) -> x.Contains("2")) (seq ["str1";"str4";"str3"])
        Assert.AreEqual(None, resultStr)

        // empty Seq
        let resultEpt = Seq.tryFindBack (fun x -> (x%2=0)) Seq.empty
        Assert.AreEqual(None, resultEpt)

        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.tryFindBack (fun x -> (x%2=0))  null |> ignore)
        ()

    [<Test>]
    member this.TryFindIndex() =

        // integer Seq
        let resultInt = Seq.tryFindIndex (fun x -> (x % 5 = 0)) [8; 9; 10]
        Assert.AreEqual(Some(2), resultInt)
        
         // integer Seq - None
        let resultInt = Seq.tryFindIndex (fun x -> (x % 5 = 0)) [9;3;11]
        Assert.AreEqual(None, resultInt)
        
        // string Seq
        let resultStr = Seq.tryFindIndex (fun (x:string) -> x.Contains("2")) ["str1"; "str2"; "str3"]
        Assert.AreEqual(Some(1),resultStr)
        
         // string Seq - None
        let resultStr = Seq.tryFindIndex (fun (x:string) -> x.Contains("2")) ["str1"; "str4"; "str3"]
        Assert.AreEqual(None,resultStr)
       
        
        // empty Seq 
        let resultEpt = Seq.tryFindIndex (fun x -> (x%2=0)) Seq.empty
        Assert.AreEqual(None, resultEpt)
        
        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.tryFindIndex (fun x -> (x % 2 = 0))  null |> ignore)
        ()
        
    [<Test>]
    member this.TryFindIndexBack() =

        // integer Seq
        let resultInt = Seq.tryFindIndexBack (fun x -> (x % 5 = 0)) [5; 9; 10; 12]
        Assert.AreEqual(Some(2), resultInt)

        // integer Seq - None
        let resultInt = Seq.tryFindIndexBack (fun x -> (x % 5 = 0)) [9;3;11]
        Assert.AreEqual(None, resultInt)

        // string Seq
        let resultStr = Seq.tryFindIndexBack (fun (x:string) -> x.Contains("2")) ["str1"; "str2"; "str2x"; "str3"]
        Assert.AreEqual(Some(2), resultStr)

        // string Seq - None
        let resultStr = Seq.tryFindIndexBack (fun (x:string) -> x.Contains("2")) ["str1"; "str4"; "str3"]
        Assert.AreEqual(None, resultStr)

        // empty Seq
        let resultEpt = Seq.tryFindIndexBack (fun x -> (x%2=0)) Seq.empty
        Assert.AreEqual(None, resultEpt)

        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.tryFindIndexBack (fun x -> (x % 2 = 0))  null |> ignore)
        ()

    [<Test>]
    member this.Unfold() =
        // integer Seq
        
        let resultInt = Seq.unfold (fun x -> if x = 1 then Some(7,2) else  None) 1
        
        VerifySeqsEqual (seq [7]) resultInt
          
        // string Seq
        let resultStr =Seq.unfold (fun (x:string) -> if x.Contains("unfold") then Some("a","b") else None) "unfold"
        VerifySeqsEqual (seq ["a"]) resultStr
        ()
        
        
    [<Test>]
    member this.Windowed() =

        let testWindowed config =
            try
                config.InputSeq
                |> Seq.windowed config.WindowSize
                |> VerifySeqsEqual config.ExpectedSeq 
            with
            | _ when Option.isNone config.Exception -> Assert.Fail()
            | e when e.GetType() = (Option.get config.Exception) -> ()
            | _ -> Assert.Fail()

        {
          InputSeq = seq [1..10]
          WindowSize = 1
          ExpectedSeq =  seq { for i in 1..10 do yield [| i |] }
          Exception = None
        } |> testWindowed
        {
          InputSeq = seq [1..10]
          WindowSize = 5
          ExpectedSeq =  seq { for i in 1..6 do yield [| i; i+1; i+2; i+3; i+4 |] }
          Exception = None
        } |> testWindowed
        {
          InputSeq = seq [1..10]
          WindowSize = 10
          ExpectedSeq =  seq { yield [| 1 .. 10 |] }
          Exception = None
        } |> testWindowed
        {
          InputSeq = seq [1..10]
          WindowSize = 25
          ExpectedSeq =  Seq.empty
          Exception = None
        } |> testWindowed
        {
          InputSeq = seq ["str1";"str2";"str3";"str4"]
          WindowSize = 2
          ExpectedSeq =  seq [ [|"str1";"str2"|];[|"str2";"str3"|];[|"str3";"str4"|]]
          Exception = None
        } |> testWindowed
        {
          InputSeq = Seq.empty
          WindowSize = 2
          ExpectedSeq = Seq.empty
          Exception = None
        } |> testWindowed
        {
          InputSeq = null
          WindowSize = 2
          ExpectedSeq = Seq.empty
          Exception = Some typeof<ArgumentNullException>
        } |> testWindowed
        {
          InputSeq = seq [1..10]
          WindowSize = 0
          ExpectedSeq =  Seq.empty
          Exception = Some typeof<ArgumentException>
        } |> testWindowed

        ()
        
    [<Test>]
    member this.Zip() =
    
        // integer Seq
        let resultInt = Seq.zip (seq [1..7]) (seq [11..17])
        let expectedInt = 
            seq { for i in 1..7 do
                    yield i, i+10 }
        VerifySeqsEqual expectedInt resultInt
        
        // string Seq
        let resultStr =Seq.zip (seq ["str3";"str4"]) (seq ["str1";"str2"])
        let expectedStr = seq ["str3","str1";"str4","str2"]
        VerifySeqsEqual expectedStr resultStr
      
        // empty Seq 
        let resultEpt = Seq.zip Seq.empty Seq.empty
        VerifySeqsEqual Seq.empty resultEpt
          
        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.zip null null |> ignore)
        CheckThrowsArgumentNullException(fun() -> Seq.zip null (seq [1..7]) |> ignore)
        CheckThrowsArgumentNullException(fun() -> Seq.zip (seq [1..7]) null |> ignore)
        ()
        
    [<Test>]
    member this.Zip3() =
        // integer Seq
        let resultInt = Seq.zip3 (seq [1..7]) (seq [11..17]) (seq [21..27])
        let expectedInt = 
            seq { for i in 1..7 do
                    yield i, (i + 10), (i + 20) }
        VerifySeqsEqual expectedInt resultInt
        
        // string Seq
        let resultStr =Seq.zip3 (seq ["str1";"str2"]) (seq ["str11";"str12"]) (seq ["str21";"str22"])
        let expectedStr = seq ["str1","str11","str21";"str2","str12","str22" ]
        VerifySeqsEqual expectedStr resultStr
      
        // empty Seq 
        let resultEpt = Seq.zip3 Seq.empty Seq.empty Seq.empty
        VerifySeqsEqual Seq.empty resultEpt
          
        // null Seq
        CheckThrowsArgumentNullException(fun() -> Seq.zip3 null null null |> ignore)
        CheckThrowsArgumentNullException(fun() -> Seq.zip3 null (seq [1..7]) (seq [1..7]) |> ignore)
        CheckThrowsArgumentNullException(fun() -> Seq.zip3 (seq [1..7]) null (seq [1..7]) |> ignore)
        CheckThrowsArgumentNullException(fun() -> Seq.zip3 (seq [1..7]) (seq [1..7]) null |> ignore)
        ()
        
    [<Test>]
    member this.tryPick() =
         // integer Seq
        let resultInt = Seq.tryPick (fun x-> if x = 1 then Some("got") else None) (seq [1..5])
         
        Assert.AreEqual(Some("got"),resultInt)
        
        // string Seq
        let resultStr = Seq.tryPick (fun x-> if x = "Are" then Some("got") else None) (seq ["Lists"; "Are"])
        Assert.AreEqual(Some("got"),resultStr)
        
        // empty Seq   
        let resultEpt = Seq.tryPick (fun x-> if x = 1 then Some("got") else None) Seq.empty
        Assert.IsNull(resultEpt)
       
        // null Seq
        let nullSeq : seq<'a> = null 
        let funcNull x = Some(1)
        
        CheckThrowsArgumentNullException(fun () -> Seq.tryPick funcNull nullSeq |> ignore)
   
        ()

    [<Test>]
    member this.tryItem() =
        // integer Seq
        let resultInt = Seq.tryItem 3 { 10..20 }
        Assert.AreEqual(Some(13), resultInt)

        // string Seq
        let resultStr = Seq.tryItem 2 (seq ["Lists"; "Are"; "Cool"; "List" ])
        Assert.AreEqual(Some("Cool"), resultStr)

        // empty Seq
        let resultEmpty = Seq.tryItem 0 Seq.empty
        Assert.AreEqual(None, resultEmpty)

        // null Seq
        let nullSeq:seq<'a> = null
        CheckThrowsArgumentNullException (fun () -> Seq.tryItem 3 nullSeq |> ignore)

        // Negative index
        let resultNegativeIndex = Seq.tryItem -1 { 10..20 }
        Assert.AreEqual(None, resultNegativeIndex)

        // Index greater than length
        let resultIndexGreater = Seq.tryItem 31 { 10..20 }
        Assert.AreEqual(None, resultIndexGreater)
