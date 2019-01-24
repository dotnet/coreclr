// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// Various tests for the:
// Microsoft.FSharp.Control.Observable module

namespace FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Control

open System
open FSharp.Core.UnitTests.LibraryTestFx
open NUnit.Framework

(*
[Test Strategy]
Create some custom types that contain diagnostics for when and
how their events are fired.
*)

// ---------------------------------------------------

[<Measure>]
type ml

type OverflowingEventArgs(amountOverflowing : float<ml>) =
    inherit EventArgs()
    
    member this.AmountOverflowing = amountOverflowing

type EmptyCoffeeCupDelegate = delegate of obj * EventArgs ->  unit
type OverflowingCoffeeCupDelegate = delegate of obj * OverflowingEventArgs ->  unit

type EventfulCoffeeCup(amount : float<ml>, cupSize : float<ml>) =
    let mutable m_amountLeft = amount
    let m_emptyCupEvent = new Event<EmptyCoffeeCupDelegate, EventArgs>()
    let m_overflowingCupEvent = new Event<OverflowingCoffeeCupDelegate, OverflowingEventArgs>()
    
    member this.Drink(amount) =
//        printfn "Drinking %.1f..." (float amount)
        m_amountLeft <- min (m_amountLeft - amount) 0.0<ml>
        if m_amountLeft <= 0.0<ml> then
            m_emptyCupEvent.Trigger(this, new EventArgs())

    member this.Refil(amountAdded) =
//        printfn "Coffee Cup refilled with %.1f" (float amountAdded)
        m_amountLeft <- m_amountLeft + amountAdded
        
        if m_amountLeft > cupSize then
            let delta = m_amountLeft - cupSize
            m_amountLeft <- cupSize
            m_overflowingCupEvent.Trigger(this, new OverflowingEventArgs(delta))
        
    [<CLIEvent>]
    member this.EmptyCup = m_overflowingCupEvent.Publish
    
    [<CLIEvent>]
    member this.Overflowing = m_overflowingCupEvent.Publish

// ---------------------------------------------------

type RadioBroadcastEventArgs(msg) =
    inherit EventArgs()
    member this.Message : string= msg

type BroadcastEventDelegate = delegate of obj * RadioBroadcastEventArgs -> unit

type RadioStation(frequency, callsign) = 
    
    let m_broadcastEvent = new Event<BroadcastEventDelegate, RadioBroadcastEventArgs>()
    
    member this.Frequency : float = frequency
    member this.Callsign : string = callsign

    member this.BeginBroadcasting(msgs) =
        for msg in msgs do
            m_broadcastEvent.Trigger(this, new RadioBroadcastEventArgs(msg))

    [<CLIEvent>]
    member this.BroadcastSignal = m_broadcastEvent.Publish

// ---------------------------------------------------

