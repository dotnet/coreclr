// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// Various tests for the:
// Microsoft.FSharp.Collections.Map type

namespace FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Collections

open System
open System.Collections
open System.Collections.Generic
open FSharp.Core.UnitTests.LibraryTestFx
open NUnit.Framework

(*
[Test Strategy]
Make sure each method works on:
* Maps with reference keys
* Maps with value keys
* Empty Maps (0 elements)
* One-element maps
* Multi-element maps (2 - 7 elements)
*)

[<TestFixture>][<Category "Collections.Map">][<Category "FSharp.Core.Collections">]
type MapType() =
    
    // Interfaces
    [<Test>]
    member this.IEnumerable() =        
        // Legit IE
        let ie = (Map.ofArray [|(1,1);(2,4);(3,9)|]) :> IEnumerable
        let enum = ie.GetEnumerator()
        
        let testStepping() =
            CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(1,1))
            
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(2,4))
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(3,9))
            Assert.AreEqual(enum.MoveNext(), false)
            CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
    
        testStepping()
        enum.Reset()
        testStepping()
    
        // Empty IE
        let ie = [] |> Map.ofList :> IEnumerable  // Note no type args
        let enum = ie.GetEnumerator()
        
        CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
        Assert.AreEqual(enum.MoveNext(), false)
        CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)  
    
    [<Test>]
    member this.IEnumerable_T() =        
        // Legit IE
        let ie = (Map.ofArray [|(1,1);(2,4);(3,9)|]) :> IEnumerable<KeyValuePair<_,_>>
        let enum = ie.GetEnumerator()
        
        let testStepping() =
            CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(1,1))
            
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(2,4))
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(3,9))
            Assert.AreEqual(enum.MoveNext(), false)
            CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
    
        testStepping()
        enum.Reset()
        testStepping()
    
        // Empty IE
        let ie = [] |> Map.ofList :> IEnumerable  // Note no type args
        let enum = ie.GetEnumerator()
        
        CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
        Assert.AreEqual(enum.MoveNext(), false)
        CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)  
    
    
    [<Test>]
    member this.IDictionary() =        
        // Legit ID
        let id = (Map.ofArray [|(1,1);(2,4);(3,9)|]) :> IDictionary<_,_> 
        
        Assert.IsTrue(id.ContainsKey(1))   
        Assert.IsFalse(id.ContainsKey(5))  
        Assert.AreEqual(id.[1], 1)  
        Assert.AreEqual(id.[3], 9) 
        Assert.AreEqual(id.Keys,   [| 1; 2; 3|])         
        Assert.AreEqual(id.Values, [| 1; 4; 9|])
        
        CheckThrowsNotSupportedException(fun () -> id.[2] <-88)

        CheckThrowsNotSupportedException(fun () -> id.Add(new KeyValuePair<int,int>(4,16)))
        Assert.IsTrue(id.TryGetValue(1, ref 1))
        Assert.IsFalse(id.TryGetValue(100, ref 1))
        CheckThrowsNotSupportedException(fun () -> id.Remove(1) |> ignore)
        
        // Empty ID
        let id = Map.empty :> IDictionary<int, int>   // Note no type args  
        Assert.IsFalse(id.ContainsKey(5))
        CheckThrowsKeyNotFoundException(fun () -> id.[1] |> ignore)  
        Assert.AreEqual(id.Keys,   [| |] )
        Assert.AreEqual(id.Values, [| |] ) 

    [<Test>]
    member this.IReadOnlyDictionary() =
        let irod = (Map.ofArray [|(1,1);(2,4);(3,9)|]) :> IReadOnlyDictionary<_,_> 
        
        Assert.IsTrue(irod.ContainsKey(1))   
        Assert.IsFalse(irod.ContainsKey(5))  
        Assert.AreEqual(irod.[1], 1)  
        Assert.AreEqual(irod.[3], 9) 
        Assert.AreEqual(irod.Keys,   [| 1; 2; 3|])         
        Assert.AreEqual(irod.Values, [| 1; 4; 9|])
        
        Assert.IsTrue(irod.TryGetValue(1, ref 1))
        Assert.IsFalse(irod.TryGetValue(100, ref 1))
        
        // Empty IROD
        let irod = Map.empty :> IReadOnlyDictionary<int, int>   // Note no type args  
        Assert.IsFalse(irod.ContainsKey(5))
        CheckThrowsKeyNotFoundException(fun () -> irod.[1] |> ignore)  
        Assert.AreEqual(irod.Keys,   [| |] )
        Assert.AreEqual(irod.Values, [| |] ) 
    
    [<Test>]
    member this.ICollection() =        
        // Legit IC
        let ic = (Map.ofArray [|(1,1);(2,4);(3,9)|]) :> ICollection<KeyValuePair<_,_>>
        
        Assert.AreEqual(ic.Count, 3)
        Assert.IsTrue(ic.Contains(new KeyValuePair<int,int>(3,9))) 
        let newArr = Array.create 5 (new KeyValuePair<int,int>(3,9))
        ic.CopyTo(newArr,0) 
        Assert.IsTrue(ic.IsReadOnly)
        
        
        // raise ReadOnlyCollection exception
        CheckThrowsNotSupportedException(fun () -> ic.Add(new KeyValuePair<int,int>(3,9)) |> ignore)
        CheckThrowsNotSupportedException(fun () -> ic.Clear() |> ignore)
        CheckThrowsNotSupportedException(fun () -> ic.Remove(new KeyValuePair<int,int>(3,9)) |> ignore) 
        
            
        // Empty IC
        let ic = Map.empty :> ICollection<KeyValuePair<int, int>>   
        Assert.IsFalse(ic.Contains(new KeyValuePair<int,int>(3,9)))      
        let newArr = Array.create 5 (new KeyValuePair<int,int>(0,0))
        ic.CopyTo(newArr,0) 
    
    [<Test>]
    member this.IReadOnlyCollection() =
        // Legit IROC
        let iroc = (Map.ofArray [|(1,1);(2,4);(3,9)|]) :> IReadOnlyCollection<KeyValuePair<_,_>>
        
        Assert.AreEqual(iroc.Count, 3)
        
        // Empty IROC
        let iroc = Map.empty :> IReadOnlyCollection<KeyValuePair<int, int>>

        Assert.AreEqual(iroc.Count, 0)

    [<Test>]
    member this.IComparable() =        
        // Legit IC
        let ic = (Map.ofArray [|(1,1);(2,4);(3,9)|]) :> IComparable    
        Assert.AreEqual(ic.CompareTo([(1,1);(2,4);(3,9)]|> Map.ofList),0) 
        Assert.AreEqual(ic.CompareTo([(1,1);(3,9);(2,4)]|> Map.ofList),0) 
        Assert.AreEqual(ic.CompareTo([(1,1);(9,81);(2,4)]|> Map.ofList),-1) 
        Assert.AreEqual(ic.CompareTo([(1,1);(0,0);(2,4)]|> Map.ofList),1)      
        CheckThrowsArgumentException(fun() -> ic.CompareTo([(1,1);(2,4);(3,9)]) |> ignore)
                  
        // Empty IC
        let ic = [] |> Map.ofList :> IComparable   
        Assert.AreEqual(ic.CompareTo([]|> Map.ofList),0)
    
    
    // Base class methods
    [<Test>]
    member this.ObjectGetHashCode() =
        // Works on empty maps
        let e = Map.ofList (List.empty<int * decimal>)
        let m = Map.ofList [ (1, -1.0M) ]
        Assert.AreNotEqual(e.GetHashCode(), m.GetHashCode())
        
        // Should be order independent
        let x = Map.ofList [(1, -1.0M); (2, -2.0M)]
        let y = Map.ofList [(2, -2.0M); (1, -1.0M)]
        Assert.AreEqual(x.GetHashCode(), y.GetHashCode())
    
    [<Test>]
    member this.ObjectToString() =
        Assert.AreEqual("map [(1, 1); (2, 4); (3, 9)]", (Map.ofArray [|(1,1);(2,4);(3,9)|]).ToString())
        Assert.AreEqual("map []", ([] |> Map.ofList).ToString())
        Assert.AreEqual("map []", 
                        (([] :(decimal*decimal)list) |> Map.ofList).ToString())
    
    [<Test>]
    member this.ObjectEquals() =
        // All three are different references, but equality has been
        // provided by the F# compiler.
        let a = [(1,1);(2,4);(3,9)] |> Map.ofList
        let b = (1,1) :: [(2,4);(3,9)] |> Map.ofList
        Assert.IsTrue( (a = b) )

        Assert.IsTrue( a.Equals(b) ); Assert.IsTrue( b.Equals(a) )

        // Equality between types
        let a = ([] : (int*int) list)  |> Map.ofList
        let b = ([] : (string*string) list ) |> Map.ofList
        Assert.IsFalse( b.Equals(a) )
        Assert.IsFalse( a.Equals(b) )
        
        // Co/contra variance not supported
        let a = ([] : (string*string) list) |> Map.ofList
        let b = ([] : (System.IComparable*System.IComparable) list)    |> Map.ofList
        Assert.IsFalse(a.Equals(b))
        Assert.IsFalse(b.Equals(a))
        
        // Self equality
        let a = [(1,1)] |> Map.ofList
        Assert.IsTrue( (a = a) )
        Assert.IsTrue(a.Equals(a))
        
        // Null
        Assert.IsFalse(a.Equals(null))

    // Instance methods
    [<Test>]
    member this.New() =    
        let newMap = new Map<int,int>([|(1,1);(2,4);(3,9)|])
        let b = newMap.Add(4,16)
        Assert.AreEqual(b.[4], 16)
        Assert.AreEqual(b.[2], 4)
    
        let e  = new  Map<int,string>([])
        let ae = e.Add(1,"Monday")
        Assert.AreEqual(ae.[1], "Monday")
        
    member this.Add() =
    
        let a = (Map.ofArray [|(1,1);(2,4);(3,9)|])
        let b = a.Add(4,16)
        Assert.AreEqual(b.[4], 16)
        Assert.AreEqual(b.[2], 4)
    
        let e  = Map.empty<int,string>
        let ae = e.Add(1,"Monday")
        Assert.AreEqual(ae.[1], "Monday")
    
    [<Test>]
    member this.ContainsKey() =
    
        let a = (Map.ofArray [|(1,1);(2,4);(3,9)|])        
        Assert.IsTrue(a.ContainsKey(3))
    
        let e  = Map.empty<int,string>
        Assert.IsFalse(e.ContainsKey(3)) 
    
    
    [<Test>]
    member this.Count() =
    
        let a = (Map.ofArray [|(1,1);(2,4);(3,9)|])
        Assert.AreEqual(a.Count, 3)
    
        let e  = Map.empty<int,string>
        Assert.AreEqual(e.Count, 0) 
    
    [<Test>]
    member this.IsEmpty() =
    
        let l = (Map.ofArray [|(1,1);(2,4);(3,9)|])
        Assert.IsFalse(l.IsEmpty)
    
        let e = Map.empty<int,int>
        Assert.IsTrue(e.IsEmpty)        
    
    [<Test>]
    member this.Item() =

        let mutable l = [(1,1)] |> Map.ofList
        Assert.AreEqual(l.[1], 1)
        l <- l.Add(100,8)
        Assert.AreEqual(l.[100], 8)
        
        for testidx = 0 to 20 do
            let l = Map.ofSeq (seq { for i in 0..testidx do yield (i,i*i)})
            for i = 0 to l.Count - 1 do
                Assert.AreEqual(i*i, l.[i])
                Assert.AreEqual(i*i, l.Item(i))
        
        // Invalid index
        let l = (Map.ofArray [|(1,1);(2,4);(3,9)|])
        CheckThrowsKeyNotFoundException(fun () -> l.[ -1 ] |> ignore)
        CheckThrowsKeyNotFoundException(fun () -> l.[1000] |> ignore)
    
    [<Test>]
    member this.Remove() =
    
        let l = (Map.ofArray [|(1,1);(2,4);(3,9)|])
        let rem = l.Remove(2)
        Assert.AreEqual(rem.Count, 2) 
        CheckThrowsKeyNotFoundException(fun () -> rem.[ 2 ] |> ignore)
    
        let e  = Map.empty<int,string>
        let ae = e.Remove(2)
        Assert.AreEqual(ae.Count, 0)
        
    [<Test>]
    member this.TryFind() =
    
        let l = (Map.ofArray [|(1,1);(2,4);(3,9)|])
        let rem = l.TryFind(2)
        Assert.AreEqual(l.TryFind(2),Some 4)         
    
        let e  = Map.empty<int,string>
    
        Assert.AreEqual(e.TryFind(2), None)    
            
    
    
