// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// Various tests for the:
// Microsoft.FSharp.Collections.Set type

namespace FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Collections

open System
open System.Collections
open System.Collections.Generic
open FSharp.Core.UnitTests.LibraryTestFx
open NUnit.Framework

(*
[Test Strategy]
Make sure each method works on:
* Empty set
* Single-element set
* Sets with 4 more more elements
*)

[<TestFixture>][<Category "Collections.Set">][<Category "FSharp.Core.Collections">]
type SetType() =

    // Interfaces
    [<Test>]
    member this.IEnumerable() =        
        // Legit IE
        let ie = (new Set<char>(['a'; 'b'; 'c'])) :> IEnumerable
        //let alphabet = new Set<char>([| 'a' .. 'z' |])
        let enum = ie.GetEnumerator()
        
        let testStepping() =
            CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, 'a')
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, 'b')
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, 'c')
            Assert.AreEqual(enum.MoveNext(), false)
            CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
    
        testStepping()
        enum.Reset()
        testStepping()
    
        // Empty IE
        let ie = (new Set<char>([])) :> IEnumerable  // Note no type args
        let enum = ie.GetEnumerator()
        
        CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
        Assert.AreEqual(enum.MoveNext(), false)
        CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)

    [<Test>]
    member this.IEnumerable_T() =        
        // Legit IE
        let ie =(new Set<char>(['a'; 'b'; 'c'])) :> IEnumerable<char>
        let enum = ie.GetEnumerator()
        
        let testStepping() =
            CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, 'a')
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, 'b')
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, 'c')
            Assert.AreEqual(enum.MoveNext(), false)
            CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
        
        testStepping()
        enum.Reset()
        testStepping()
    
        // Empty IE
        let ie = (new Set<int>([])) :> IEnumerable<int>  
        let enum = ie.GetEnumerator()
        
        CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
        Assert.AreEqual(enum.MoveNext(), false)
        CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
        
        
    [<Test>]
    member this.ICollection() =        
        // Legit IC        
        let ic = (new Set<int>([1;2;3;4])) :> ICollection<int>
        let st = new Set<int>([1;2;3;4])        
        
        Assert.IsTrue(ic.Contains(3)) 
        let newArr = Array.create 5 0
        ic.CopyTo(newArr,0) 
        Assert.IsTrue(ic.IsReadOnly)       
            
        // Empty IC
        let ic = (new Set<string>([])) :> ICollection<string>
        Assert.IsFalse(ic.Contains("A") )     
        let newArr = Array.create 5 "a"
        ic.CopyTo(newArr,0) 

    [<Test>]
    member this.IReadOnlyCollection() =        
        // Legit IROC
        let iroc = (new Set<int>([1;2;3;4])) :> IReadOnlyCollection<int>
        Assert.AreEqual(iroc.Count, 4)       
            
        // Empty IROC
        let iroc = (new Set<string>([])) :> IReadOnlyCollection<string>
        Assert.AreEqual(iroc.Count, 0)
    
    [<Test>]
    member this.IComparable() =        
        // Legit IC
        let ic = (new Set<int>([1;2;3;4])) :> IComparable    
        Assert.AreEqual(ic.CompareTo(new Set<int>([1;2;3;4])),0) 
        
        // Empty IC
        let ic = (new Set<string>([])) :> IComparable   
        Assert.AreEqual(ic.CompareTo(Set.empty<string>),0)
        
        
    // Base class methods
    [<Test>]
    member this.ObjectGetHashCode() =
        // Verify order added is independent
        let x = Set.ofList [1; 2; 3]
        let y = Set.ofList [3; 2; 1]
        Assert.AreEqual(x.GetHashCode(), y.GetHashCode())
    
    [<Test>]
    member this.ObjectToString() =
        Assert.AreEqual("set [1; 2; 3; ... ]", (new Set<int>([1;2;3;4])).ToString())
        Assert.AreEqual("set []", (Set.empty).ToString())
        Assert.AreEqual("set [1; 3]", (new Set<decimal>([1M;3M])).ToString())
        
    
    [<Test>]
    member this.ObjectEquals() =
        // All three are different references, but equality has been
        // provided by the F# compiler.
        let a = new Set<int>([1;2;3])
        let b = new Set<int>([1..3])
        let c = new Set<int>(seq{1..3})
        Assert.IsTrue( (a = b) )
        Assert.IsTrue( (b = c) )
        Assert.IsTrue( (c = a) )
        Assert.IsTrue( a.Equals(b) ); Assert.IsTrue( b.Equals(a) )
        Assert.IsTrue( b.Equals(c) ); Assert.IsTrue( c.Equals(b) )
        Assert.IsTrue( c.Equals(a) ); Assert.IsTrue( a.Equals(c) )

        // Equality between types
        let a = Set.empty<int>
        let b = Set.empty<string>
        Assert.IsFalse( b.Equals(a) )
        Assert.IsFalse( a.Equals(b) )
        
        // Co/contra variance not supported
        let a = Set.empty<string>
        let b = Set.empty
        Assert.IsFalse(a.Equals(b))
        Assert.IsFalse(b.Equals(a))
        
        // Self equality
        let a = new Set<int>([1])
        Assert.IsTrue( (a = a) )
        Assert.IsTrue(a.Equals(a))
        
        // Null
        Assert.IsFalse(a.Equals(null))  
        
        
    // Instance methods
    [<Test>]
    member this.Add() =    
        let l = new Set<int>([1 .. 10])
        let ad = l.Add 88
        Assert.IsTrue(ad.Contains(88))
    
        let e : Set<string> = Set.empty<string>
        let ade = e.Add "A"
        Assert.IsTrue(ade.Contains("A"))
        
        let s = Set.singleton 168
        let ads = s.Add 100
        Assert.IsTrue(ads.Contains(100))
        
    [<Test>]
    member this.Contains() =    
        let i = new Set<int>([1 .. 10])
        Assert.IsTrue(i.Contains(8))
    
        let e : Set<string> = Set.empty<string>
        Assert.IsFalse(e.Contains("A"))
        
        let s = Set.singleton 168
        Assert.IsTrue(s.Contains(168))
    
    [<Test>]
    member this.Count() =    
        let l = new Set<int>([1 .. 10])
        Assert.AreEqual(l.Count, 10)
    
        let e : Set<string> = Set.empty<string>
        Assert.AreEqual(e.Count, 0)
        
        let s = Set.singleton 'a'
        Assert.AreEqual(s.Count, 1)        
        
    [<Test>]
    member this.IsEmpty() =
        let i = new Set<int>([1 .. 10])
        Assert.IsFalse(i.IsEmpty)
    
        let e : Set<string> = Set.empty<string>
        Assert.IsTrue(e.IsEmpty)
        
        let s = Set.singleton 168
        Assert.IsFalse(s.IsEmpty)   
        
    [<Test>]
    member this.IsSubsetOf() =
        let fir = new Set<int>([1 .. 20])
        let sec = new Set<int>([1 .. 10])
        Assert.IsTrue(sec.IsSubsetOf(fir))
        Assert.IsTrue(Set.isSubset sec fir)
    
        let e : Set<int> = Set.empty<int>
        Assert.IsTrue(e.IsSubsetOf(fir))
        Assert.IsTrue(Set.isSubset e fir)
        
        let s = Set.singleton 8
        Assert.IsTrue(s.IsSubsetOf(fir)) 
        Assert.IsTrue(Set.isSubset s fir)
        
        let s100 = set [0..100]
        let s101 = set [0..101]
        for i = 0 to 100 do 
            Assert.IsFalse( (set [-1..i]).IsSubsetOf s100)
            Assert.IsTrue( (set [0..i]).IsSubsetOf s100)
            Assert.IsTrue( (set [0..i]).IsProperSubsetOf s101)
           
        
    [<Test>]
    member this.IsSupersetOf() =
        let fir = new Set<int>([1 .. 10])
        let sec = new Set<int>([1 .. 20])
        Assert.IsTrue(sec.IsSupersetOf(fir))
        Assert.IsTrue(Set.isSuperset sec fir)
    
        let e : Set<int> = Set.empty<int>
        Assert.IsFalse(e.IsSupersetOf(fir))
        Assert.IsFalse(Set.isSuperset e fir)
        
        let s = Set.singleton 168
        Assert.IsFalse(s.IsSupersetOf(fir))  
        Assert.IsFalse(Set.isSuperset s fir)

        let s100 = set [0..100]
        let s101 = set [0..101]
        for i = 0 to 100 do 
            Assert.IsFalse( s100.IsSupersetOf (set [-1..i]))
            Assert.IsTrue( s100.IsSupersetOf (set [0..i]))
            Assert.IsTrue( s101.IsSupersetOf (set [0..i]))
        
    [<Test>]
    member this.Remove() =    
        let i = new Set<int>([1;2;3;4])
        Assert.AreEqual(i.Remove 3,(new Set<int>([1;2;4])))
    
        let e : Set<string> = Set.empty<string>
        Assert.AreEqual(e.Remove "A", e)
        
        let s = Set.singleton 168
        Assert.AreEqual(s.Remove 168, Set.empty<int>) 
        
        
    // Static methods
    [<Test>]
    member this.Addition() =
        let fir = new Set<int>([1;3;5])
        let sec = new Set<int>([2;4;6])
        Assert.AreEqual(fir + sec, new Set<int>([1;2;3;4;5;6]))
        Assert.AreEqual(Set.op_Addition(fir,sec), new Set<int>([1;2;3;4;5;6]))
    
        let e : Set<int> = Set.empty<int>
        Assert.AreEqual(e + e, e)
        Assert.AreEqual(Set.op_Addition(e,e),e)
        
        let s1 = Set.singleton 8
        let s2 = Set.singleton 6
        Assert.AreEqual(s1 + s2, new Set<int>([8;6]))
        Assert.AreEqual(Set.op_Addition(s1,s2), new Set<int>([8;6]))
        

    [<Test>]
    member this.Subtraction() =
        let fir = new Set<int>([1..6])
        let sec = new Set<int>([2;4;6])
        Assert.AreEqual(fir - sec, new Set<int>([1;3;5]))
        Assert.AreEqual(Set.difference fir sec, new Set<int>([1;3;5]))
        Assert.AreEqual(Set.op_Subtraction(fir,sec), new Set<int>([1;3;5]))
    
        let e : Set<int> = Set.empty<int>
        Assert.AreEqual(e - e, e)
        Assert.AreEqual(Set.difference e e, e)
        Assert.AreEqual(Set.op_Subtraction(e,e),e)
        
        let s1 = Set.singleton 8
        let s2 = Set.singleton 6
        Assert.AreEqual(s1 - s2, new Set<int>([8]))
        Assert.AreEqual(Set.difference s1 s2, new Set<int>([8]))
        Assert.AreEqual(Set.op_Subtraction(s1,s2), new Set<int>([8]))
        

    [<Test>]
    member this.MinimumElement() =
        let fir = new Set<int>([1..6])
        let sec = new Set<int>([2;4;6])
        Assert.AreEqual(fir.MinimumElement, 1)
        Assert.AreEqual(sec.MinimumElement, 2)
        Assert.AreEqual(Set.minElement fir, 1)
        Assert.AreEqual(Set.minElement sec, 2)
        

    [<Test>]
    member this.MaximumElement() =
        let fir = new Set<int>([1..6])
        let sec = new Set<int>([2;4;7])
        Assert.AreEqual(fir.MaximumElement, 6)
        Assert.AreEqual(sec.MaximumElement, 7)
        Assert.AreEqual(Set.maxElement fir, 6)
        Assert.AreEqual(Set.maxElement sec, 7)
