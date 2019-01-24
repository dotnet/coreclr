// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// Various tests for the:
// Microsoft.FSharp.Collections.Map module

namespace FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Collections

open System
open FSharp.Core.UnitTests.LibraryTestFx
open NUnit.Framework

(*
[Test Strategy]
Make sure each method works on:
* Maps with reference keys
* Maps with value keys
* Empty Maps (0 elements)
* One-element maps
* Multi-element maps (2 – 7 elements)
*)


[<TestFixture>][<Category "Collections.Map">][<Category "FSharp.Core.Collections">]
type MapModule() =
    [<Test>]
    member this.Empty() =
        let emptyMap = Map.empty        
        Assert.IsTrue(Map.isEmpty emptyMap)
        
        let a:Map<int,int>    = Map.empty<int,int>
        let b : Map<string,string> = Map.empty<string,string>
        let c : Map<int,string> = Map.empty<int,string>  
              
        ()
        
    [<Test>]
    member this.Add() =
        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap = Map.add 1 "a" valueKeyMap        
        Assert.AreEqual(resultValueMap.[1], "a")
        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let resultRefMap = Map.add  ""  0 refMap
        Assert.AreEqual(resultRefMap.[""] , 0)
        
        // empty Map
        let eptMap = Map.empty
        let resultEpt = Map.add 1 "a" eptMap
        Assert.AreEqual(resultEpt.[1], "a")
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele = Map.add  7  "seven" oeleMap
        Assert.AreEqual(resultOele.[7], "seven")     
        
        // extra test for add -- add a key which already exists in the Map
        let extMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultExt = Map.add 2 "dup" extMap        
        Assert.AreEqual(resultExt.[2], "dup")   
        
         
        ()

    [<Test>]
    member this.Exists() =
        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap = Map.exists (fun x y -> x > 3) valueKeyMap        
        Assert.IsTrue(resultValueMap)

        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let resultRefMap = refMap |> Map.exists  (fun x y -> y>2 )
        Assert.IsTrue(resultRefMap)
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele = oeleMap |> Map.exists  (fun x y -> (x + y.Length) % 4 = 0 ) 
        Assert.IsTrue(resultOele)
        
        // empty Map
        let eptMap = Map.empty
        let resultEpt = Map.exists (fun x y -> false) eptMap
        Assert.IsFalse(resultEpt)
       
        ()
        
    [<Test>]
    member this.Filter() =
        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap =valueKeyMap |> Map.filter (fun x y -> x % 3 = 0)         
        Assert.AreEqual(resultValueMap,[3,"c"] |> Map.ofList)
        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let resultRefMap = refMap |> Map.filter  (fun x y -> y  > 3 ) 
        Assert.AreEqual(resultRefMap,["....",4] |> Map.ofList)
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele = oeleMap |> Map.filter  (fun x y -> x<3 )         
        Assert.AreEqual(resultOele,oeleMap)
        
        // empty Map
        let eptMap = Map.empty
        let resultEpt = Map.filter (fun x y -> true) eptMap        
        Assert.AreEqual(resultEpt,eptMap)
               
        ()       


    [<Test>]
    member this.Find() =
        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap = Map.find 5 valueKeyMap        
        Assert.AreEqual(resultValueMap,"e")
        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let resultRefMap = Map.find  ".." refMap        
        Assert.AreEqual(resultRefMap,2)
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele = Map.find 1 oeleMap        
        Assert.AreEqual(resultOele,"one")
        
        // empty Map
        let eptMap = Map.empty
        CheckThrowsKeyNotFoundException (fun () -> Map.find 1 eptMap |> ignore)
               
        ()  

    [<Test>]
    member this.FindIndex() =
        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap =valueKeyMap |> Map.findKey (fun x y -> x % 3 = 0)         
        Assert.AreEqual(resultValueMap,3)
        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let resultRefMap = refMap |> Map.findKey  (fun x y -> y % 3 = 0 )         
        Assert.AreEqual(resultRefMap,"...")
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele = oeleMap |> Map.findKey  (fun x y -> x = 1 )         
        Assert.AreEqual(resultOele,1)
        
        // empty Map
        let eptMap = Map.empty
        CheckThrowsKeyNotFoundException (fun () -> Map.findKey (fun x y -> true) eptMap |> ignore)
               
        ()          
     
    [<Test>]
    member this.TryPick() =
        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap = valueKeyMap |> Map.tryPick (fun x y -> if x % 3 = 0 then Some (x) else None)         
        Assert.AreEqual(resultValueMap,Some 3)
        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let resultRefMap = refMap |> Map.tryPick  (fun x y -> if (y % 3 = 0 ) then Some y else None ) 
        
        Assert.AreEqual(resultRefMap,Some 3)
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele = oeleMap |> Map.tryPick  (fun x y -> if(x + y.Length) % 4 = 0 then Some y else None )         
        Assert.AreEqual(resultOele,Some "one")
        
        // empty Map
        let eptMap = Map.empty
        let resultEpt = Map.tryPick (fun x y -> Some x) eptMap        
        Assert.AreEqual(resultEpt,None)
               
        ()     

    [<Test>]
    member this.Pick() =
        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValue = valueKeyMap |> Map.pick (fun x y -> if x % 3 = 0 then Some (y) else None)         
        Assert.AreEqual(resultValue, "c")
        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let result = refMap |> Map.pick (fun x y -> if (y % 3 = 0 ) then Some y else None ) 
        Assert.AreEqual(result, 3)
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele = oeleMap |> Map.pick (fun x y -> if(x + y.Length) % 4 = 0 then Some y else None )         
        Assert.AreEqual(resultOele, "one")
        
        // empty Map
        let eptMap = Map.empty
        let resultEpt = 
            try 
                Map.pick (fun x y -> Some x) eptMap
            with :? System.Collections.Generic.KeyNotFoundException -> Some 0
        Assert.AreEqual(resultEpt, Some 0)
        
        ()

    [<Test>]
    member this.Fold() =
        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap = valueKeyMap |> Map.fold (fun x y z -> x + y + z.Length) 10         
        Assert.AreEqual(resultValueMap,28)
        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let resultRefMap =   refMap |> Map.fold  (fun x y z -> x + y + z.ToString())  "*"      
        Assert.AreEqual(resultRefMap,"*.1..2...3....4")
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele =   oeleMap |> Map.fold  (fun x y z -> x + y.ToString() + z)  "got"      
        Assert.AreEqual(resultOele,"got1one")
        
        // empty Map
        let eptMap = Map.empty
        let resultEpt = eptMap |> Map.fold (fun x y z -> 1) 1         
        Assert.AreEqual(resultEpt,1)
               
        ()

    [<Test>]
    member this.FoldBack() =
        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap = Map.foldBack (fun x y z -> x.ToString() + y + z.ToString()) valueKeyMap "*"     
        Assert.AreEqual(resultValueMap,"2b3c4d5e*")
        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let resultRefMap = Map.foldBack  (fun x y z -> x + y.ToString() + z) refMap "right"         
        Assert.AreEqual(resultRefMap,".1..2...3....4right")
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele = Map.foldBack  (fun x y z -> x.ToString() + y + z) oeleMap "right"         
        Assert.AreEqual(resultOele,"1oneright")
        
        // empty Map
        let eptMap = Map.empty
        let resultEpt = Map.foldBack (fun x y z -> 1) eptMap 1         
        Assert.AreEqual(resultEpt,1)
               
        ()
        
    [<Test>]
    member this.ForAll() =
        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap = valueKeyMap |> Map.forall (fun x y -> x % 3 = 0)         
        Assert.IsFalse(resultValueMap)
        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let resultRefMap = refMap |> Map.forall  (fun x y -> x.Length  > 4 )         
        Assert.IsFalse(resultRefMap)
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele = oeleMap |> Map.forall  (fun x y -> x<3 )         
        Assert.IsTrue(resultOele)
        
        // empty Map
        let eptMap = Map.empty
        let resultEpt =eptMap |>  Map.forall (fun x y -> true)         
        Assert.IsTrue(resultEpt)
               
        ()       


    [<Test>]
    member this.IsEmpty() =
        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap = Map.isEmpty  valueKeyMap        
        Assert.IsFalse(resultValueMap)
        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let resultRefMap = Map.isEmpty   refMap        
        Assert.IsFalse(resultRefMap)
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele = Map.isEmpty   oeleMap        
        Assert.IsFalse(resultOele)
        
        // empty Map
        let eptMap = Map.empty
        let resultEpt = Map.isEmpty  eptMap        
        Assert.IsTrue(resultEpt)
               
        ()  

    [<Test>]
    member this.Iter() =
        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap = ref 0    
        let funInt (x:int) (y:string) =   
            resultValueMap := !resultValueMap + x + y.Length             
            () 
        Map.iter funInt valueKeyMap        
        Assert.AreEqual(!resultValueMap,18)
        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let resultRefMap = ref ""
        let funStr (x:string) (y:int) = 
            resultRefMap := !resultRefMap + x + y.ToString()
            ()
        Map.iter funStr refMap        
        Assert.AreEqual(!resultRefMap,".1..2...3....4")
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele = ref ""
        let funMix  (x:int) (y:string) =
            resultOele := !resultOele + x.ToString() + y
            ()
        Map.iter funMix oeleMap        
        Assert.AreEqual(!resultOele,"1one")
        
        // empty Map
        let eptMap = Map.empty
        let resultEpt = ref 0    
        let funEpt (x:int) (y:int) =   
            resultEpt := !resultEpt + x + y              
            () 
        Map.iter funEpt eptMap        
        Assert.AreEqual(!resultEpt,0)
               
        ()          
     
    [<Test>]
    member this.Map() =

        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap = valueKeyMap |> Map.map (fun x y -> x.ToString() + y )         
        Assert.AreEqual(resultValueMap,[(2,"2b"); (3,"3c"); (4,"4d"); (5,"5e")] |> Map.ofList)
        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let resultRefMap = refMap |> Map.map (fun x y -> x.Length + y ) 
        Assert.AreEqual(resultRefMap,[(".",2); ("..",4);( "...",6); ("....",8)] |> Map.ofList)
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele = oeleMap |> Map.map  (fun x y -> x.ToString() + y )         
        Assert.AreEqual(resultOele,[1,"1one"] |> Map.ofList)
        
        // empty Map
        let eptMap = Map.empty<int,int>
        let resultEpt = eptMap |> Map.map (fun x y -> x+y)         
        Assert.AreEqual(resultEpt,eptMap)
               
        ()     

    [<Test>]
    member this.Contains() =
        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap = Map.containsKey 2 valueKeyMap        
        Assert.IsTrue(resultValueMap)
        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let resultRefMap = Map.containsKey ".."  refMap        
        Assert.IsTrue(resultRefMap)
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele = Map.containsKey 1 oeleMap        
        Assert.IsTrue(resultOele)
        
        // empty Map
        let eptMap = Map.empty
        let resultEpt = Map.containsKey 3 eptMap        
        Assert.IsFalse(resultEpt)
               
        () 
        
    [<Test>]
    member this.Of_Array_Of_List_Of_Seq() =
        // value keys    
        let valueKeyMapOfArr = Map.ofArray [|(2,"b"); (3,"c"); (4,"d"); (5,"e")|]     
        let valueKeyMapOfList = Map.ofList [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let valueKeyMapOfSeq = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]

        Assert.AreEqual(valueKeyMapOfArr,valueKeyMapOfList)
        Assert.AreEqual(valueKeyMapOfList,valueKeyMapOfSeq)
        Assert.AreEqual(valueKeyMapOfArr,valueKeyMapOfSeq)
        
        // reference keys
        let refMapOfArr = Map.ofArray [|(".",1); ("..",2);( "...",3); ("....",4)|]
        let refMapOfList = Map.ofList [(".",1); ("..",2);( "...",3); ("....",4)]
        let refMapOfSeq = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]

        Assert.AreEqual(refMapOfArr,refMapOfList)
        Assert.AreEqual(refMapOfList,refMapOfSeq)
        Assert.AreEqual(refMapOfArr,refMapOfSeq)
        
        // One-element Map
        let oeleMapOfArr = Map.ofArray [|(1,"one")|]
        let oeleMapOfList = Map.ofList [(1,"one") ]
        let oeleMapOfSeq = Map.ofSeq [(1,"one") ]

        Assert.AreEqual(oeleMapOfArr,oeleMapOfList)
        Assert.AreEqual(oeleMapOfList,oeleMapOfSeq)
        Assert.AreEqual(oeleMapOfArr,oeleMapOfSeq)
                
        ()

    [<Test>]
    member this.Partition() =
        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap = Map.partition (fun x y  -> x%2 = 0) valueKeyMap         
        let choosed = [(2,"b"); (4,"d")] |> Map.ofList
        let notChoosed = [(3,"c"); (5,"e")] |> Map.ofList
        Assert.AreEqual(resultValueMap,(choosed,notChoosed))
        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let resultRefMap = refMap |>  Map.partition  (fun x y  -> x.Length >2 )        
        let choosed = [( "...",3); ("....",4)] |> Map.ofList
        let notChoosed = [(".",1); ("..",2)] |> Map.ofList
        Assert.AreEqual(resultRefMap,(choosed,notChoosed))
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele = Map.partition  (fun x y  -> x<4) oeleMap     
        let choosed = [(1,"one")] |> Map.ofList
        let notChoosed = Map.empty<int,string>
        Assert.AreEqual(resultOele,(choosed,notChoosed))
        
        // empty Map
        let eptMap = Map.empty
        let resultEpt = Map.partition (fun x y  -> true) eptMap          
        Assert.AreEqual(resultEpt,(eptMap,eptMap))
               
        ()
    
        
    [<Test>]
    member this.Remove() =
        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap = Map.remove 5 valueKeyMap        
        Assert.AreEqual(resultValueMap,[(2,"b"); (3,"c"); (4,"d")] |> Map.ofList)
        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let resultRefMap = Map.remove  ".." refMap         
        Assert.AreEqual(resultRefMap,[(".",1); ( "...",3); ("....",4)] |> Map.ofList)
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele = Map.remove  1 oeleMap             
        Assert.AreEqual(resultOele,Map.empty<int,string>)
        
        // Two-element Map
        let oeleMap = Map.ofSeq [(1, "one");(2,"Two")]
        let resultOele = Map.remove  1 oeleMap             
        let exOele = Map.ofSeq [(2, "Two")]
        Assert.AreEqual(resultOele, exOele)
        
        // Item which want to be removed not included in the map
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap = Map.remove 5 valueKeyMap        
        Assert.AreEqual(resultValueMap,[(2,"b"); (3,"c"); (4,"d")] |> Map.ofList)
        
                               
        ()
        
    [<Test>]
    member this.To_Array() =
        // value keys    
        let valueKeyMapOfArr = Map.ofArray [|(1,1);(2,4);(3,9)|]     
        let resultValueMap = Map.toArray valueKeyMapOfArr        
        Assert.AreEqual(resultValueMap,[|(1,1);(2,4);(3,9)|])
        
        // reference keys
        let refMapOfArr = Map.ofArray [|(".",1); ("..",2);( "...",3); ("....",4)|]
        let resultRefMap = Map.toArray refMapOfArr        
        Assert.AreEqual(resultRefMap,[|(".",1); ("..",2);( "...",3); ("....",4)|])
        
        // One-element Map
        let oeleMapOfArr = Map.ofArray [|(1,"one")|]
        let resultOele = Map.toArray oeleMapOfArr        
        Assert.AreEqual(resultOele,[|(1,"one")|])
        
        // empty Map
        let eptMap = Map.ofArray [||]
        let resultEpt = Map.toArray eptMap            
        Assert.AreEqual(resultEpt,[||])

        () 

    [<Test>]
    member this.To_List() =
        // value keys    
        let valueKeyMapOfArr = Map.ofList [(1,1);(2,4);(3,9)]     
        let resultValueMap = Map.toList valueKeyMapOfArr        
        Assert.AreEqual(resultValueMap,[(1,1);(2,4);(3,9)])
        
        // reference keys
        let refMapOfArr = Map.ofList [(".",1); ("..",2);( "...",3); ("....",4)]
        let resultRefMap = Map.toList refMapOfArr        
        Assert.AreEqual(resultRefMap,[(".",1); ("..",2);( "...",3); ("....",4)])
        
        // One-element Map
        let oeleMapOfArr = Map.ofList [(1,"one")]
        let resultOele = Map.toList oeleMapOfArr        
        Assert.AreEqual(resultOele,[(1,"one")])
        
        // empty Map
        let eptMap = Map.empty<int,string>
        let resultEpt = Map.toList eptMap  
        let eptList :(int*string) list = []          
        Assert.AreEqual(resultEpt,eptList)

        ()     

    [<Test>]
    member this.To_Seq() =
        // value keys    
        let valueKeyMapOfArr = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]  
        let resultValueMap = Map.toSeq valueKeyMapOfArr
        let originInt = seq { for i in 1..3 do yield (i,i*i)}
        VerifySeqsEqual resultValueMap [(2,"b"); (3,"c"); (4,"d"); (5,"e")]

        
        // reference keys
        let refMapOfArr = Map.ofSeq [(".",1); ("..",2);( "...",3); ("....",4)]
        let resultRefMap = Map.toSeq refMapOfArr
        let originStr = seq { for x in [  "is" ;"lists";"str"; "this"] do yield (x,x.ToUpper())}
        VerifySeqsEqual resultRefMap [(".",1); ("..",2);( "...",3); ("....",4)]

        
        // One-element Map
        let oeleMapOfArr = Map.ofSeq [(1,"one")]
        let resultOele = Map.toSeq oeleMapOfArr
        let originMix = seq { for x in [ "is" ;"str"; "this" ;"lists"] do yield (x.Length,x.ToUpper())}
        VerifySeqsEqual resultOele [(1,"one")]

         
        ()           

    [<Test>]
    member this.TryFind() =
        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap = Map.tryFind 5 valueKeyMap        
        Assert.AreEqual(resultValueMap,Some "e")
        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let resultRefMap = Map.tryFind  "..." refMap        
        Assert.AreEqual(resultRefMap,Some 3)
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele = Map.tryFind 1 oeleMap        
        Assert.AreEqual(resultOele,Some "one")
        
        // empty Map
        let eptMap = Map.empty
        let resultEpt = Map.tryFind 1 eptMap        
        Assert.AreEqual(resultEpt,None)
               
        ()      

    [<Test>]
    member this.TryFindIndex() =
        // value keys
        let valueKeyMap = Map.ofSeq [(2,"b"); (3,"c"); (4,"d"); (5,"e")]
        let resultValueMap = valueKeyMap |> Map.tryFindKey (fun x y -> x+y.Length >30)         
        Assert.AreEqual(resultValueMap,None)
        
        // reference keys
        let refMap = Map.ofSeq [for c in ["."; ".."; "..."; "...."] do yield (c, c.Length) ]
        let resultRefMap = refMap |> Map.tryFindKey  (fun x y -> (x.Length+y)>6)         
        Assert.AreEqual(resultRefMap,Some "....")
        
        // One-element Map
        let oeleMap = Map.ofSeq [(1, "one")]
        let resultOele = oeleMap |> Map.tryFindKey (fun x y -> y.Contains("o"))        
        Assert.AreEqual(resultOele,Some 1)
        
        // empty Map
        let eptMap = Map.empty
        let resultEpt = Map.tryFindKey (fun x y -> x+y >30) eptMap        
        Assert.AreEqual(resultEpt,None)
               
        ()  
