// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// Various tests for the:
// Microsoft.FSharp.Core.Operators module

namespace SystematicUnitTests.FSharp_Core.Microsoft_FSharp_Core

open System
open SystematicUnitTests.LibraryTestFx
open NUnit.Framework
open Microsoft.FSharp.Core.Operators.Checked

[<TestFixture>]
type OperatorsModule2() =

#if IGNORED_TESTS
    [<Test; Ignore( "[FSharp Bugs 1.0] #3842 - OverflowException does not pop up on Operators.int int16 int 32 int64 ")>]
    member this.int() =         
        // int 
        let result = Operators.int 10
        Assert.AreEqual(result,10)
        
        // string
        let result = Operators.int "10"
        Assert.AreEqual(result,10)
        
        // double
        let result = Operators.int 10.0
        Assert.AreEqual(result,10) 
        
        // negative
        let result = Operators.int -10
        Assert.AreEqual(result,-10) 
        
        // zero
        let result = Operators.int 0
        Assert.AreEqual(result,0) 
        
        // overflow
        CheckThrowsOverflowException(fun() -> Operators.int System.Double.MaxValue |>ignore)
        
        ()
#endif

#if IGNORED_TESTS
    [<Test; Ignore( "[FSharp Bugs 1.0] #3842 - OverflowException does not pop up on Operators.int int16 int 32 int64 ")>]
    member this.int16() =         
        // int 
        let result = Operators.int16 10
        Assert.AreEqual(result,10)
        
        // double
        let result = Operators.int16 10.0
        Assert.AreEqual(result,10) 
        
        // negative
        let result = Operators.int16 -10
        Assert.AreEqual(result,-10) 
        
        // zero
        let result = Operators.int16 0
        Assert.AreEqual(result,0) 
        
        // string
        let result = Operators.int16 "10"
        Assert.AreEqual(result,10)
        
        // overflow
        CheckThrowsOverflowException(fun() -> Operators.int16 System.Double.MaxValue |>ignore)
        
        ()
#endif

#if IGNORED_TESTS
    [<Test; Ignore( "[FSharp Bugs 1.0] #3842 - OverflowException does not pop up on Operators.int int16 int 32 int64 ")>]
    member this.int32() =         
        // int 
        let result = Operators.int32 10
        Assert.AreEqual(result,10)
        
        // double
        let result = Operators.int32 10.0
        Assert.AreEqual(result,10) 
        
        // negative
        let result = Operators.int32 -10
        Assert.AreEqual(result,-10) 
        
        // zero
        let result = Operators.int32 0
        Assert.AreEqual(result,0) 
        
        // string
        let result = Operators.int32 "10"
        Assert.AreEqual(result,10)
        
        // overflow
        CheckThrowsOverflowException(fun() -> Operators.int32 System.Double.MaxValue |>ignore)
        
        ()
#endif

#if IGNORED_TESTS
    [<Test; Ignore( "[FSharp Bugs 1.0] #3842 - OverflowException does not pop up on Operators.int int16 int 32 int64 ")>]
    member this.int64() =         
        // int 
        let result = Operators.int64 10
        Assert.AreEqual(result,10)
        
        // double
        let result = Operators.int64 10.0
        Assert.AreEqual(result,10) 
        
        // negative
        let result = Operators.int64 -10
        Assert.AreEqual(result,-10) 
        
        // zero
        let result = Operators.int64 0
        Assert.AreEqual(result,0) 
        
        // string
        let result = Operators.int64 "10"
        Assert.AreEqual(result,10)
        
        // overflow
        CheckThrowsOverflowException(fun() -> Operators.int64 System.Double.MaxValue |>ignore)
        
        ()
$endif

