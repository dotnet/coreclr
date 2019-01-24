// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// Various tests for the:
// Microsoft.FSharp.Collections.List module

namespace FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Collections

open System
open FSharp.Core.UnitTests.LibraryTestFx
open NUnit.Framework

(*
[Test Strategy]
Make sure each method works on:
* Integer List (value type)
* String List (reference type)
* Empty List (0 elements)
*)

type ListWindowedTestInput<'t> =
    {
        InputList : 't list
        WindowSize : int
        ExpectedList : 't list list
        Exception : Type option
    }

[<TestFixture>][<Category "Collections.List">][<Category "FSharp.Core.Collections">]
type ListModule02() =
    [<Test>]
    member this.Length() =
        // integer List  
        let resultInt = List.length [1..8]        
        Assert.AreEqual(8, resultInt)
        
        // string List    
        let resultStr = List.length ["a";"b";"c";"d"]        
        Assert.AreEqual(4, resultStr)
        
        // empty List     
        let resultEpt = List.length [ ]        
        Assert.AreEqual(0, resultEpt)
        
        ()

    [<Test>]
    member this.Map() = 
        // integer List
        let funcInt x = 
                match x with
                | _ when x % 2 = 0 -> 10*x            
                | _ -> x
        let resultInt = List.map funcInt [ 1..10 ]        
        Assert.AreEqual([1;20;3;40;5;60;7;80;9;100], resultInt)
        
        // string List
        let funcStr (x:string) = x.ToLower()
        let resultStr = List.map funcStr ["A";"B";"C";"D"]        
        Assert.AreEqual(["a";"b";"c";"d"], resultStr)
        
        // empty List
        let resultEpt = List.map funcInt [ ]        
        Assert.AreEqual(List.empty<int>, resultEpt)
        
        ()

    [<Test>]
    member this.Map2() = 
        // integer List 
        let funcInt x y = x+y
        let resultInt = List.map2 funcInt [1..10] [2..2..20]        
        Assert.AreEqual([3;6;9;12;15;18;21;24;27;30], resultInt)
        
        // string List
        let funcStr (x:int) (y:string) =  x+ y.Length
        let resultStr = List.map2 funcStr [3;6;9;11] ["a";"b";"c";"d"]        
        Assert.AreEqual([4;7;10;12], resultStr )
        
        // empty List
        let emptyArr:int list = [ ]
        let resultEpt = List.map2 funcInt emptyArr emptyArr        
        Assert.AreEqual(List.empty<int>, resultEpt)
        
        ()
        
    [<Test>]
    member this.Map3 () =

        // integer List  
        let funcInt x y z = (x + y) / z
        let resultInt = List.map3 funcInt  [ 1..10 ]  [2..2..20] [3..3..30]
        let expectedInt = List.init 10 (fun x ->  1)    
        Assert.AreEqual(expectedInt, resultInt)
        
        // string List
        let funcStr x y z = x + y + z        
        let resultStr = List.map3 funcStr ["A";"B";"C";"D"] ["a";"b";"c";"d"] ["1";"2";"3";"4"]        
        Assert.AreEqual(["Aa1";"Bb2";"Cc3";"Dd4"], resultStr)
        
        // lists of different length
        let shortList = [1]
        let longerList = [1; 2]
        CheckThrowsArgumentException  (fun () -> List.map3 funcInt shortList shortList longerList |> ignore)
        CheckThrowsArgumentException  (fun () -> List.map3 funcInt shortList longerList shortList |> ignore)
        CheckThrowsArgumentException  (fun () -> List.map3 funcInt shortList shortList longerList |> ignore)  

        // empty List
        let resultEpt = List.map3 funcInt List.empty List.empty List.empty
        Assert.AreEqual(List.empty<int>, resultEpt)
        
        ()


    [<Test>]
    member this.Collect() = 
        // integer List
        let funcInt x = 
                match x with
                | _ when x % 3 = 0 -> [999;999]            
                | _ -> [168;168]
        let resultInt = List.collect funcInt [ 1..5 ]
        let resultList = List.toArray resultInt        
        Assert.AreEqual([168;168;168;168;999;999;168;168;168;168;], resultInt )
        
        // string List
        let funcStr (x:string) = 
            match x with
            | _ when x.Length>3 -> ["long"]
            | _ -> ["short"]
        let resultStr = List.collect funcStr ["a";"b";"c";"d"]        
        Assert.AreEqual(["short"; "short";  "short" ; "short" ], resultStr)
        
        // empty List
        let resultEpt = List.collect funcInt [ ]        
        Assert.AreEqual(List.empty<int>, resultEpt)
        
        ()

    [<Test>]
    member this.Collect2() = 
        // The collect implementation uses mutation to create the result list; this test
        // helps verify that lists created by the user are never mutated
        let lists = [| [1]
                       [1;2]
                       [1;2;3] |]
        let g x =
            lists.[x]
        let r = [0..2] |> List.collect g
        Assert.AreEqual( [1; 1; 2; 1; 2; 3], r )
        Assert.AreEqual( [1], lists.[0])
        Assert.AreEqual( [1; 2] , lists.[1] )
        Assert.AreEqual( [1; 2; 3], lists.[2] )
        ()

    [<Test>]
    member this.Mapi() = 
        // integer List 
        let funcInt x y = x+y
        let resultInt = List.mapi funcInt [10..2..20]        
        Assert.AreEqual([10;13;16;19;22;25], resultInt)
        
        // string List
        let funcStr (x:int) (y:string) =  x+ y.Length
        let resultStr = List.mapi funcStr  ["a";"b";"c";"d"]        
        Assert.AreEqual([1;2;3;4], resultStr)
        
        // empty List
        let emptyArr:int list = [ ]
        let resultEpt = List.mapi funcInt emptyArr         
        Assert.AreEqual(List.empty<int>, resultEpt)
        
        ()

    [<Test>]
    member this.Mapi2() = 
        // integer List 
        let funcInt x y z = x + y + z
        let resultInt = List.mapi2 funcInt [1..10] [2..2..20]        
        Assert.AreEqual([3;7;11;15;19;23;27;31;35;39], resultInt)
        
        // string List
        let funcStr  z (x:int) (y:string) = z + x+ y.Length 
        let resultStr = List.mapi2 funcStr [3;6;9;11] ["a";"b";"c";"d"]        
        Assert.AreEqual([4;8;12;15], resultStr)
        
        // lists of different length
        let shortList = [1]
        let longerList = [1; 2]       
        CheckThrowsArgumentException  (fun () -> List.mapi2 funcInt shortList longerList |> ignore)
        CheckThrowsArgumentException  (fun () -> List.mapi2 funcInt longerList shortList |> ignore)

        // empty List
        let emptyArr:int list = [ ]
        let resultEpt = List.mapi2 funcInt emptyArr emptyArr        
        Assert.AreEqual(List.empty<int>, resultEpt)

        ()

    [<Test>]
    member this.Indexed() =
        // integer List
        Assert.AreEqual([(0,10);(1,12);(2,14);(3,16);(4,18);(5,20)], List.indexed [10..2..20])
        Assert.AreEqual([(0,10)], List.indexed [10])

        // string List
        let resultStr = List.indexed ["a";"b";"c";"d"]
        Assert.AreEqual([(0,"a");(1,"b");(2,"c");(3,"d")], resultStr)

        // empty List
        let emptyList:string list = []
        let resultEpt = List.indexed emptyList
        Assert.AreEqual(List.empty<int*string>, resultEpt)

        ()

    [<Test>]
    member this.MapFold() =
        // integer List
        let funcInt acc x = if x % 2 = 0 then 10*x, acc + 1 else x, acc
        let resultInt,resultIntAcc = List.mapFold funcInt 100 [ 1..10 ]
        Assert.AreEqual([1;20;3;40;5;60;7;80;9;100], resultInt)
        Assert.AreEqual(105, resultIntAcc)

        // integer List single item
        let funcInt acc x = if x % 2 = 0 then 10*x, acc + 1 else x, acc
        let resultInt,resultIntAcc = List.mapFold funcInt 100 [ 2 ]
        Assert.AreEqual([20], resultInt)
        Assert.AreEqual(101, resultIntAcc)

        // string List
        let funcStr acc (x:string) = match x.Length with 0 -> "empty", acc | _ -> x.ToLower(), sprintf "%s%s" acc x
        let resultStr,resultStrAcc = List.mapFold funcStr "" ["";"BB";"C";""]
        Assert.AreEqual(["empty";"bb";"c";"empty"], resultStr)
        Assert.AreEqual("BBC", resultStrAcc)

        // empty List
        let (resultEpt: int list),resultEptAcc = List.mapFold funcInt 100 []
        Assert.AreEqual(([]: int list), resultEpt)
        Assert.AreEqual(100, resultEptAcc)

        ()

    [<Test>]
    member this.MapFoldBack() =
        // integer List
        let funcInt x acc = if acc < 105 then 10*x, acc + 2 else x, acc
        let resultInt,resultIntAcc = List.mapFoldBack funcInt [ 1..10 ] 100
        Assert.AreEqual([1;2;3;4;5;6;7;80;90;100], resultInt)
        Assert.AreEqual(106, resultIntAcc)

        // integer List single item
        let resultInt,resultIntAcc = List.mapFoldBack funcInt [1] 100
        Assert.AreEqual([10], resultInt)
        Assert.AreEqual(102, resultIntAcc)

        // string List
        let funcStr (x:string) acc = match x.Length with 0 -> "empty", acc | _ -> x.ToLower(), sprintf "%s%s" acc x
        let resultStr,resultStrAcc = List.mapFoldBack funcStr ["";"BB";"C";""] ""
        Assert.AreEqual(["empty";"bb";"c";"empty"], resultStr)
        Assert.AreEqual("CBB", resultStrAcc)

        // empty List
        let (resultEpt: int list),resultEptAcc = List.mapFoldBack funcInt [] 100
        Assert.AreEqual(([]: int list), resultEpt)
        Assert.AreEqual(100, resultEptAcc)

        ()

    [<Test>]
    member this.Max() = 
        // integer List 
        let resultInt = List.max  [2..2..20]        
        Assert.AreEqual(20, resultInt)
        
        // string List
        let resultStr = List.max ["a";"b";"c";"d"]        
        Assert.AreEqual("d", resultStr)
        
        // empty List
        CheckThrowsArgumentException ( fun() -> List.max List.empty) 
        
        ()

    [<Test>]
    member this.MaxBy() = 
        // integer List 
        let funcInt x = x%8
        let resultInt = List.maxBy funcInt [2..2..20]        
        Assert.AreEqual(resultInt , 6)
        
        // string List
        let funcStr (x:string)  =x.Length 
        let resultStr = List.maxBy funcStr  ["a";"b";"c";"d"]        
        Assert.AreEqual("a", resultStr)
        
        // empty List    
        CheckThrowsArgumentException ( fun() -> List.maxBy (fun () -> 1) List.empty )

        ()

    [<Test>]
    member this.Min() =
        // integer List 
        let resultInt = List.min  [3;7;8;9;4;1;1;2]        
        Assert.AreEqual(1, resultInt)
        
        // string List
        let resultStr = List.min ["a";"b";"c";"d"]        
        Assert.AreEqual("a", resultStr)
        
        // empty List   
        CheckThrowsArgumentException ( fun() -> List.min List.empty)
        
        () 

    [<Test>]
    member this.MinBy() = 
        // integer List 
        let funcInt x = x%8
        let resultInt = List.minBy funcInt [3;7;9;4;8;1;1;2]        
        Assert.AreEqual(8, resultInt)
        
        // string List
        let funcStr (x:string) = x.Length 
        let resultStr = List.minBy funcStr  ["a";"b";"c";"d"]        
        Assert.AreEqual("a", resultStr)
        
        // empty List
        let funcEpt () = 1
        CheckThrowsArgumentException ( fun() -> List.minBy funcEpt List.empty)
       
        ()

    [<Test>]
    member this.Item() =
        // integer List
        let resultInt = List.item 3 [3;7;9;4;8;1;1;2]
        Assert.AreEqual(4, resultInt)

        // string List
        let resultStr = List.item 3 ["a";"b";"c";"d"]
        Assert.AreEqual("d", resultStr)

        // empty List
        CheckThrowsArgumentException (fun() -> List.item 1 List.empty)

        // Negative index
        for i = -1 downto -10 do
           CheckThrowsArgumentException (fun () -> List.item i [3;7;9;4;8;1;1;2] |> ignore)

        // Out of range
        for i = 8 to 16 do
           CheckThrowsArgumentException (fun () -> List.item i [3;7;9;4;8;1;1;2] |> ignore)


    [<Test>]
    member this.Of_Array() =
        // integer List  
        let resultInt = List.ofArray [|1..10|]        
        Assert.AreEqual([1..10], resultInt)
        
        // string List    
        let resultStr = List.ofArray [|"a";"b";"c";"d"|]        
        Assert.AreEqual(["a";"b";"c";"d"], resultStr)
        
        // empty List     
        let resultEpt = List.ofArray  [||]        
        Assert.AreEqual(List.empty, resultEpt)
        
        ()

    [<Test>]
    member this.Of_Seq() =
        // integer List  
        let resultInt = List.ofSeq {1..10}        
        Assert.AreEqual([1..10], resultInt)
        
        // string List    
        let resultStr = List.ofSeq (seq {for x in 'a'..'f' -> x.ToString()})        
        Assert.AreEqual([ "a";"b";"c";"d";"e";"f" ], resultStr)
        
        // empty List     
        let resultEpt = List.ofSeq  []        
        Assert.AreEqual(List.empty, resultEpt)
        
        ()

    [<Test>]
    member this.Partition() =
        // integer List  
        let resultInt = List.partition (fun x -> x % 3 = 0) [1..10]        
        Assert.AreEqual(([3;6;9], [1;2;4;5;7;8;10]), resultInt )
        
        // string List    
        let resultStr = List.partition (fun (x:string) -> x.Length > 4) ["a";"b";"c";"d"]        
        Assert.AreEqual(List.empty<string>, fst resultStr)
        Assert.AreEqual(["a";"b";"c";"d"] |> Array.ofList, (snd resultStr) |> Array.ofList)
        
        // empty List     
        let resultEpt = List.partition (fun x -> x % 3 = 0) []        
        Assert.AreEqual(List.empty<int>, fst resultEpt)
        Assert.AreEqual(List.empty<int>, snd resultEpt)
        
        ()

    [<Test>]
    member this.Permute() =
        // integer List  
        let resultInt = List.permute (fun i -> (i+1) % 4) [1;2;3;4]
        Assert.AreEqual([4;1;2;3], resultInt)
        
        // string List    
        let resultStr = List.permute (fun i -> (i+1) % 4) ["a";"b";"c";"d"]    
        Assert.AreEqual(["d";"a";"b";"c"], resultStr)
        
        // empty List     
        let resultEpt = List.permute (fun i -> (i+1) % 4) []    
        Assert.AreEqual([], resultEpt)           
        
        ()

    [<Test>]
    member this.Reduce() =
        // integer List  
        let resultInt = List.reduce (fun x y -> x/y) [5*4*3*2; 4;3;2;1]
        Assert.AreEqual(5, resultInt)  
        
        // string List    
        let resultStr = List.reduce (fun (x:string) (y:string) -> x.Remove(0,y.Length)) ["ABCDE";"A"; "B";  "C" ; "D"]
        Assert.AreEqual("E", resultStr) 
        
        // empty List 
        CheckThrowsArgumentException (fun () -> List.reduce (fun x y -> x/y)  [] |> ignore)

        
        ()
        
    [<Test>]
    member this.ReduceBack() =
        // integer List  
        let resultInt = List.reduceBack (fun x y -> x/y) [5*4*3*2; 4;3;2;1]
        Assert.AreEqual(30, resultInt) 
        
        // string List    
        let resultStr = List.reduceBack (fun (x:string) (y:string) -> x.Remove(0,y.Length)) ["ABCDE";"A"; "B";  "C" ; "D"]
        Assert.AreEqual("ABCDE",resultStr) 
        
        // empty List 
        CheckThrowsArgumentException (fun () -> List.reduceBack (fun x y -> x/y) [] |> ignore)
        
        ()

    [<Test>]
    member this.Rev() =
        // integer List  
        let resultInt = List.rev  [1..10]        
        Assert.AreEqual(resultInt , [10;9;8;7;6;5;4;3;2;1])
        
        // string List    
        let resultStr = List.rev  ["a";"b";"c";"d"]        
        Assert.AreEqual( ["d";"c";"b";"a"], resultStr)
        
        // empty List     
        let resultEpt = List.rev  []        
        Assert.AreEqual([], resultEpt)
        
        ()

    [<Test>]
    member this.Scan() =
        // integer List
        let funcInt x y = x+y
        let resultInt = List.scan funcInt 9 [ 1..10 ]        
        Assert.AreEqual([9;10;12;15;19;24;30;37;45;54;64], resultInt)
        
        // string List
        let funcStr x y = x+y        
        let resultStr = List.scan funcStr "*" ["a";"b";"c";"d"]        
        Assert.AreEqual(["*";"*a";"*ab";"*abc";"*abcd"], resultStr)
        
        // empty List
        let resultEpt = List.scan funcInt 5 [ ]        
        Assert.AreEqual([5], resultEpt)
        
        ()   
    [<Test>]
    member this.ScanBack() =
        // integer List 
        let funcInt x y = x+y
        let resultInt = List.scanBack funcInt [ 1..10 ] 9        
        Assert.AreEqual([64;63;61;58;54;49;43;36;28;19;9], resultInt)
        
        // string List
        let funcStr x y = x+y        
        let resultStr = List.scanBack funcStr ["a";"b";"c";"d"] "*"         
        Assert.AreEqual(["abcd*";"bcd*";"cd*";"d*";"*"], resultStr)
        
        // empty List
        let resultEpt = List.scanBack funcInt [ ] 5         
        Assert.AreEqual([5], resultEpt)
        
        () 

    [<Test>]
    member this.Skip() =
        // integer List
        let resultInt = List.skip 2 [1..10]
        Assert.AreEqual([3..10], resultInt)
        
        let resultInt2 = List.skip 0 [1..10]
        Assert.AreEqual([1..10], resultInt2)
        
        let resultInt3 = List.skip -1 [1..10]
        Assert.AreEqual([1..10], resultInt3)

        // string List
        let resultStr = List.skip 2 ["str1";"str2";"str3";"str4"]
        Assert.AreEqual(["str3";"str4"], resultStr)

        // empty List
        let resultEpt = List.skip 0 []
        Assert.AreEqual([], resultEpt)

        // exceptions
        CheckThrowsArgumentException(fun () -> List.skip 1 [] |> ignore)
        CheckThrowsArgumentException(fun () -> List.skip 4 [1; 2; 3] |> ignore)

    [<Test>]
    member this.SkipWhile() =
        // integer list
        let funcInt x = (x < 4)
        let intList = [1..10]
        let resultInt = List.skipWhile funcInt intList
        if resultInt <> [4..10] then Assert.Fail()

        // string list
        let funcStr (s:string) = s.Length < 8
        let strList = [ "Lists"; "are";  "commonly" ; "list" ]
        let resultStr = List.skipWhile funcStr strList
        if resultStr <> [ "commonly" ; "list" ] then Assert.Fail()

        // empty list
        let resultEmpt = List.skipWhile (fun _ -> failwith "unexpected error") []
        if resultEmpt <> [] then Assert.Fail()

        // skip all
        let resultAll = List.skipWhile (fun _ -> true) intList
        if resultAll <> [] then Assert.Fail()

        // skip none
        let resultNone = List.skipWhile (fun _ -> false) intList
        if resultNone <> intList then Assert.Fail()

        ()

    [<Test>]
    member this.Sort() =
        // integer List  
        let intArr = [3;5;7;2;4;8]
        let resultInt = List.sort intArr          
        Assert.AreEqual(resultInt , [2;3;4;5;7;8])
        
        // string List
        let strArr = ["Z";"a";"d";"Y";"c";"b";"X"]   
        let resultStr = List.sort strArr         
        Assert.AreEqual(["X"; "Y"; "Z"; "a"; "b"; "c"; "d"], resultStr)
        
        // empty List
        let emptyArr : int list = [ ]
        let resultEpt = List.sort emptyArr        
        Assert.AreEqual(List.empty<int>, resultEpt)

        // tuple List
        let tupArr = [(2,"a");(1,"d");(1,"b");(1,"a");(2,"x");(2,"b");(1,"x")]   
        let resultTup = List.sort tupArr         
        Assert.AreEqual([(1,"a");(1,"b");(1,"d");(1,"x");(2,"a");(2,"b");(2,"x")], resultTup)
        
        ()    

    [<Test>]
    member this.SortBy() =
        // integer List  
        let intArr = [3;5;7;2;4;8]
        let resultInt = List.sortBy int intArr          
        Assert.AreEqual([2;3;4;5;7;8], resultInt)
        
        // string List
        let strArr = [".."; "..."; "."; "...."]    
        let resultStr = List.sortBy (fun (x:string) -> x.Length)  strArr 
        Assert.AreEqual(["."; ".."; "..."; "...."], resultStr)
        
        // empty List
        let emptyArr:int list = [ ]
        let resultEpt = List.sortBy int emptyArr        
        Assert.AreEqual(List.empty<int>, resultEpt)

        // tuple List
        let tupArr = [(2,"a");(1,"d");(1,"b");(1,"a");(2,"x");(2,"b");(1,"x")]   
        let resultTup = List.sortBy snd tupArr         
        Assert.AreEqual([(2,"a");(1,"a");(1,"b");(2,"b");(1,"d");(2,"x");(1,"x")], resultTup)
        
        ()
        
    [<Test>]
    member this.SortDescending() =
        // integer List
        let minInt,maxInt = System.Int32.MinValue,System.Int32.MaxValue
        let intArr = [3;5;7;2;4;8;0;-1;1;minInt;maxInt]
        let resultInt = List.sortDescending intArr          
        Assert.AreEqual(resultInt , [maxInt;8;7;5;4;3;2;1;0;-1;minInt])
        
        // string List
        let strArr = ["Z";"a";"d";"Y";"c";"b";"X"]   
        let resultStr = List.sortDescending strArr         
        Assert.AreEqual(["d"; "c"; "b"; "a"; "Z"; "Y"; "X"], resultStr)
        
        // empty List
        let emptyArr : int list = [ ]
        let resultEpt = List.sortDescending emptyArr        
        Assert.AreEqual(List.empty<int>, resultEpt)

        // tuple List
        let tupArr = [(2,"a");(1,"d");(1,"b");(1,"a");(2,"x");(2,"b");(1,"x")]   
        let resultTup = List.sortDescending tupArr         
        Assert.AreEqual([(2,"x");(2,"b");(2,"a");(1,"x");(1,"d");(1,"b");(1,"a")], resultTup)
        
        // float Seq
        let minFloat,maxFloat,epsilon = System.Double.MinValue,System.Double.MaxValue,System.Double.Epsilon
        let floatArr = [0.0; 0.5; 2.0; 1.5; 1.0; minFloat;maxFloat;epsilon;-epsilon]
        let resultFloat = List.sortDescending floatArr
        let expectedFloat = [maxFloat; 2.0; 1.5; 1.0; 0.5; epsilon; 0.0; -epsilon; minFloat; ]
        Assert.AreEqual(expectedFloat, resultFloat)

        () 
        
    [<Test>]
    member this.SortByDescending() =
        // integer List  
        let intArr = [3;5;7;2;4;8]
        let resultInt = List.sortByDescending int intArr          
        Assert.AreEqual([8;7;5;4;3;2], resultInt)
        
        // string List
        let strArr = [".."; "..."; "."; "...."]    
        let resultStr = List.sortByDescending (fun (x:string) -> x.Length)  strArr 
        Assert.AreEqual(["...."; "..."; ".."; "."], resultStr)
        
        // empty List
        let emptyArr:int list = [ ]
        let resultEpt = List.sortByDescending int emptyArr        
        Assert.AreEqual(List.empty<int>, resultEpt)
        
        // tuple List
        let tupArr = [(2,"a");(1,"d");(1,"b");(1,"a");(2,"x");(2,"b");(1,"x")]   
        let resultTup = List.sortByDescending snd tupArr         
        Assert.AreEqual( [(2,"x");(1,"x");(1,"d");(1,"b");(2,"b");(2,"a");(1,"a")] , resultTup)

        // float Seq
        let minFloat,maxFloat,epsilon = System.Double.MinValue,System.Double.MaxValue,System.Double.Epsilon
        let floatArr = [0.0; 0.5; 2.0; 1.5; 1.0; minFloat;maxFloat;epsilon;-epsilon]
        let resultFloat = List.sortByDescending id floatArr
        let expectedFloat = [maxFloat; 2.0; 1.5; 1.0; 0.5; epsilon; 0.0; -epsilon; minFloat; ]
        Assert.AreEqual(expectedFloat, resultFloat)

        ()  
  
    [<Test>]
    member this.SortWith() =

        // integer list
        let intComparer a b = compare (a%3) (b%3)
        let resultInt = List.sortWith intComparer [0..10]
        Assert.AreEqual([0;3;6;9;1;4;7;10;2;5;8], resultInt)

        // string list
        let resultStr = List.sortWith compare ["str1";"str3";"str2";"str4"]
        Assert.AreEqual(["str1";"str2";"str3";"str4"], resultStr)

        // empty list
        let resultEpt = List.sortWith intComparer []
        Assert.AreEqual(([] : int list), resultEpt)

        ()

    [<Test>]
    member this.Sum() =
        // empty integer List 
        let resultEptInt = List.sum ([]:int list)         
        Assert.AreEqual(0, resultEptInt)
        
        // empty float32 List
        let emptyFloatList = List.empty<System.Single> 
        let resultEptFloat = List.sum emptyFloatList         
        Assert.AreEqual(0.0f, resultEptFloat)
        
        // empty double List
        let emptyDoubleList = List.empty<System.Double> 
        let resultDouEmp = List.sum emptyDoubleList         
        Assert.AreEqual(0.0, resultDouEmp)
        
        // empty decimal List
        // currently bugged. see[FSharp Bugs 1.0] #3510 - 'List.average does not work on empty Decimals List'
        let emptyDecimalList = List.empty<System.Decimal> 
        let resultDecEmp = List.sum emptyDecimalList         
        Assert.AreEqual(0M, resultDecEmp)

        // integer List  
        let resultInt = List.sum [1..10]         
        Assert.AreEqual(55, resultInt)
        
        // float32 List
        let floatList: float32 list = [ 1.2f;3.5f;6.7f ]
        let resultFloat = List.sum floatList        
        Assert.AreEqual(11.4f, resultFloat)
        
        // double List
        let doubleList: System.Double list = [ 1.0;8.0 ]
        let resultDouble = List.sum doubleList        
        Assert.AreEqual(9.0, resultDouble)
        
        // decimal List
        let decimalList: decimal list = [ 0M;19M;19.03M ]
        let resultDecimal = List.sum decimalList        
        Assert.AreEqual(38.03M , resultDecimal)
       
        ()

    [<Test>]
    member this.SumBy() =
        // empty integer List 
        let resultEptInt = List.sumBy int ([]:int list)         
        Assert.AreEqual(0, resultEptInt)
        
        // empty float32 List
        let emptyFloatList = List.empty<System.Single> 
        let resultEptFloat = List.sumBy float32 emptyFloatList         
        Assert.AreEqual(0.0f, resultEptFloat)
        
        // empty double List
        let emptyDoubleList = List.empty<System.Double> 
        let resultDouEmp = List.sumBy float emptyDoubleList         
        Assert.AreEqual(0.0, resultDouEmp)
        
        // empty decimal List
        // currently bugged. see[FSharp Bugs 1.0] #3510 - 'List.average does not work on empty Decimals List'
        let emptyDecimalList = List.empty<System.Decimal> 
        let resultDecEmp = List.sumBy decimal emptyDecimalList         
        Assert.AreEqual(0M, resultDecEmp)

        // integer List  
        let resultInt = List.sumBy int [1..10]         
        Assert.AreEqual(55, resultInt)
        
        // float32 List
        let floatList: string list = [ "1.2";"3.5";"6.7" ]
        let resultFloat = List.sumBy float32 floatList        
        Assert.AreEqual(11.4f, resultFloat)
        
        // double List
        let doubleList: System.Double list = [ 1.0;8.0 ]
        let resultDouble = List.sumBy float doubleList        
        Assert.AreEqual(9.0, resultDouble)
        
        // decimal List
        let decimalList: decimal list = [ 0M;19M;19.03M ]
        let resultDecimal = List.sumBy decimal decimalList        
        Assert.AreEqual(38.03M, resultDecimal)
        
        ()

    [<Test>]
    member this.Tl() =
        // integer List  
        let resultInt = List.tail [1..10]        
        Assert.AreEqual([2..10], resultInt)
        
        // string List    
        let resultStr = List.tail ["a";"b";"c";"d"]        
        Assert.AreEqual([ "b";  "c" ; "d"], resultStr)

        CheckThrowsArgumentException(fun () -> List.tail [] |> ignore)
        ()        

    [<Test>]
    member this.To_Array() =
        // integer List  
        let resultInt = List.toArray [1..10]        
        Assert.AreEqual([|1..10|] , resultInt)
        
        // string List    
        let resultStr = List.toArray ["a";"b";"c";"d"]        
        Assert.AreEqual([|"a";"b";"c";"d"|], resultStr)
        
        // empty List     
        let resultEpt = List.toArray  []        
        Assert.AreEqual([| |], resultEpt)
      
        ()    
        
    [<Test>]
    member this.To_Seq() =
        // integer List  
        let resultInt = [1..10] |> List.toSeq  |> List.ofSeq        
        Assert.AreEqual(resultInt , [1..10])
        
        // string List    
        let resultStr = ["a";"b";"c";"d"] |> List.toSeq |> List.ofSeq        
        Assert.AreEqual(["a";"b";"c";"d"], resultStr)
        
        // empty List     
        let resultEpt =[] |> List.toSeq  |> List.ofSeq        
        Assert.AreEqual(List.empty, resultEpt)
        
        ()   

    [<Test>]
    member this.Transpose() =
        // integer list
        Assert.AreEqual([[1; 4]; [2; 5]; [3; 6]], List.transpose (seq [[1..3]; [4..6]]))
        Assert.AreEqual([[1]; [2]; [3]], List.transpose [[1..3]])
        Assert.AreEqual([[1..2]], List.transpose [[1]; [2]])

        // string list
        Assert.AreEqual([["a";"d"]; ["b";"e"]; ["c";"f"]], List.transpose (seq [["a";"b";"c"]; ["d";"e";"f"]]))

        // empty list
        Assert.AreEqual([], List.transpose [])

        // list of empty lists - m x 0 list transposes to 0 x m (i.e. empty)
        Assert.AreEqual([], List.transpose [[]])
        Assert.AreEqual([], List.transpose [[]; []])

        // jagged lists
        CheckThrowsArgumentException (fun () -> List.transpose [[1; 2]; [3]] |> ignore)
        CheckThrowsArgumentException (fun () -> List.transpose [[1]; [2; 3]] |> ignore)

    [<Test>]
    member this.Truncate() =
        // integer list
        Assert.AreEqual([1..3], List.truncate 3 [1..5])
        Assert.AreEqual([1..5], List.truncate 10 [1..5])
        Assert.AreEqual(([] : int list), List.truncate 0 [1..5])

        // string list
        Assert.AreEqual(["str1";"str2"], List.truncate 2 ["str1";"str2";"str3"])

        // empty list
        Assert.AreEqual(([] : int list), List.truncate 0 ([] : int list))
        Assert.AreEqual(([] : int list), List.truncate 1 ([] : int list))

        // negative count
        Assert.AreEqual(([] : int list), List.truncate -1 [1..5])
        Assert.AreEqual(([] : int list), List.truncate System.Int32.MinValue [1..5])

        ()

    [<Test>]
    member this.TryFind() =
        // integer List  
        let resultInt = [1..10] |> List.tryFind (fun x -> x%7 = 0)          
        Assert.AreEqual(Some 7, resultInt)
        
        // string List    
        let resultStr = ["a";"b";"c";"d"] |> List.tryFind (fun (x:string) -> x.Contains("c"))        
        Assert.AreEqual(Some "c", resultStr)
        
        // empty List     
        let resultEpt =[] |> List.tryFind  (fun x -> x%7 = 0)          
        Assert.AreEqual(None, resultEpt)
        
        // Head satisfy
        let resultHead = [7 .. -1 .. 0] |> List.tryFind (fun x -> x % 7 = 0)
        Assert.AreEqual(Some 7, resultHead)

        ()
        
    [<Test>]
    member this.TryFindBack() =
        // integer List
        Assert.AreEqual(Some 20, [1..20] |> List.tryFindBack (fun x -> x%5 = 0))
        Assert.AreEqual(Some 15, [1..19] |> List.tryFindBack (fun x -> x%5 = 0))
        Assert.AreEqual(Some 5, [5..9] |> List.tryFindBack (fun x -> x%5 = 0))

        // string List
        let resultStr = ["a";"b";"cc";"dd"] |> List.tryFindBack (fun (x:string) -> x.Length > 1)
        Assert.AreEqual(Some "dd", resultStr)

        // empty List
        Assert.AreEqual(None, [] |> List.tryFindBack  (fun _ -> true))

        // not found
        Assert.AreEqual(None, [1..20] |> List.tryFindBack  (fun _ -> false))

        // Head satisfy
        let resultHead = [7 .. -1 .. 1] |> List.tryFindBack (fun x -> x % 7 = 0)
        Assert.AreEqual(Some 7, resultHead)

        ()

    [<Test>]
    member this.TryFindIndex() =
        // integer List  
        let resultInt = [1..10] |> List.tryFindIndex (fun x -> x%7 = 0)          
        Assert.AreEqual(Some 6, resultInt)
        
        // string List    
        let resultStr = ["a";"b";"c";"d"] |> List.tryFindIndex (fun (x:string) -> x.Length > 4)        
        Assert.AreEqual(None, resultStr)
        
        // empty List     
        let resultEpt = [] |> List.tryFindIndex  (fun x -> x%7 = 0)          
        Assert.AreEqual(None, resultEpt)
        ()

    [<Test>]
    member this.TryFindIndexBack() =
        // integer List
        Assert.AreEqual(Some 19, [1..20] |> List.tryFindIndexBack (fun x -> x%5 = 0))
        Assert.AreEqual(Some 14, [1..19] |> List.tryFindIndexBack (fun x -> x%5 = 0))
        Assert.AreEqual(Some 0, [5..9] |> List.tryFindIndexBack (fun x -> x%5 = 0))

        // string List
        let resultStr = ["a";"b";"cc";"dd"] |> List.tryFindIndexBack (fun (x:string) -> x.Length > 1)
        Assert.AreEqual(Some 3, resultStr)

        // empty List
        Assert.AreEqual(None, [] |> List.tryFindIndexBack (fun _ -> true))

        // not found
        Assert.AreEqual(None, [1..20] |> List.tryFindIndexBack (fun _ -> false))

        ()

    [<Test>]
    member this.Unfold() =
        // integer Seq
        let resultInt = List.unfold (fun x -> if x < 20 then Some (x+1,x*2) else None) 1
        Assert.AreEqual([2;3;5;9;17], resultInt)

        // string Seq
        let resultStr = List.unfold (fun (x:string) -> if x.Contains("unfold") then Some("a","b") else None) "unfold"
        Assert.AreEqual(["a"], resultStr)

        // empty seq
        //let resultEpt = List.unfold (fun _ -> Option<string>.None) 1
        let resultEpt = List.unfold (fun _ -> None) 1
        Assert.AreEqual([], resultEpt)

        ()

    [<Test>]
    member this.Unzip() =
        // integer List  
        let resultInt =  List.unzip [(1,2);(2,4);(3,6)]         
        Assert.AreEqual(([1..3], [2..2..6]) , resultInt )
        
        // string List    
        let resultStr = List.unzip [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let str = resultStr.ToString()        
        Assert.AreEqual(([2;3;4;5],["b";"c";"d";"e"]) , resultStr)
        
        // empty List     
        let resultEpt = List.unzip  []        
        Assert.AreEqual(( [], []), resultEpt)

        // null List
        
        ()

    [<Test>]
    member this.Unzip3() =
        // integer List  
        let resultInt =  List.unzip3 [(1,2,3);(2,4,6);(3,6,9)]        
        Assert.AreEqual(([1;2;3], [2;4;6], [3;6;9]), resultInt)
        
        // string List    
        let resultStr = List.unzip3 [(2,"b","II");(3,"c","III");(4,"d","IV");(5,"e","V")]        
        Assert.AreEqual(([2;3;4;5], ["b";"c";"d";"e"], ["II"; "III"; "IV"; "V"]), resultStr)
        
        // empty List     
        let resultEpt = List.unzip3   []        
        Assert.AreEqual(( [],  [],  []), resultEpt)

        // null List
        
        ()

    [<Test>]
    member this.Windowed() =
        let testWindowed config =
            try
                config.InputList
                |> List.windowed config.WindowSize
                |> (fun actual -> Assert.AreEqual(config.ExpectedList,actual))
            with
            | _ when Option.isNone config.Exception -> Assert.Fail()
            | e when e.GetType() = (Option.get config.Exception) -> ()
            | _ -> Assert.Fail()

        {
          InputList = [1..10]
          WindowSize = 1
          ExpectedList =  [ for i in 1..10 do yield [i] ]
          Exception = None
        } |> testWindowed
        {
          InputList = [1..10]
          WindowSize = 5
          ExpectedList =  [ for i in 1..6 do yield [i; i+1; i+2; i+3; i+4] ]
          Exception = None
        } |> testWindowed
        {
          InputList = [1..10]
          WindowSize = 10
          ExpectedList =  [ yield [1..10] ]
          Exception = None
        } |> testWindowed
        {
          InputList = [1..10]
          WindowSize = 25
          ExpectedList = ([] : int list list)
          Exception = None
        } |> testWindowed
        {
          InputList = ["str1";"str2";"str3";"str4"]
          WindowSize = 2
          ExpectedList =  [ ["str1";"str2"]; ["str2";"str3"]; ["str3";"str4"] ]
          Exception = None
        } |> testWindowed
        {
          InputList = ([] : int list)
          WindowSize = 2
          ExpectedList = ([] : int list list)
          Exception = None
        } |> testWindowed
        {
          InputList = [1..10]
          WindowSize = 0
          ExpectedList =  ([] : int list list)
          Exception = Some typeof<ArgumentException>
        } |> testWindowed

        // expectedLists indexed by arraySize,windowSize
        let expectedLists = Array2D.zeroCreate 6 6
        expectedLists.[1,1] <- [ [1] ]
        expectedLists.[2,1] <- [ [1]; [2] ]
        expectedLists.[2,2] <- [ [1; 2] ]
        expectedLists.[3,1] <- [ [1]; [2]; [3] ]
        expectedLists.[3,2] <- [ [1; 2]; [2; 3] ]
        expectedLists.[3,3] <- [ [1; 2; 3] ]
        expectedLists.[4,1] <- [ [1]; [2]; [3]; [4] ]
        expectedLists.[4,2] <- [ [1; 2]; [2; 3]; [3; 4] ]
        expectedLists.[4,3] <- [ [1; 2; 3]; [2; 3; 4] ]
        expectedLists.[4,4] <- [ [1; 2; 3; 4] ]
        expectedLists.[5,1] <- [ [1]; [2]; [3]; [4]; [5] ]
        expectedLists.[5,2] <- [ [1; 2]; [2; 3]; [3; 4]; [4; 5] ]
        expectedLists.[5,3] <- [ [1; 2; 3]; [2; 3; 4]; [3; 4; 5] ]
        expectedLists.[5,4] <- [ [1; 2; 3; 4]; [2; 3; 4; 5] ]
        expectedLists.[5,5] <- [ [1; 2; 3; 4; 5] ]

        for arraySize = 0 to 5 do
            for windowSize = -1 to 5 do
                if windowSize <= 0 then
                    CheckThrowsArgumentException (fun () -> List.windowed windowSize [1..arraySize] |> ignore)
                elif arraySize < windowSize then
                    Assert.AreEqual(([] : int list list), List.windowed windowSize [1..arraySize])
                else
                    Assert.AreEqual(expectedLists.[arraySize, windowSize], List.windowed windowSize [1..arraySize])

        ()

    [<Test>]
    member this.Zip() =
        // integer List  
        let resultInt =  List.zip [1..3] [2..2..6]         
        Assert.AreEqual([(1,2);(2,4);(3,6)], resultInt)
        
        // string List    
        let resultStr = List.zip [2;3;4;5] ["b";"c";"d";"e"]         
        Assert.AreEqual([(2,"b");(3,"c");(4,"d");(5,"e")] , resultStr)
        
        // empty List     
        let resultEpt = List.zip   []  []  
        let empTuple:(obj*obj) list = []      
        Assert.AreEqual(empTuple, resultEpt)
        
        ()

    [<Test>]
    member this.Zip3() =
        // integer List  
        let resultInt =  List.zip3 [1..3] [2..2..6] [3;6;9]        
        Assert.AreEqual([(1,2,3); (2,4,6); (3,6,9)], resultInt)
        
        // string List    
        let resultStr = List.zip3[2;3;4;5] ["b";"c";"d";"e"] ["II"; "III"; "IV"; "V"]        
        Assert.AreEqual([(2,"b","II");(3,"c","III");(4,"d","IV");(5,"e","V")], resultStr)
        
        // empty List     
        let resultEpt = List.zip3   []  []  [] 
        let empTriple:(obj*obj*obj) list = []        
        Assert.AreEqual(empTriple, resultEpt)
       
        ()            

    [<Test>]
    member this.tryItem() =
        // integer List
        let resultInt = List.tryItem 4 [3;7;9;4;8;1;1;2]
        Assert.AreEqual(Some(8), resultInt)

        // string List
        let resultStr = List.tryItem 2 ["a";"b";"c";"d"]
        Assert.AreEqual(Some("c"), resultStr)

        // empty List
        let resultEmpty = List.tryItem 0 List.empty
        Assert.AreEqual(None, resultEmpty)

        // Negative index
        let resultNegativeIndex = List.tryItem -1 [3;1;6;2]
        Assert.AreEqual(None, resultNegativeIndex)

        // Index equals to length
        let resultIndexGreater = List.tryItem 4 [3;1;6;2]
        Assert.AreEqual(None, resultIndexGreater)