[<TestFixture>]
type ObservableModule() =
    
    [<Test>]
    member this.Choose() = 
        
        let coffeeCup = new EventfulCoffeeCup(10.0<ml>, 10.0<ml>)

        let needToCleanupEvent =
            coffeeCup.Overflowing 
            |> Observable.choose(fun amtOverflowingArgs -> 
                        let amtOverflowing = amtOverflowingArgs.AmountOverflowing
                        if   amtOverflowing <  5.0<ml> then None
                        elif amtOverflowing < 10.0<ml> then Some("Medium")
                        else Some("Large"))

        let lastCleanup = ref ""
        needToCleanupEvent.Add(fun amount -> lastCleanup := amount)
        
        // Refil the cup, Overflow event will fire, will fire our 'needToCleanupEvent'
        coffeeCup.Refil(20.0<ml>)
        Assert.AreEqual("Large", !lastCleanup)
        
        // Refil the cup, Overflow event will fire, will fire our 'needToCleanupEvent'
        coffeeCup.Refil(8.0<ml>)
        Assert.AreEqual("Medium", !lastCleanup)
    
        // Refil the cup, Overflow event will fire, will NOT fire our 'needToCleanupEvent'
        lastCleanup := "NA"
        coffeeCup.Refil(2.5<ml>)
        Assert.AreEqual("NA", !lastCleanup)
        
        ()

    [<Test>]
    member this.Filter() = 
    
        let kexp = new RadioStation(90.3, "KEXP")
        
        let fotpSongs =
            kexp.BroadcastSignal
            |> Observable.filter(fun broadEventArgs -> broadEventArgs.Message.Contains("Flight of the Penguins"))
        
        let songsHeard = ref []
        fotpSongs.Add(fun rbEventArgs -> songsHeard := rbEventArgs.Message :: !songsHeard)
        
        // Firing the main event, we should only listen in on those we want to hear
        kexp.BeginBroadcasting(
            [
                "Flaming Hips"
                "Daywish"
                "Flight of the Penguins - song 1"
                "Flight of the Penguins - song 2"
            ])

        Assert.AreEqual(!songsHeard, ["Flight of the Penguins - song 2"; 
                                      "Flight of the Penguins - song 1"])
        ()

    [<Test>]
    member this.Listen() = 
    
        let kqfc = new RadioStation(90.3, "KEXP")
        

        let timesListened = ref 0
        kqfc.BroadcastSignal
        |> Observable.add(fun rbEventArgs -> incr timesListened)

        kqfc.BeginBroadcasting(
            [
                "Elvis"
                "The Beatles"
                "The Rolling Stones"
            ])
            
        // The broadcast event should have fired 3 times
        Assert.AreEqual(!timesListened, 3)
    
        ()

    [<Test>]
    member this.Map() = 
    
        let numEvent = new Event<int>()
        
        let getStr = 
            numEvent.Publish
            |> Observable.map(fun i -> i.ToString())
        
        let results = ref ""
        
        getStr |> Observable.add(fun msg -> results := msg + !results)
        
        numEvent.Trigger(1)
        numEvent.Trigger(22)
        numEvent.Trigger(333)
        
        Assert.AreEqual(!results, "333221")
        ()

    [<Test>]
    member this.Merge() =
        
        let evensEvent = new Event<int>()
        let oddsEvent  = new Event<int>()
        
        let numberEvent = Observable.merge evensEvent.Publish oddsEvent.Publish
        
        let lastResult = ref 0
        numberEvent.Add(fun i -> lastResult := i)
        
        // Verify triggering either the evens or oddsEvent fires the 'numberEvent'
        evensEvent.Trigger(2)
        Assert.AreEqual(!lastResult, 2)

        oddsEvent.Trigger(3)
        Assert.AreEqual(!lastResult, 3)
        
        ()

    [<Test>]
    member this.Pairwise() = 
    
        let numEvent = new Event<int>()
        
        let pairwiseEvent = Observable.pairwise numEvent.Publish
        
        let lastResult = ref (-1, -1)
        pairwiseEvent.Add(fun (x, y) -> lastResult := (x, y))

        // Verify not fired until second call        
        numEvent.Trigger(1)
        Assert.AreEqual(!lastResult, (-1, -1))
        
        numEvent.Trigger(2)
        Assert.AreEqual(!lastResult, (1, 2))
        
        numEvent.Trigger(3)
        Assert.AreEqual(!lastResult, (2, 3))
        
        ()
        
    [<Test>]
    member this.Partition() = 
    
        let numEvent = new Event<int>()
        
        let oddsEvent, evensEvent = Observable.partition (fun i -> (i % 2 = 1)) numEvent.Publish
        
        let lastOdd = ref 0
        oddsEvent.Add(fun i -> lastOdd := i)
        
        let lastEven = ref 0
        evensEvent.Add(fun i -> lastEven := i)
        
        numEvent.Trigger(1)
        Assert.AreEqual(1, !lastOdd)
        Assert.AreEqual(0,!lastEven)
        
        numEvent.Trigger(2)
        Assert.AreEqual(1, !lastOdd) // Not updated
        Assert.AreEqual(2, !lastEven)

        ()
 
    [<Test>]
    member this.Scan() = 
    
        let numEvent = new Event<int>()
        
        let sumEvent = 
            numEvent.Publish 
            |> Observable.scan(fun acc i -> acc + i) 0
            
        let lastSum = ref 0
        sumEvent.Add(fun sum -> lastSum := sum)
        
        numEvent.Trigger(1)
        Assert.AreEqual(!lastSum, 1)
        
        numEvent.Trigger(10)
        Assert.AreEqual(!lastSum, 11)
        
        numEvent.Trigger(100)
        Assert.AreEqual(!lastSum, 111)
        
        ()
        
    [<Test>]
    member this.Split() = 
    
        let numEvent = new Event<int>()
        
        // Note the different types int -> { string * int, string * string }
        let positiveEvent, negativeEvent =
            numEvent.Publish |> Observable.split(fun i -> if i > 0 then Choice1Of2(i.ToString(), i)
                                                          else          Choice2Of2(i.ToString(), i.ToString()))
                 
        let lastResult = ref ""
        positiveEvent.Add(fun (msg, i)    -> lastResult := sprintf "Positive [%s][%d]" msg i)
        negativeEvent.Add(fun (msg, msg2) -> lastResult := sprintf "Negative [%s][%s]" msg msg2)
        
        numEvent.Trigger(10)
        Assert.AreEqual("Positive [10][10]", !lastResult)
        
        numEvent.Trigger(-3)
        Assert.AreEqual("Negative [-3][-3]", !lastResult)
        
        ()