//    [<Test>]
//    member this.invalidArg() =         
//        CheckThrowsArgumentException(fun() -> Operators.invalidArg  "A" "B" |>ignore )
//        
//        ()
        
    [<Test>]
    member this.lock() = 
        // lock         
        printfn "test8 started"
        let syncRoot = System.Object()
        let k = ref 0
        let comp _ = async { return lock syncRoot (fun () -> incr k
                                                             System.Threading.Thread.Sleep(1)
                                                             !k ) }
        let arr = Async.RunSynchronously (Async.Parallel(Seq.map comp [1..50]))
        Assert.AreEqual((Array.sort compare arr; arr), [|1..50|])
        
        // without lock
        let syncRoot = System.Object()
        let k = ref 0
        let comp _ = async { do incr k
                             do! System.Threading.Thread.AsyncSleep(10)
                             return !k }
        let arr = Async.RunSynchronously (Async.Parallel(Seq.map comp [1..100]))
        Assert.AreNotEqual ((Array.sort compare arr; arr) , [|1..100|])
        
        ()
        
    [<Test>]
    member this.log() =  
        // double
        let result = Operators.log 10.0
        Assert.AreEqual(result.ToString(),"2.30258509299405") 
        
        // negative
        let result = Operators.log -10.0
        Assert.AreEqual(result.ToString(),System.Double.NaN.ToString()) 
        
        // zero
        let result = Operators.log 0.0
        Assert.AreEqual(result,-infinity) 
        
        ()
        
    [<Test>]
    member this.log10() =  
        // double
        let result = Operators.log10 10.0
        Assert.AreEqual(result,1) 
        
        // negative
        let result = Operators.log10 -10.0
        Assert.AreEqual(result.ToString(),System.Double.NaN.ToString())
        
        // zero
        let result = Operators.log10 0.0
        Assert.AreEqual(result,-infinity) 
        
        ()
        
    [<Test>]
    member this.max() =  
        // value type
        let result = Operators.max 10 8
        Assert.AreEqual(result,10) 
        
        // negative
        let result = Operators.max -10.0 -8.0
        Assert.AreEqual(result,-8.0) 
        
        // zero
        let result = Operators.max 0 0
        Assert.AreEqual(result,0) 
        
        // reference type
        let result = Operators.max "A" "ABC"
        Assert.AreEqual(result,"ABC") 
        
        // overflow
        CheckThrowsOverflowException(fun() -> Operators.max 10 System.Int32.MaxValue+1 |>ignore)
        
        ()
        
    [<Test>]
    member this.min() =  
        // value type
        let result = Operators.min 10 8
        Assert.AreEqual(result,8) 
        
        // negative
        let result = Operators.min -10.0 -8.0
        Assert.AreEqual(result,-10.0) 
        
        // zero
        let result = Operators.min 0 0
        Assert.AreEqual(result,0) 
        
        // reference type
        let result = Operators.min "A" "ABC"
        Assert.AreEqual(result,"A") 
        
        // overflow
        CheckThrowsOverflowException(fun() -> Operators.min 10 System.Int32.MinValue - 1 |>ignore)
        
        ()
        
    [<Test>]
    member this.nan() =  
        // value type
        let result = Operators.nan 
        Assert.AreEqual(result.ToString(),System.Double.NaN.ToString()) 
        
        ()
        
    [<Test>]
    member this.nanf() =  
        // value type
        let result = Operators.nanf 
        Assert.AreEqual(result,System.Single.NaN) 
        
        ()

#if IGNORED_TESTS
    [<Test; Ignore( "[FSharp Bugs 1.0] #3842 - OverflowException does not pop up on Operators.int int16 int 32 int64 ")>]
    member this.nativeint() =  
        // int 
        let result = Operators.nativeint 10
        Assert.AreEqual(result,10n)
        
        // double
        let result = Operators.nativeint 10.0
        Assert.AreEqual(result,10n) 
        
        // int64
        let result = Operators.nativeint 10L
        Assert.AreEqual(result,10n)         
       
        // negative
        let result = Operators.nativeint -10
        Assert.AreEqual(result,-10n) 
        
        // zero
        let result = Operators.nativeint 0
        Assert.AreEqual(result,0n) 
        
        // overflow
        CheckThrowsOverflowException(fun() -> Operators.nativeint System.Double.MaxValue |>ignore)
        
        ()
#endif

    [<Test>]
    member this.not() =  
        let result = Operators.not true
        Assert.IsFalse(result)
        
        let result = Operators.not false
        Assert.IsTrue(result) 
        
        ()
        
