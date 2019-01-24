// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// Various tests for the:
// Microsoft.FSharp.Control.Event module

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

// Note types like BroadcastEvent and EventfulCoffeeCup are
// defined in ObservableModule.fs

// ---------------------------------------------------


type internal InternalDelegate = delegate of obj * unit -> unit
type MultiArgDelegate2 = delegate of obj * int * string -> unit
type MultiArgDelegate3 = delegate of obj * int * string * bool -> unit
type MultiArgDelegate4 = delegate of obj * int * string * bool * int -> unit
type MultiArgDelegate5 = delegate of obj * int * string * bool * int * string -> unit
type MultiArgDelegate6 = delegate of obj * int * string * bool * int * string * bool -> unit

[<TestFixture>]
type EventModule() =
    
    [<Test>]
    member this.Choose() = 
        
        let coffeeCup = new EventfulCoffeeCup(10.0<ml>, 10.0<ml>)
        
        let needToCleanupEvent =
            coffeeCup.Overflowing 
            |> Event.choose(fun amtOverflowingArgs -> 
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
            |> Event.filter(fun broadEventArgs -> broadEventArgs.Message.Contains("Flight of the Penguins"))
        
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
        |> Event.add(fun rbEventArgs -> incr timesListened)

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
            |> Event.map(fun i -> i.ToString())
        
        let results = ref ""
        
        getStr |> Event.add(fun msg -> results := msg + !results)
        
        numEvent.Trigger(1)
        numEvent.Trigger(22)
        numEvent.Trigger(333)
        
        Assert.AreEqual(!results, "333221")
        ()

    [<Test>]
    member this.Merge() =
        
        let evensEvent = new Event<int>()
        let oddsEvent  = new Event<int>()
        
        let numberEvent = Event.merge evensEvent.Publish oddsEvent.Publish
        
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
        
        let pairwiseEvent = Event.pairwise numEvent.Publish
        
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
        
        let oddsEvent, evensEvent = Event.partition (fun i -> (i % 2 = 1)) numEvent.Publish
        
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
            |> Event.scan(fun acc i -> acc + i) 0
            
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
            numEvent.Publish |> Event.split(fun i -> if i > 0 then Choice1Of2(i.ToString(), i)
                                                          else          Choice2Of2(i.ToString(), i.ToString()))
                 
        let lastResult = ref ""
        positiveEvent.Add(fun (msg, i)    -> lastResult := sprintf "Positive [%s][%d]" msg i)
        negativeEvent.Add(fun (msg, msg2) -> lastResult := sprintf "Negative [%s][%s]" msg msg2)
        
        numEvent.Trigger(10)
        Assert.AreEqual("Positive [10][10]", !lastResult)
        
        numEvent.Trigger(-3)
        Assert.AreEqual("Negative [-3][-3]", !lastResult)
        
        ()

    [<Test>]
    member this.InternalDelegate() = 
        let event = new Event<InternalDelegate, unit>()
        let p = event.Publish
        use s = p.Subscribe(fun _ -> ())
        event.Trigger(null, ())

    [<Test>]
    member this.MultipleArguments() =  
        let count = ref 0
        let test (evt : Event<_, _>) arg = 
            let p = evt.Publish
            use s = p.Subscribe(fun _ -> incr count)
            evt.Trigger(null, arg)

        test (new Event<MultiArgDelegate2, _>()) (1, "")
        test (new Event<MultiArgDelegate3, _>()) (1, "", true)
        test (new Event<MultiArgDelegate4, _>()) (1, "", true, 1)
        test (new Event<MultiArgDelegate5, _>()) (1, "", true, 1, "")
        test (new Event<MultiArgDelegate6, _>()) (1, "", true, 1, "", true)

        Assert.AreEqual(5, !count)

        