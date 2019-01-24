// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// Various tests for the:
// Microsoft.FSharp.Collections.Array module

namespace FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Collections

open System
open FSharp.Core.UnitTests.LibraryTestFx
open NUnit.Framework

(*
[Test Strategy]
Make sure each method works on:
* Integer array (value type)
* String  array (reference type)
* Empty   array (0 elements)
* Null    array (null)
*)

type ArrayWindowedTestInput<'t> =
    {
        InputArray : 't[]
        WindowSize : int
        ExpectedArray : 't[][]
        Exception : Type option
    }

[<TestFixture>][<Category "Collections.Array">][<Category "FSharp.Core.Collections">]
type ArrayModule2() =

    [<Test>]
    member this.Length() =
        // integer array  
        let resultInt = Array.length [|1..8|]
        if resultInt <> 8 then Assert.Fail()
        
        // string array    
        let resultStr = Array.length [|"Lists"; "are";  "commonly" ; "list" |]
        if resultStr <> 4 then Assert.Fail()
        
        // empty array     
        let resultEpt = Array.length [| |]
        if resultEpt <> 0 then Assert.Fail()

        // null array
        let nullArr = null:string[]      
        CheckThrowsNullRefException (fun () -> Array.length  nullArr  |> ignore)  
        
        ()

    [<Test>]
    member this.Indexed() =
        // integer array
        let resultInt = Array.indexed [|10..2..20|]
        Assert.AreEqual([|(0,10);(1,12);(2,14);(3,16);(4,18);(5,20)|], resultInt)

        // string array
        let funcStr (x:int) (y:string) =  x+ y.Length
        let resultStr = Array.indexed [| "Lists"; "Are"; "Commonly"; "List" |]
        Assert.AreEqual([| (0,"Lists");(1,"Are");(2,"Commonly");(3,"List") |], resultStr)

        // empty array
        let emptyArr:int[] = [| |]
        let resultEpt = Array.indexed emptyArr
        Assert.AreEqual([| |], resultEpt)

        // null array
        let nullArr = null:string[]
        CheckThrowsArgumentNullException (fun () -> Array.indexed nullArr |> ignore)

        ()

    [<Test>]
    member this.Map() = 
        // integer array
        let funcInt x = 
                match x with
                | _ when x % 2 = 0 -> 10*x            
                | _ -> x
        let resultInt = Array.map funcInt [| 1..10 |]
        if resultInt <> [|1;20;3;40;5;60;7;80;9;100|] then Assert.Fail()
        
        // string array
        let funcStr (x:string) = x.ToLower()
        let resultStr = Array.map funcStr [|"Lists"; "Are";  "Commonly" ; "List" |]
        if resultStr <> [|"lists"; "are";  "commonly" ; "list" |] then Assert.Fail()
        
        // empty array
        let resultEpt = Array.map funcInt [| |]
        if resultEpt <> [| |] then Assert.Fail()

        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.map funcStr nullArr |> ignore)
        
        ()

    [<Test>]
    member this.Map2() = 
        // integer array 
        let funcInt x y = x+y
        let resultInt = Array.map2 funcInt [|1..10|] [|2..2..20|]
        if resultInt <> [|3;6;9;12;15;18;21;24;27;30|] then Assert.Fail()
        
        // string array
        let funcStr (x:int) (y:string) =  x+ y.Length
        let resultStr = Array.map2 funcStr [|3;6;9;11|] [|"Lists"; "Are";  "Commonly" ; "List" |]
        if resultStr <> [|8;9;17;15|] then Assert.Fail()
        
        // empty array
        let emptyArr:int[] = [| |]
        let resultEpt = Array.map2 funcInt emptyArr emptyArr
        if resultEpt <> [| |] then Assert.Fail()

        // null array
        let nullArr = null:int[]
        let validArray = [| 1 |]       
        CheckThrowsArgumentNullException (fun () -> Array.map2 funcInt nullArr validArray |> ignore)  
        CheckThrowsArgumentNullException (fun () -> Array.map2 funcInt validArray nullArr |> ignore)  
        
        // len1 <> len2
        CheckThrowsArgumentException(fun () -> Array.map2 funcInt [|1..10|] [|2..20|] |> ignore)
        
        ()

    [<Test>]
    member this.Map3() =
        // Integer array
        let funcInt a b c = (a + b) * c
        let resultInt = Array.map3 funcInt [| 1..8 |] [| 2..9 |] [| 3..10 |]
        if resultInt <> [| 9; 20; 35; 54; 77; 104; 135; 170 |] then Assert.Fail()

        // First array is shorter
        CheckThrowsArgumentException (fun () -> Array.map3 funcInt [| 1..2 |] [| 2..9 |] [| 3..10 |] |> ignore)
        // Second array is shorter
        CheckThrowsArgumentException (fun () -> Array.map3 funcInt [| 1..8 |] [| 2..6 |] [| 3..10 |] |> ignore)
        // Third array is shorter
        CheckThrowsArgumentException (fun () -> Array.map3 funcInt [| 1..8 |] [| 2..9 |] [| 3..6 |] |> ignore)
        
        // String array
        let funcStr a b c = a + b + c
        let resultStr = Array.map3 funcStr [| "A";"B";"C";"D" |] [| "a";"b";"c";"d" |] [| "1";"2";"3";"4" |]
        if resultStr <> [| "Aa1";"Bb2";"Cc3";"Dd4" |] then Assert.Fail()

        // Empty array
        let resultEmpty = Array.map3 funcStr [||] [||] [||]
        if resultEmpty <> [||] then Assert.Fail()

        // Null array
        let nullArray = null : int[]
        let nonNullArray = [|1|]
        CheckThrowsArgumentNullException (fun () -> Array.map3 funcInt nullArray nonNullArray nonNullArray |> ignore)
        CheckThrowsArgumentNullException (fun () -> Array.map3 funcInt nonNullArray nullArray nonNullArray |> ignore)
        CheckThrowsArgumentNullException (fun () -> Array.map3 funcInt nonNullArray nonNullArray nullArray |> ignore)

        ()

    [<Test>]
    member this.MapFold() =
        // integer array
        let funcInt acc x = if x % 2 = 0 then 10*x, acc + 1 else x, acc
        let resultInt,resultIntAcc = Array.mapFold funcInt 100 [| 1..10 |]
        if resultInt <> [| 1;20;3;40;5;60;7;80;9;100 |] then Assert.Fail()
        Assert.AreEqual(105, resultIntAcc)

        // string array
        let funcStr acc (x:string) = match x.Length with 0 -> "empty", acc | _ -> x.ToLower(), sprintf "%s%s" acc x
        let resultStr,resultStrAcc = Array.mapFold funcStr "" [| "";"BB";"C";"" |]
        if resultStr <> [| "empty";"bb";"c";"empty" |] then Assert.Fail()
        Assert.AreEqual("BBC", resultStrAcc)

        // empty array
        let resultEpt,resultEptAcc = Array.mapFold funcInt 100 [| |]
        if resultEpt <> [| |] then Assert.Fail()
        Assert.AreEqual(100, resultEptAcc)

        // null array
        let nullArr = null:string[]
        CheckThrowsArgumentNullException (fun () -> Array.mapFold funcStr "" nullArr |> ignore)

        ()

    [<Test>]
    member this.MapFoldBack() =
        // integer array
        let funcInt x acc = if acc < 105 then 10*x, acc + 2 else x, acc
        let resultInt,resultIntAcc = Array.mapFoldBack funcInt [| 1..10 |] 100
        if resultInt <> [| 1;2;3;4;5;6;7;80;90;100 |] then Assert.Fail()
        Assert.AreEqual(106, resultIntAcc)

        // string array
        let funcStr (x:string) acc = match x.Length with 0 -> "empty", acc | _ -> x.ToLower(), sprintf "%s%s" acc x
        let resultStr,resultStrAcc = Array.mapFoldBack funcStr [| "";"BB";"C";"" |] ""
        if resultStr <> [| "empty";"bb";"c";"empty" |] then Assert.Fail()
        Assert.AreEqual("CBB", resultStrAcc)

        // empty array
        let resultEpt,resultEptAcc = Array.mapFoldBack funcInt [| |] 100
        if resultEpt <> [| |] then Assert.Fail()
        Assert.AreEqual(100, resultEptAcc)

        // null array
        let nullArr = null:string[]
        CheckThrowsArgumentNullException (fun () -> Array.mapFoldBack funcStr nullArr "" |> ignore)

        ()

    [<Test>]
    member this.Mapi() = 
        // integer array 
        let funcInt x y = x+y
        let resultInt = Array.mapi funcInt [|10..2..20|]
        if resultInt <> [|10;13;16;19;22;25|] then Assert.Fail()
        
        // string array
        let funcStr (x:int) (y:string) =  x+ y.Length
        let resultStr = Array.mapi funcStr  [|"Lists"; "Are";  "Commonly" ; "List" |]
        if resultStr <> [|5;4;10;7|] then Assert.Fail()
        
        // empty array
        let emptyArr:int[] = [| |]
        let resultEpt = Array.mapi funcInt emptyArr 
        if resultEpt <> [| |] then Assert.Fail()

        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.mapi funcStr nullArr |> ignore)  
        
        ()

    [<Test>]
    member this.mapi2() = 
        // integer array 
        let funcInt x y z = x+y+z
        let resultInt = Array.mapi2 funcInt [|1..10|] [|2..2..20|]
        if resultInt <> [|3;7;11;15;19;23;27;31;35;39|] then Assert.Fail()
        
        // string array
        let funcStr  z (x:int) (y:string)  =z + x+ y.Length 
        let resultStr = Array.mapi2 funcStr [|3;6;9;11|] [|"Lists"; "Are";  "Commonly" ; "List" |]
        if resultStr <> [|8;10;19;18|] then Assert.Fail()
        
        // empty array
        let emptyArr:int[] = [| |]
        let resultEpt = Array.mapi2 funcInt emptyArr emptyArr
        if resultEpt <> [| |] then Assert.Fail()

        // null array
        let nullArr = null:int[] 
        let validArray = [| 1 |]      
        CheckThrowsArgumentNullException (fun () -> Array.mapi2 funcInt validArray  nullArr  |> ignore)  
        CheckThrowsArgumentNullException (fun () -> Array.mapi2 funcInt  nullArr validArray |> ignore)  
        
        // len1 <> len2
        CheckThrowsArgumentException(fun () -> Array.mapi2 funcInt [|1..10|] [|2..20|] |> ignore)
        
        ()

    [<Test>]
    member this.Max() = 
        // integer array 
        let resultInt = Array.max  [|2..2..20|]
        if resultInt <> 20 then Assert.Fail()
        
        // string array
        let resultStr = Array.max [|"t"; "ahe"; "Lists"; "Are";  "Commonly" ; "List";"a" |]
        if resultStr <> "t" then Assert.Fail()
        
        // empty array -- argumentexception   
        
        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.max   nullArr  |> ignore)  
        
        // len = 0
        CheckThrowsArgumentException(fun() -> Array.max  [||] |> ignore)
        
        ()

    [<Test>]
    member this.MaxBy()= 
        // integer array 
        let funcInt x = x%8
        let resultInt = Array.maxBy funcInt [|2..2..20|]
        if resultInt <> 6 then Assert.Fail()
        
        // string array
        let funcStr (x:string) = x.Length 
        let resultStr = Array.maxBy funcStr  [|"Lists"; "Are";  "Commonly" ; "List"|]
        if resultStr <> "Commonly" then Assert.Fail()    
        
        // empty array -- argumentexception    

        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.maxBy funcStr   nullArr  |> ignore)  
        
        // len = 0
        CheckThrowsArgumentException(fun() -> Array.maxBy funcInt (Array.empty<int>) |> ignore)
        
        ()

    [<Test>]
    member this.Min() =
        // integer array 
        let resultInt = Array.min  [|3;7;8;9;4;1;1;2|]
        if resultInt <> 1 then Assert.Fail()
        
        // string array
        let resultStr = Array.min [|"a"; "Lists";  "Commonly" ; "List"  |] 
        if resultStr <> "Commonly" then Assert.Fail()
        
        // empty array -- argumentexception   
        
        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.min   nullArr  |> ignore)  
        
        // len = 0
        CheckThrowsArgumentException(fun () -> Array.min  [||] |> ignore)
        
        () 

    [<Test>]
    member this.MinBy()= 
        // integer array 
        let funcInt x = x%8
        let resultInt = Array.minBy funcInt [|3;7;9;4;8;1;1;2|]
        if resultInt <> 8 then Assert.Fail()
        
        // string array
        let funcStr (x:string) = x.Length 
        let resultStr = Array.minBy funcStr  [|"Lists"; "Are";  "Commonly" ; "List"|]
        if resultStr <> "Are" then Assert.Fail()    
        
        // empty array -- argumentexception    

        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.minBy funcStr   nullArr  |> ignore)  
        
        // len = 0
        CheckThrowsArgumentException(fun () -> Array.minBy funcInt (Array.empty<int>) |> ignore)
        
        ()
        

    [<Test>]
    member this.Of_List() =
        // integer array  
        let resultInt = Array.ofList [1..10]
        if resultInt <> [|1..10|] then Assert.Fail()
        
        // string array    
        let resultStr = Array.ofList ["Lists"; "are";  "commonly" ; "list" ]
        if resultStr <> [| "Lists"; "are";  "commonly" ; "list" |] then Assert.Fail()
        
        // empty array     
        let resultEpt = Array.ofList []
        if resultEpt <> [||] then Assert.Fail()

        // null array
        
        ()

    [<Test>]
    member this.Of_Seq() =
        // integer array  
        let resultInt = Array.ofSeq {1..10}
        if resultInt <> [|1..10|] then Assert.Fail()
        
        // string array    
        let resultStr = Array.ofSeq (seq {for x in 'a'..'f' -> x.ToString()})
        if resultStr <> [| "a";"b";"c";"d";"e";"f" |] then Assert.Fail()
        
        // empty array     
        let resultEpt = Array.ofSeq []
        if resultEpt <> [| |] then Assert.Fail()

        // null array
        
        ()

    [<Test>]
    member this.Partition() =
        // integer array  
        let resultInt = Array.partition (fun x -> x%3 = 0) [|1..10|]
        if resultInt <> ([|3;6;9|], [|1;2;4;5;7;8;10|]) then Assert.Fail()
        
        // string array    
        let resultStr = Array.partition (fun (x:string) -> x.Length >4) [|"Lists"; "are";  "commonly" ; "list" |]
        if resultStr <> ([|"Lists";"commonly"|],[|"are"; "list"|]) then Assert.Fail()
        
        // empty array     
        let resultEpt = Array.partition (fun x -> x%3 = 0) [||]
        if resultEpt <> ([||],[||]) then Assert.Fail()

        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.partition (fun (x:string) -> x.Length >4)  nullArr  |> ignore)  
        
        ()

    [<Test>]
    member this.Permute() =
        // integer array  
        let resultInt = Array.permute (fun i -> (i+1) % 4) [|1;2;3;4|]
        if resultInt <> [|4;1;2;3|] then Assert.Fail()
        
        // string array    
        let resultStr = Array.permute (fun i -> (i+1) % 4) [|"Lists"; "are";  "commonly" ; "list" |]
        if resultStr <> [|"list";"Lists"; "are";  "commonly" |] then Assert.Fail()
        
        // empty array     
        let resultEpt = Array.permute (fun i -> (i+1) % 4) [||]
        if resultEpt <> [||] then Assert.Fail()
    
        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.permute (fun i -> (i+1) % 4)  nullArr  |> ignore)   
        
        ()

    [<Test>]
    member this.Reduce() =
        // integer array  
        let resultInt = Array.reduce (fun x y -> x/y) [|5*4*3*2; 4;3;2;1|]
        if resultInt <> 5 then Assert.Fail()
        
        // string array    
        let resultStr = Array.reduce (fun (x:string) (y:string) -> x.Remove(0,y.Length)) [|"ABCDE";"A"; "B";  "C" ; "D" |]
        if resultStr <> "E" then  Assert.Fail()
        
        // empty array 
        CheckThrowsArgumentException (fun () -> Array.reduce (fun x y -> x/y)  [||] |> ignore)

        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.reduce (fun (x:string) (y:string) -> x.Remove(0,y.Length))  nullArr  |> ignore)   
        
        ()

        
    [<Test>]
    member this.ReduceBack() =
        // integer array  
        let resultInt = Array.reduceBack (fun x y -> x/y) [|5*4*3*2; 4;3;2;1|]
        if resultInt <> 30 then Assert.Fail()
        
        // string array    
        let resultStr = Array.reduceBack (fun (x:string) (y:string) -> x.Remove(0,y.Length)) [|"ABCDE";"A"; "B";  "C" ; "D" |]
        if resultStr <> "ABCDE" then  Assert.Fail()
        
        // empty array 
        CheckThrowsArgumentException (fun () -> Array.reduceBack (fun x y -> x/y) [||] |> ignore)

        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.reduceBack (fun (x:string) (y:string) -> x.Remove(0,y.Length))  nullArr  |> ignore)   
        
        ()
    

    [<Test>]
    member this.Rev() =
        // integer array  
        let resultInt = Array.rev  [|1..10|]
        if resultInt <> [|10;9;8;7;6;5;4;3;2;1|] then Assert.Fail()
        
        // string array    
        let resultStr = Array.rev  [|"Lists"; "are";  "commonly" ; "list" |]
        if resultStr <> [|"list"; "commonly"; "are"; "Lists" |] then Assert.Fail()
        
        // empty array     
        let resultEpt = Array.rev  [||]
        if resultEpt <> [||] then Assert.Fail()

        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.rev  nullArr  |> ignore) 
        ()

    [<Test>] 
    member this.Scan() =
        // integer array
        let funcInt x y = x+y
        let resultInt = Array.scan funcInt 9 [| 1..10 |]
        if resultInt <> [|9;10;12;15;19;24;30;37;45;54;64|] then Assert.Fail()
        
        // string array
        let funcStr x y = x+y        
        let resultStr = Array.scan funcStr "x" [|"A"; "B";  "C" ; "D" |]
        if resultStr <> [|"x";"xA";"xAB";"xABC";"xABCD"|] then Assert.Fail()
        
        // empty array
        let resultEpt = Array.scan funcInt 5 [| |]
        if resultEpt <> [|5|] then Assert.Fail()

        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.scan funcStr "begin"  nullArr  |> ignore)  
        
        ()   
    
    [<Test>]
    member this.ScanBack() =
        // integer array 
        let funcInt x y = x+y
        let resultInt = Array.scanBack funcInt [| 1..10 |] 9
        if resultInt <> [|64;63;61;58;54;49;43;36;28;19;9|] then Assert.Fail()
        
        // string array
        let funcStr x y = x+y        
        let resultStr = Array.scanBack funcStr [|"A"; "B";  "C" ; "D" |] "X" 
        if resultStr <> [|"ABCDX";"BCDX";"CDX";"DX";"X"|] then Assert.Fail()
        
        // empty array
        let resultEpt = Array.scanBack funcInt [| |] 5 
        if resultEpt <> [|5|] then Assert.Fail()

        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.scanBack funcStr nullArr "begin"  |> ignore) 
        
        ()

    [<Test>]
    member this.Skip() =
        // integer array
        let resultInt = Array.skip 2 [|1..10|]
        if resultInt <> [|3..10|] then Assert.Fail()
        
        let resultInt2 = Array.skip 0 [|1..10|]
        if resultInt2 <> [|1..10|] then Assert.Fail()
        
        let resultInt3 = Array.skip -5 [|1..10|]
        if resultInt3 <> [|1..10|] then Assert.Fail()

        // string List
        let resultStr = Array.skip 2 [|"str1";"str2";"str3";"str4"|]
        if resultStr <> [|"str3";"str4"|] then Assert.Fail()

        // empty List
        let resultEpt = Array.skip 0 [||]
        if resultEpt <> [||] then Assert.Fail()

        // exceptions
        CheckThrowsArgumentNullException (fun () -> Array.skip 0 (null:string[]) |> ignore)
        CheckThrowsArgumentNullException (fun () -> Array.skip -3 (null:string[]) |> ignore)
        CheckThrowsArgumentException (fun () -> Array.skip 1 [||] |> ignore)
        CheckThrowsArgumentException (fun () -> Array.skip 4 [|1; 2; 3|] |> ignore)

    [<Test>]
    member this.SkipWhile() =
        // integer array
        let funcInt x = (x < 4)
        let intArr = [|1..10|]
        let resultInt = Array.skipWhile funcInt intArr
        if resultInt <> [|4..10|] then Assert.Fail()

        // string array
        let funcStr (s:string) = s.Length < 8
        let strArr = [| "Lists"; "are";  "commonly" ; "list" |]
        let resultStr = Array.skipWhile funcStr strArr
        if resultStr <> [| "commonly" ; "list" |] then Assert.Fail()

        // empty array
        let resultEmpt = Array.skipWhile (fun _ -> failwith "unexpected error") [| |]
        if resultEmpt <> [| |] then Assert.Fail()

        // null array
        CheckThrowsArgumentNullException (fun () -> Array.skipWhile (fun _ -> failwith "unexpected error") null |> ignore)

        // skip all
        let resultAll = Array.skipWhile (fun _ -> true) intArr
        if resultAll <> [| |] then Assert.Fail()

        // skip none
        let resultNone = Array.skipWhile (fun _ -> false) intArr
        if resultNone <> intArr then Assert.Fail()

        ()

    [<Test>]
    member this.Set() =
        // integer array  
        let intArr = [|10;9;8;7|]
        Array.set intArr  3 600
        if intArr <> [|10;9;8;600|] then Assert.Fail()  
        
        // string array
        let strArr = [|"Lists"; "are";  "commonly" ; "list" |]    
        Array.set strArr 2 "always"
        if strArr <> [|"Lists"; "are";  "always" ; "list" |]     then Assert.Fail()
        
        // empty array -- outofbundaryexception
        
        // null array
        let nullArr = null:string[]      
        CheckThrowsNullRefException (fun () -> Array.set nullArr 0 "null"   |> ignore)
        
        ()    

    [<Test>]
    member this.sortInPlaceWith() =
        // integer array  
        let intArr = [|3;5;7;2;4;8|]
        Array.sortInPlaceWith compare intArr  
        if intArr <> [|2;3;4;5;7;8|] then Assert.Fail()  

        // Sort backwards
        let intArr = [|3;5;7;2;4;8|]
        Array.sortInPlaceWith (fun a b -> -1 * compare a b) intArr  
        if intArr <> [|8;7;5;4;3;2|] then Assert.Fail()  
        
        // string array
        let strArr = [|"Lists"; "are"; "a"; "commonly"; "used"; "data"; "structure"|]    
        Array.sortInPlaceWith compare strArr 
        if strArr <> [| "Lists"; "a"; "are"; "commonly"; "data"; "structure"; "used"|]     then Assert.Fail()
        
        // empty array
        let emptyArr:int[] = [| |]
        Array.sortInPlaceWith compare emptyArr
        if emptyArr <> [||] then Assert.Fail()
        
        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.sortInPlaceWith compare nullArr  |> ignore)  
        
        // len = 2  
        let len2Arr = [|8;3|]      
        Array.sortInPlaceWith compare len2Arr
        Assert.AreEqual([|3;8|], len2Arr)
        
        // Equal elements
        let eights = [|8; 8;8|]      
        Array.sortInPlaceWith compare eights
        Assert.AreEqual([|8;8;8|], eights)
        
        ()   
        

    [<Test>]
    member this.sortInPlaceBy() =
        // integer array  
        let intArr = [|3;5;7;2;4;8|]
        Array.sortInPlaceBy int intArr  
        if intArr <> [|2;3;4;5;7;8|] then Assert.Fail()  
        
        // string array
        let strArr = [|"Lists"; "are"; "a"; "commonly"; "used"; "data"; "structure"|]    
        Array.sortInPlaceBy (fun (x:string) -> x.Length)  strArr 
        // note: Array.sortInPlaceBy is not stable, so we allow 2 results.
        if strArr <> [| "a"; "are";"data"; "used";"Lists"; "commonly";"structure"|] && strArr <> [| "a"; "are"; "used"; "data"; "Lists"; "commonly";"structure"|]    then Assert.Fail()
        
        // empty array
        let emptyArr:int[] = [| |]
        Array.sortInPlaceBy int emptyArr
        if emptyArr <> [||] then Assert.Fail()
        
        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.sortInPlaceBy (fun (x:string) -> x.Length) nullArr |> ignore)  
        
        // len = 2  
        let len2Arr = [|8;3|]      
        Array.sortInPlaceBy int len2Arr
        if len2Arr <> [|3;8|] then Assert.Fail()  
        Assert.AreEqual([|3;8|],len2Arr)  
        
        () 
        
    [<Test>]
    member this.SortDescending() =
        // integer array  
        let intArr = [|3;5;7;2;4;8|]
        let resultInt = Array.sortDescending intArr  
        Assert.AreEqual([|8;7;5;4;3;2|], resultInt)
        
        // string Array
        let strArr = [|"Z";"a";"d"; ""; "Y"; null; "c";"b";"X"|]   
        let resultStr = Array.sortDescending strArr         
        Assert.AreEqual([|"d"; "c"; "b"; "a"; "Z"; "Y"; "X"; ""; null|], resultStr)
        
        // empty array
        let emptyArr:int[] = [| |]
        let resultEmpty = Array.sortDescending emptyArr
        if resultEmpty <> [||] then Assert.Fail()
        
        // tuple array
        let tupArr = [|(2,"a");(1,"d");(1,"b");(1,"a");(2,"x");(2,"b");(1,"x")|]   
        let resultTup = Array.sortDescending tupArr         
        Assert.AreEqual([|(2,"x");(2,"b");(2,"a");(1,"x");(1,"d");(1,"b");(1,"a")|], resultTup)

        // date array
        let dateArr = [|DateTime(2014,12,31);DateTime(2014,1,1);DateTime(2015,1,1);DateTime(2013,12,31);DateTime(2014,1,1)|]   
        let resultDate = Array.sortDescending dateArr         
        Assert.AreEqual([|DateTime(2014,12,31);DateTime(2014,1,1);DateTime(2015,1,1);DateTime(2013,12,31);DateTime(2014,1,1)|], dateArr)
        Assert.AreEqual([|DateTime(2015,1,1);DateTime(2014,12,31);DateTime(2014,1,1);DateTime(2014,1,1);DateTime(2013,12,31)|], resultDate)

        // float array
        let minFloat,maxFloat,epsilon = System.Double.MinValue,System.Double.MaxValue,System.Double.Epsilon
        let floatArr = [| 0.0; 0.5; 2.0; 1.5; 1.0; minFloat; maxFloat; epsilon; -epsilon |]
        let resultFloat = Array.sortDescending floatArr
        Assert.AreEqual([| maxFloat; 2.0; 1.5; 1.0; 0.5; epsilon; 0.0; -epsilon; minFloat; |], resultFloat)

        () 
        
    [<Test>]
    member this.SortByDescending() =
        // integer array  
        let intArr = [|3;5;7;2;4;8|]
        let resultInt = Array.sortByDescending int intArr           
        Assert.AreEqual([|3;5;7;2;4;8|], intArr)
        Assert.AreEqual([|8;7;5;4;3;2|], resultInt)
                
        // string array
        let strArr = [|".."; ""; "..."; "."; "...."|]    
        let resultStr = Array.sortByDescending (fun (x:string) -> x.Length)  strArr 
        Assert.AreEqual([|".."; ""; "..."; "."; "...."|], strArr)
        Assert.AreEqual([|"....";"...";"..";"."; ""|], resultStr)
        
        // empty array
        let emptyArr:int[] = [| |]
        let resultEmpty = Array.sortByDescending int emptyArr        
        if resultEmpty <> [||] then Assert.Fail()    
        
        // tuple array
        let tupArr = [|(2,"a");(1,"d");(1,"b");(2,"x")|]
        let sndTup = Array.sortByDescending snd tupArr         
        Assert.AreEqual( [|(2,"a");(1,"d");(1,"b");(2,"x")|] , tupArr)
        Assert.AreEqual( [|(2,"x");(1,"d");(1,"b");(2,"a")|] , sndTup)
        
        // date array
        let dateArr = [|DateTime(2013,12,31);DateTime(2014,2,1);DateTime(2015,1,1);DateTime(2014,3,1)|]
        let resultDate = Array.sortByDescending (fun (d:DateTime) -> d.Month) dateArr         
        Assert.AreEqual([|DateTime(2013,12,31);DateTime(2014,2,1);DateTime(2015,1,1);DateTime(2014,3,1)|], dateArr)
        Assert.AreEqual([|DateTime(2013,12,31);DateTime(2014,3,1);DateTime(2014,2,1);DateTime(2015,1,1)|], resultDate)

        // float array
        let minFloat,maxFloat,epsilon = System.Double.MinValue,System.Double.MaxValue,System.Double.Epsilon
        let floatArr = [| 0.0; 0.5; 2.0; 1.5; 1.0; minFloat; maxFloat; epsilon; -epsilon |]
        let resultFloat = Array.sortByDescending id floatArr
        Assert.AreEqual([| maxFloat; 2.0; 1.5; 1.0; 0.5; epsilon; 0.0; -epsilon; minFloat; |], resultFloat)

        ()  
         
    [<Test>]
    member this.Sub() =
        // integer array  
        let resultInt = Array.sub [|1..8|] 3 3
        if resultInt <> [|4;5;6|] then Assert.Fail()
        
        // string array    
        let resultStr = Array.sub [|"Lists"; "are";  "commonly" ; "list" |] 1 2
        if resultStr <> [|"are";  "commonly" |] then Assert.Fail()
        
        // empty array     
        let resultEpt = Array.sub [| |] 0 0
        if resultEpt <> [||] then Assert.Fail()

        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.sub nullArr 1 1 |> ignore)  
        
        // bounds
        CheckThrowsArgumentException (fun () -> Array.sub resultInt -1 2 |> ignore)
        CheckThrowsArgumentException (fun () -> Array.sub resultInt 1 -2 |> ignore)
        CheckThrowsArgumentException (fun () -> Array.sub resultInt 1 20 |> ignore)
        
        ()

    [<Test>]
    member this.Sum() =
        // empty integer array 
        let resultEptInt = Array.sum ([||]:int[]) 
        if resultEptInt <> 0 then Assert.Fail()    
        
        // empty float32 array
        let emptyFloatArray = Array.empty<System.Single> 
        let resultEptFloat = Array.sum emptyFloatArray 
        if resultEptFloat <> 0.0f then Assert.Fail()
        
        // empty double array
        let emptyDoubleArray = Array.empty<System.Double> 
        let resultDouEmp = Array.sum emptyDoubleArray 
        if resultDouEmp <> 0.0 then Assert.Fail()
        
        // empty decimal array
        let emptyDecimalArray = Array.empty<System.Decimal> 
        let resultDecEmp = Array.sum emptyDecimalArray 
        if resultDecEmp <> 0M then Assert.Fail()

        // integer array  
        let resultInt = Array.sum [|1..10|] 
        if resultInt <> 55 then Assert.Fail()  
        
        // float32 array
        let floatArray: float32[] = [| 1.1f; 1.1f; 1.1f |]
        let resultFloat = Array.sum floatArray
        if resultFloat < 3.3f - 0.001f || resultFloat > 3.3f + 0.001f then
            Assert.Fail()
        
        // double array
        let doubleArray: System.Double[] = [| 1.0; 8.0 |]
        let resultDouble = Array.sum doubleArray
        if resultDouble <> 9.0 then Assert.Fail()
        
        // decimal array
        let decimalArray: decimal[] = [| 0M; 19M; 19.03M |]
        let resultDecimal = Array.sum decimalArray
        if resultDecimal <> 38.03M then Assert.Fail()      
 
        // null array
        let nullArr = null:double[]    
        CheckThrowsArgumentNullException (fun () -> Array.sum  nullArr  |> ignore) 
        ()

    [<Test>]
    member this.SumBy() =
        // empty integer array         
        let resultEptInt = Array.sumBy int ([||]:int[]) 
        if resultEptInt <> 0 then Assert.Fail()    
        
        // empty float32 array
        let emptyFloatArray = Array.empty<System.Single> 
        let resultEptFloat = Array.sumBy float32 emptyFloatArray 
        if resultEptFloat <> 0.0f then Assert.Fail()
        
        // empty double array
        let emptyDoubleArray = Array.empty<System.Double> 
        let resultDouEmp = Array.sumBy float emptyDoubleArray 
        if resultDouEmp <> 0.0 then Assert.Fail()
        
        // empty decimal array
        let emptyDecimalArray = Array.empty<System.Decimal> 
        let resultDecEmp = Array.sumBy decimal emptyDecimalArray 
        if resultDecEmp <> 0M then Assert.Fail()

        // integer array  
        let resultInt = Array.sumBy int [|1..10|] 
        if resultInt <> 55 then Assert.Fail()  
        
        // float32 array
        let floatArray: string[] = [| "1.2";"3.5";"6.7" |]
        let resultFloat = Array.sumBy float32 floatArray
        if resultFloat <> 11.4f then Assert.Fail()
        
        // double array
        let doubleArray: System.Double[] = [| 1.0;8.0 |]
        let resultDouble = Array.sumBy float doubleArray
        if resultDouble <> 9.0 then Assert.Fail()
        
        // decimal array
        let decimalArray: decimal[] = [| 0M;19M;19.03M |]
        let resultDecimal = Array.sumBy decimal decimalArray
        if resultDecimal <> 38.03M then Assert.Fail()      
        
        // null array
        let nullArr = null:double[]    
        CheckThrowsArgumentNullException (fun () -> Array.sumBy float32  nullArr  |> ignore) 
        ()

    [<Test>]
    member this.Tl() =
        // integer array  
        let resultInt = Array.tail [|1..10|]        
        Assert.AreEqual([|2..10|], resultInt)
        
        // string array    
        let resultStr = Array.tail [| "a"; "b"; "c"; "d" |]        
        Assert.AreEqual([| "b";  "c" ; "d" |], resultStr)
        
        // 1-element array    
        let resultStr2 = Array.tail [| "a" |]        
        Assert.AreEqual([| |], resultStr2)

        CheckThrowsArgumentException(fun () -> Array.tail [||] |> ignore)

        CheckThrowsArgumentNullException(fun () -> Array.tail null |> ignore)
        ()

    [<Test>]
    member this.To_List() =
        // integer array  
        let resultInt = Array.toList [|1..10|]
        if resultInt <> [1..10] then Assert.Fail()
        
        // string array    
        let resultStr = Array.toList [|"Lists"; "are";  "commonly" ; "list" |]
        if resultStr <> ["Lists"; "are";  "commonly" ; "list"] then Assert.Fail()
        
        // empty array     
        let resultEpt = Array.toList  [||]
        if resultEpt <> [] then Assert.Fail()

        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.toList   nullArr  |> ignore)  
        
        ()    
        
    [<Test>]
    member this.To_Seq() =
        // integer array  
        let resultInt = [|1..10|] |> Array.toSeq  |> Array.ofSeq
        if resultInt <> [|1..10|] then Assert.Fail()
        
        // string array    
        let resultStr = [|"Lists"; "are";  "commonly" ; "list" |] |> Array.toSeq |> Array.ofSeq
        if resultStr <> [|"Lists"; "are";  "commonly" ; "list" |] then Assert.Fail()
        
        // empty array     
        let resultEpt =[||] |> Array.toSeq  |> Array.ofSeq
        if resultEpt <> [||]  then Assert.Fail()

        // null array
        let nullArr = null:string[]  
        CheckThrowsArgumentNullException (fun () -> nullArr  |> Array.toSeq   |> ignore)  
        
        ()   

    [<Test>]
    member this.Transpose() =
        // integer array
        Assert.AreEqual([|[|1;4|]; [|2;5|]; [|3;6|]|], Array.transpose (seq [[|1..3|]; [|4..6|]]))
        Assert.AreEqual([|[|1|]; [|2|]; [|3|]|], Array.transpose [|[|1..3|]|])
        Assert.AreEqual([|[|1..2|]|], Array.transpose [|[|1|]; [|2|]|])

        // string array
        Assert.AreEqual([|[|"a";"d"|]; [|"b";"e"|]; [|"c";"f"|]|], Array.transpose (seq [[|"a";"b";"c"|]; [|"d";"e";"f"|]]))

        // empty array
        Assert.AreEqual([| |], Array.transpose [| |])

        // array of empty arrays - m x 0 array transposes to 0 x m (i.e. empty)
        Assert.AreEqual([| |], Array.transpose [| [||] |])
        Assert.AreEqual([| |], Array.transpose [| [||]; [||] |])

        // null array
        let nullArr = null: string[][]
        CheckThrowsArgumentNullException (fun () -> nullArr |> Array.transpose |> ignore)

        // jagged arrays
        CheckThrowsArgumentException (fun () -> Array.transpose [| [|1; 2|]; [|3|] |] |> ignore)
        CheckThrowsArgumentException (fun () -> Array.transpose [| [|1|]; [|2; 3|] |] |> ignore)

    [<Test>]
    member this.Truncate() =
        // integer array
        Assert.AreEqual([|1..3|], Array.truncate 3 [|1..5|])
        Assert.AreEqual([|1..5|], Array.truncate 10 [|1..5|])
        Assert.AreEqual([| |], Array.truncate 0 [|1..5|])

        // string array
        Assert.AreEqual([|"str1";"str2"|], Array.truncate 2 [|"str1";"str2";"str3"|])

        // empty array
        Assert.AreEqual([| |], Array.truncate 0 [| |])
        Assert.AreEqual([| |], Array.truncate 1 [| |])

        // null array
        CheckThrowsArgumentNullException(fun() -> Array.truncate 1 null |> ignore)

        // negative count
        Assert.AreEqual([| |], Array.truncate -1 [|1..5|])
        Assert.AreEqual([| |], Array.truncate System.Int32.MinValue [|1..5|])

        ()

    [<Test>]
    member this.TryFind() =
        // integer array  
        let resultInt = [|1..10|] |> Array.tryFind (fun x -> x%7 = 0)  
        if resultInt <> Some 7 then Assert.Fail()
        
        // string array    
        let resultStr = [|"Lists"; "are";  "commonly" ; "list" |] |> Array.tryFind (fun (x:string) -> x.Length > 4)
        if resultStr <> Some "Lists" then Assert.Fail()
        
        // empty array     
        let resultEpt =[||] |> Array.tryFind  (fun x -> x%7 = 0)  
        if resultEpt <> None  then Assert.Fail()

        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.tryFind (fun (x:string) -> x.Length > 4)  nullArr  |> ignore)  
        
        ()
        
    [<Test>]
    member this.TryFindBack() =
        // integer array
        let funcInt x = x%5 = 0
        Assert.AreEqual(Some 20, [| 1..20 |] |> Array.tryFindBack funcInt)
        Assert.AreEqual(Some 15, [| 1..19 |] |> Array.tryFindBack funcInt)
        Assert.AreEqual(Some 5, [| 5..9 |] |> Array.tryFindBack funcInt)

        // string array
        let resultStr = [|"Lists"; "are";  "commonly" ; "list" |] |> Array.tryFindBack (fun (x:string) -> x.Length > 4)
        Assert.AreEqual(Some "commonly", resultStr)

        // empty array
        Assert.AreEqual(None, [| |] |> Array.tryFindBack (fun _ -> failwith "error"))

        // not found
        Assert.AreEqual(None, [| 1..20 |] |> Array.tryFindBack (fun _ -> false))

        // null array
        let nullArr = null:string[]
        CheckThrowsArgumentNullException (fun () -> Array.tryFindBack (fun _ -> failwith "error") nullArr |> ignore)

        ()

    [<Test>]
    member this.TryFindIndex() =
        // integer array  
        let resultInt = [|1..10|] |> Array.tryFindIndex (fun x -> x%7 = 0)  
        if resultInt <> Some 6 then Assert.Fail()
        
        // string array    
        let resultStr = [|"Lists"; "are";  "commonly" ; "list" |] |> Array.tryFindIndex (fun (x:string) -> x.Length > 4)
        if resultStr <> Some 0 then Assert.Fail()
        
        // empty array     
        let resultEpt =[||] |> Array.tryFindIndex  (fun x -> x % 7 = 0)  
        if resultEpt <> None  then Assert.Fail()

        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.tryFindIndex (fun (x:string) -> x.Length > 4)  nullArr  |> ignore)  
        
        ()

    [<Test>]
    member this.TryFindIndexBack() =
        // integer array
        let funcInt x = x%5 = 0
        Assert.AreEqual(Some 19, [| 1..20 |] |> Array.tryFindIndexBack funcInt)
        Assert.AreEqual(Some 14, [| 1..19 |] |> Array.tryFindIndexBack funcInt)
        Assert.AreEqual(Some 0, [| 5..9 |] |> Array.tryFindIndexBack funcInt)

        // string array
        let resultStr = [|"Lists"; "are";  "commonly" ; "list" |] |> Array.tryFindIndexBack (fun (x:string) -> x.Length > 4)
        Assert.AreEqual(Some 2, resultStr)

        // empty array
        Assert.AreEqual(None, [| |] |> Array.tryFindIndexBack (fun _ -> true))

        // not found
        Assert.AreEqual(None, [| 1..20 |] |> Array.tryFindIndexBack (fun _ -> false))

        // null array
        let nullArr = null:string[]
        CheckThrowsArgumentNullException (fun () -> Array.tryFindIndexBack (fun (x:string) -> x.Length > 4) nullArr |> ignore)

        ()

    [<Test>]
    member this.Unfold() =
        // integer Seq
        let resultInt = Array.unfold (fun x -> if x < 20 then Some (x+1,x*2) else None) 1
        Assert.AreEqual([|2;3;5;9;17|], resultInt)

        // string Seq
        let resultStr = Array.unfold (fun (x:string) -> if x.Contains("unfold") then Some("a","b") else None) "unfold"
        Assert.AreEqual([|"a"|], resultStr)

        // empty seq
        let resultEpt = Array.unfold (fun _ -> None) 1
        Assert.AreEqual([| |], resultEpt)

        ()

    [<Test>]
    member this.Unzip() =
        // integer array  
        let resultInt =  Array.unzip [|(1,2);(2,4);(3,6)|] 
        if resultInt <>  ([|1..3|], [|2..2..6|]) then Assert.Fail()
        
        // string array    
        let resultStr = Array.unzip [|("A","a");("B","b");("C","c");("D","d")|]
        let str = resultStr.ToString()
        if resultStr <> ([|"A"; "B";  "C" ; "D" |],[|"a";"b";"c";"d"|]) then Assert.Fail()
        
        // empty array     
        let resultEpt = Array.unzip  [||]
        if resultEpt <> ([||],[||])  then Assert.Fail()

        // null array
        
        ()

    [<Test>]
    member this.Unzip3() =
        // integer array  
        let resultInt =  Array.unzip3 [|(1,2,3);(2,4,6);(3,6,9)|]
        if resultInt <> ([|1;2;3|], [|2;4;6|], [|3;6;9|]) then Assert.Fail()
        
        // string array    
        let resultStr = Array.unzip3 [|("A","1","a");("B","2","b");("C","3","c");("D","4","d")|]
        if resultStr <> ([|"A"; "B";  "C" ; "D" |], [|"1";"2";"3";"4"|], [|"a"; "b"; "c"; "d"|]) then Assert.Fail()
        
        // empty array     
        let resultEpt = Array.unzip3  [||]
        if resultEpt <>  ([||], [||], [||]) then Assert.Fail()

        // null array
        
        ()

    [<Test>]
    member this.Windowed() =
        let testWindowed config =
            try
                config.InputArray
                |> Array.windowed config.WindowSize
                |> (fun actual -> Assert.IsTrue(config.ExpectedArray = actual))
            with
            | _ when Option.isNone config.Exception -> Assert.Fail()
            | e when e.GetType() = (Option.get config.Exception) -> ()
            | _ -> Assert.Fail()

        {
          InputArray = [|1..10|]
          WindowSize = 1
          ExpectedArray =  [| for i in 1..10 do yield [| i |] |]
          Exception = None
        } |> testWindowed
        {
          InputArray = [|1..10|]
          WindowSize = 5
          ExpectedArray =  [| for i in 1..6 do yield [| i; i+1; i+2; i+3; i+4 |] |]
          Exception = None
        } |> testWindowed
        {
          InputArray = [|1..10|]
          WindowSize = 10
          ExpectedArray =  [| yield [| 1 .. 10 |] |]
          Exception = None
        } |> testWindowed
        {
          InputArray = [|1..10|]
          WindowSize = 25
          ExpectedArray = [| |]
          Exception = None
        } |> testWindowed
        {
          InputArray = [|"str1";"str2";"str3";"str4"|]
          WindowSize = 2
          ExpectedArray =  [| [|"str1";"str2"|]; [|"str2";"str3"|]; [|"str3";"str4"|] |]
          Exception = None
        } |> testWindowed
        {
          InputArray = [| |]
          WindowSize = 2
          ExpectedArray = [| |]
          Exception = None
        } |> testWindowed
        {
          InputArray = null
          WindowSize = 2
          ExpectedArray = [| |]
          Exception = Some typeof<ArgumentNullException>
        } |> testWindowed
        {
          InputArray = [|1..10|]
          WindowSize = 0
          ExpectedArray =  [| |]
          Exception = Some typeof<ArgumentException>
        } |> testWindowed

        // expectedArrays indexed by arraySize,windowSize
        let expectedArrays = Array2D.zeroCreate 6 6
        expectedArrays.[1,1] <- [| [|1|] |]
        expectedArrays.[2,1] <- [| [|1|]; [|2|] |]
        expectedArrays.[2,2] <- [| [|1; 2|] |]
        expectedArrays.[3,1] <- [| [|1|]; [|2|]; [|3|] |]
        expectedArrays.[3,2] <- [| [|1; 2|]; [|2; 3|] |]
        expectedArrays.[3,3] <- [| [|1; 2; 3|] |]
        expectedArrays.[4,1] <- [| [|1|]; [|2|]; [|3|]; [|4|] |]
        expectedArrays.[4,2] <- [| [|1; 2|]; [|2; 3|]; [|3; 4|] |]
        expectedArrays.[4,3] <- [| [|1; 2; 3|]; [|2; 3; 4|] |]
        expectedArrays.[4,4] <- [| [|1; 2; 3; 4|] |]
        expectedArrays.[5,1] <- [| [|1|]; [|2|]; [|3|]; [|4|]; [|5|] |]
        expectedArrays.[5,2] <- [| [|1; 2|]; [|2; 3|]; [|3; 4|]; [|4; 5|] |]
        expectedArrays.[5,3] <- [| [|1; 2; 3|]; [|2; 3; 4|]; [|3; 4; 5|] |]
        expectedArrays.[5,4] <- [| [|1; 2; 3; 4|]; [|2; 3; 4; 5|] |]
        expectedArrays.[5,5] <- [| [|1; 2; 3; 4; 5|] |]

        for arraySize = 0 to 5 do
            for windowSize = -1 to 5 do
                if windowSize <= 0 then
                    CheckThrowsArgumentException (fun () -> Array.windowed windowSize [|1..arraySize|] |> ignore)
                elif arraySize < windowSize then
                    Assert.IsTrue([||] = Array.windowed windowSize [|1..arraySize|])
                else
                    Assert.IsTrue(expectedArrays.[arraySize, windowSize] = Array.windowed windowSize [|1..arraySize|])

        ()

    [<Test>]
    member this.Zero_Create() =
        
        // Check for bogus input
        CheckThrowsArgumentException(fun () -> Array.zeroCreate -1 |> ignore)
        
        // integer array  
        let resultInt =  Array.zeroCreate 8 
        if resultInt <> [|0;0;0;0;0;0;0;0|] then Assert.Fail()
        
        // string array    
        let resultStr = Array.zeroCreate 3 
        if resultStr <> [|null;null;null|] then Assert.Fail()
        
        // empty array     
        let resultEpt = Array.zeroCreate  0
        if resultEpt <> [||]  then Assert.Fail()
        
        ()

    [<Test>]
    member this.BadCreateArguments() =
        // negative number
        CheckThrowsArgumentException (fun () -> Array.create -1 0 |> ignore)

    [<Test>]
    member this.Zip() =
        // integer array  
        let resultInt =  Array.zip [|1..3|] [|2..2..6|] 
        if resultInt <> [|(1,2);(2,4);(3,6)|] then Assert.Fail()
        
        // string array    
        let resultStr = Array.zip [|"A"; "B";  "C" ; "D" |] [|"a";"b";"c";"d"|]
        if resultStr <> [|("A","a");("B","b");("C","c");("D","d")|] then Assert.Fail()
        
        // empty array     
        let resultEpt = Array.zip  [||] [||]
        if resultEpt <> [||]  then Assert.Fail()

        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.zip nullArr   nullArr  |> ignore)  
        
        // len1 <> len2
        CheckThrowsArgumentException(fun () -> Array.zip [|1..10|] [|2..20|] |> ignore)
        
        ()

    [<Test>]
    member this.Zip3() =
        // integer array  
        let resultInt =  Array.zip3 [|1..3|] [|2..2..6|] [|3;6;9|]
        if resultInt <> [|(1,2,3);(2,4,6);(3,6,9)|] then Assert.Fail()
        
        // string array    
        let resultStr = Array.zip3 [|"A"; "B";  "C" ; "D" |]  [|"1";"2";"3";"4"|]  [|"a"; "b"; "c"; "d"|]
        let str = resultStr.ToString()
        if resultStr <> [|("A","1","a");("B","2","b");("C","3","c");("D","4","d")|] then Assert.Fail()
        
        // empty array     
        let resultEpt = Array.zip3  [||] [||] [||]
        if resultEpt <> [||]  then Assert.Fail()

        // null array
        let nullArr = null:string[]      
        CheckThrowsArgumentNullException (fun () -> Array.zip3 nullArr  nullArr  nullArr  |> ignore)  
        
        // len1 <> len2
        CheckThrowsArgumentException(fun () -> Array.zip3 [|1..10|] [|2..20|] [|1..10|] |> ignore)
        // len1 <> len3
        CheckThrowsArgumentException(fun () -> Array.zip3 [|1..10|] [|1..10|] [|2..20|] |> ignore)
        
        ()

    [<Test>]
    member this.Item() =
        // integer array
        let resultInt = Array.item 3 [|1..8|]
        Assert.AreEqual(4, resultInt)

        // string array
        let resultStr = Array.item 2 [|"Arrays"; "are"; "commonly"; "array" |]
        Assert.AreEqual("commonly", resultStr)

        // empty array
        CheckThrowsIndexOutRangException(fun () -> Array.item 0 ([| |] : decimal[]) |> ignore)

        // null array
        let nullArr = null:string[]
        CheckThrowsNullRefException (fun () -> Array.item 0 nullArr |> ignore)

        // Negative index
        for i = -1 downto -10 do
           CheckThrowsIndexOutRangException (fun () -> Array.item i [|1..8|] |> ignore)

        // Out of range
        for i = 11 to 20 do
           CheckThrowsIndexOutRangException (fun () -> Array.item i [|1..8|] |> ignore)

    [<Test>]
    member this.tryItem() =
        // integer array
        let intArr = [| 3;4;7;8;10 |]
        let resultInt = Array.tryItem 3 intArr
        Assert.AreEqual(Some(8), resultInt)

        // string array
        let strArr = [| "Lists"; "are"; "commonly"; "list" |]
        let resultStr = Array.tryItem 1 strArr
        Assert.AreEqual(Some("are"), resultStr)

        // empty array
        let emptyArr:int[] = [| |]
        let resultEmpty = Array.tryItem 1 emptyArr
        Assert.AreEqual(None, resultEmpty)

        // null array
        let nullArr = null:string[]
        CheckThrowsArgumentNullException (fun () -> Array.tryItem 0 nullArr |> ignore)

        // Negative index
        let resultNegativeIndex = Array.tryItem -1 [| 3;1;6;2 |]
        Assert.AreEqual(None, resultNegativeIndex)

        // Index greater than length
        let resultIndexGreater = Array.tryItem 14 [| 3;1;6;2 |]
        Assert.AreEqual(None, resultIndexGreater)