//    [<Test>]
//    member this.nullArg() =  
//        CheckThrowsArgumentNullException(fun() -> Operators.nullArg "A" |> ignore)
//          
//        ()
        
    [<Test>]
    member this.pown() =  
        // int 
        let result = Operators.pown 10 2
        Assert.AreEqual(result,100)
        
        // double
        let result = Operators.pown 10.0 2
        Assert.AreEqual(result,100) 
        
        // int64
        let result = Operators.pown 10L 2
        Assert.AreEqual(result,100) 
        
        // decimal
        let result = Operators.pown 10M 2
        Assert.AreEqual(result,100) 
        
        // negative
        let result = Operators.pown -10 2
        Assert.AreEqual(result,100) 
        
        // zero
        let result = Operators.pown 0 2
        Assert.AreEqual(result,0) 
        
        // overflow
        let result = Operators.pown System.Double.MaxValue System.Int32.MaxValue
        Assert.AreEqual(result,infinity) 
        
        CheckThrowsOverflowException(fun() -> Operators.pown System.Int32.MaxValue System.Int32.MaxValue |>ignore)
        
        ()
        
    [<Test>]
    member this.raise() =  
        CheckThrowsArgumentException(fun()-> Operators.raise <| new ArgumentException("Invalid Argument ")  |> ignore)
          
        ()
        
    
    [<Test>]
    member this.ref() =
        // value type
        let result = Operators.ref 0    
        let funInt (x:int) =   
            result := !result + x              
            () 
        Array.iter funInt [|1..10|]  
        Assert.AreEqual(!result,55)
        
        // reference type
        let result = Operators.ref ""
        let funStr (x : string) =
            result := (!result) + x   
            ()
        Array.iter funStr [|"A";"B";"C";"D"|]
        Assert.AreEqual(!result,"ABCD")
        
        ()    
    
    [<Test>]
    member this.reraise() =
        // double
        try
            ()
        with
        | _ ->    Operators.reraise()
        
        ()
    
    [<Test>]
    member this.round() =
        // double
        let result = Operators.round 10.0
        Assert.AreEqual(result,10) 
        
        // decimal
        let result = Operators.round 10M
        Assert.AreEqual(result,10)
        
        ()
    
    [<Test>]
    member this.sbyte() =         
        // int 
        let result = Operators.sbyte 10
        Assert.AreEqual(result,10)
        
        // double
        let result = Operators.sbyte 10.0
        Assert.AreEqual(result,10) 
        
        // negative
        let result = Operators.sbyte -10
        Assert.AreEqual(result,-10) 
        
        // zero
        let result = Operators.sbyte 0
        Assert.AreEqual(result,0) 
        
        ()
    
    [<Test>]
    member this.sign() =         
        // int 
        let result = Operators.sign 10
        Assert.AreEqual(result,1)
        
        // double
        let result = Operators.sign 10.0
        Assert.AreEqual(result,1) 
        
        // negative
        let result = Operators.sign -10
        Assert.AreEqual(result,-1) 
        
        // zero
        let result = Operators.sign 0
        Assert.AreEqual(result,0) 
        
        ()
    
    [<Test>]
    member this.sin() = 
        
        let result = Operators.sin 0.5
        Assert.AreEqual(result.ToString(),"0.479425538604203")      
        
        ()
    
    [<Test>]
    member this.single() = 
        // int 
        let result = Operators.single 10
        Assert.AreEqual(result,10)
        
        // double
        let result = Operators.single 10.0
        Assert.AreEqual(result,10) 
        
        // string
        let result = Operators.single "10"
        Assert.AreEqual(result,10) 
                
        ()
    
    [<Test>]
    member this.sinh() = 
     
        let result = Operators.sinh 1.0
        Assert.AreEqual(result.ToString(),"1.1752011936438") 
        
        ()
    
    [<Test>]
    member this.sizeof() = 
        // value type        
        let result = Operators.sizeof<int>
        Assert.AreEqual(result,4) 
        
        // System.Int64        
        let result = Operators.sizeof<System.Int64>
        Assert.AreEqual(result,8) 
        
        // reference type        
        let result = Operators.sizeof<string>
        Assert.AreEqual(result,4) 
        
        // null        
        let result = Operators.sizeof<unit>
        Assert.AreEqual(result,4) 
        
        ()
    
    [<Test>]
    member this.snd() = 
        // value type        
        let result = Operators.snd ("ABC",100)
        Assert.AreEqual(result,100) 
        
        // reference type        
        let result = Operators.snd (100,"ABC")
        Assert.AreEqual(result,"ABC") 
        
        // null        
        let result = Operators.snd (100,null)
        Assert.AreEqual(result,null) 
        
        ()
    
    [<Test>]
    member this.sqrt() = 
        // double        
        let result = Operators.sqrt 100.0
        Assert.AreEqual(result,10) 
        
        ()
    
    [<Test>]
    member this.stderr() =         
        let result = Operators.stderr 
        Assert.AreEqual(result.WriteLine("go"),null) 
        
        ()
    
    [<Test>]
    member this.stdin() =         
        let result = Operators.stdin 
        Assert.AreEqual(result.Dispose(),null)
        
        ()   
    
    [<Test>]
    member this.stdout() =         
        let result = Operators.stdout 
        Assert.AreEqual(result.WriteLine("go"),null)
        
        ()   
    
    [<Test>]
    member this.string() =  
        // value type
        let result = Operators.string 100
        Assert.AreEqual(result,"100")
        
        // reference type
        let result = Operators.string "ABC"
        Assert.AreEqual(result,"ABC")
        
        // unit
        CheckThrowsNullRefException(fun () -> Operators.string null |>ignore)
        
        ()      
    
    [<Test>]
    member this.tan() =  
        // double
        let result = Operators.tan 1.0
        Assert.AreEqual(result.ToString(),"1.5574077246549")
        
        ()    
    
    [<Test>]
    member this.tanh() =  
        // double
        let result = Operators.tanh 0.8
        Assert.AreEqual(result,0.664036770267849)
        
        ()    
    
    [<Test>]
    member this.truncate() =        
        // double
        let result = Operators.truncate 10.101
        Assert.AreEqual(result,10)
        
        // decimal
        let result = Operators.truncate 10.101M
        Assert.AreEqual(result,10M)
        
        // zero
        let result = Operators.truncate 0.101
        Assert.AreEqual(result,0)
        
        ()    
    
    [<Test>]
    member this.typedefof() =        
        // value type
        let result = Operators.typedefof<int>
        Assert.AreEqual(result.FullName,"System.Int32")
        
        // reference type
        let result = Operators.typedefof<string>
        Assert.AreEqual(result.FullName,"System.String")
        
        // unit
        let result = Operators.typedefof<unit>
        Assert.AreEqual(result.FullName,"Microsoft.FSharp.Core.Unit")
        
        ()
    
    [<Test>]
    member this.typeof() =        
        // value type
        let result = Operators.typeof<int>
        Assert.AreEqual(result.FullName,"System.Int32")
        
        // reference type
        let result = Operators.typeof<string>
        Assert.AreEqual(result.FullName,"System.String")
        
        // unit
        let result = Operators.typeof<unit>
        Assert.AreEqual(result.FullName,"Microsoft.FSharp.Core.Unit")
        
        ()
    
    [<Test>]
    member this.uint16() =        
        // int        
        let result = Operators.uint16 100
        Assert.AreEqual(result,100us)
        
        // double
        let result = Operators.uint16 (100.0:double)
        Assert.AreEqual(result,100us)
        
        // decimal
        let result = Operators.uint16 100M
        Assert.AreEqual(result,100us)
        
        ()
    
    [<Test>]
    member this.uint32() =        
        // int
        let result = Operators.uint32 100
        Assert.AreEqual(result,100ul)
        
        // double
        let result = Operators.uint32 (100.0:double)
        Assert.AreEqual(result,100ul)
        
        // decimal
        let result = Operators.uint32 100M
        Assert.AreEqual(result,100ul)
        
        ()
    
    [<Test>]
    member this.uint64() =        
        // int
        let result = Operators.uint64 100
        Assert.AreEqual(result,100UL)
        
        // double
        let result = Operators.uint64 (100.0:double)
        Assert.AreEqual(result,100UL)
        
        // decimal
        let result = Operators.uint64 100M
        Assert.AreEqual(result,100UL)
            
        ()   
    
    [<Test>]
    member this.unativeint() =        
        // int
        let result = Operators.unativeint 100
        Assert.AreEqual(result,100un)
        
        // double
        let result = Operators.unativeint (100.0:double)
        Assert.AreEqual(result,100un)
            
        ()     
    
    [<Test>]
    member this.unbox() =        
        // value type
        let oint = box 100
        let result = Operators.unbox oint
        Assert.AreEqual(result,100)
        
        // reference type
        let ostr = box "ABC"
        let result = Operators.unbox ostr
        Assert.AreEqual(result,"ABC")
        
        // null 
        let onull = box null
        let result = Operators.unbox onull
        Assert.AreEqual(result,null)
            
        ()     
    
    [<Test>]
    member this.using() =
        let sr = new System.IO.StringReader("ABCD")
        Assert.AreEqual(sr.ReadToEnd(),"ABCD")
        let result = Operators.using sr (fun x -> x.ToString())        
        CheckThrowsObjectDisposedException(fun () -> sr.ReadToEnd() |> ignore)
        
        ()    
    