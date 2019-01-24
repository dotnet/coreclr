// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// Various tests for the:
// Microsoft.FSharp.Control.Async module

namespace FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Control

open System
open System.Threading
open FSharp.Core.UnitTests.LibraryTestFx
open NUnit.Framework
open FsCheck

module Utils =
    let internal memoizeAsync f =
        let cache = System.Collections.Concurrent.ConcurrentDictionary<'a, System.Threading.Tasks.Task<'b>>()
        fun (x: 'a) -> // task.Result serialization to sync after done.
            cache.GetOrAdd(x, fun x -> f(x) |> Async.StartAsTask) |> Async.AwaitTask

type [<Struct>] Dummy (x: int) =
  member this.X = x
  interface IDisposable with
    member this.Dispose () = ()


[<AutoOpen>]
module ChoiceUtils =

    // FsCheck driven Async.Choice specification test

    exception ChoiceExn of index:int

    /// represents a child computation of a choice workflow
    type ChoiceOp =
        | NoneResultAfter of timeout:int
        | SomeResultAfter of timeout:int
        | ExceptionAfter of timeout:int

        member c.Timeout =
            match c with
            | NoneResultAfter t -> t
            | SomeResultAfter t -> t
            | ExceptionAfter t -> t

    /// represent a choice workflow
    type ChoiceWorkflow = ChoiceWorkflow of children:ChoiceOp list * cancelAfter:int option

    /// normalizes random timeout arguments
    let normalize (ChoiceWorkflow(ops, cancelAfter)) =
        let ms t = 2000 * (abs t % 15) // timeouts only positive multiples of 2 seconds, up to 30 seconds
        let mkOp op =
            match op with
            | NoneResultAfter t -> NoneResultAfter (ms t)
            | SomeResultAfter t -> SomeResultAfter (ms t)
            | ExceptionAfter t -> ExceptionAfter (ms t)

        let ops = ops |> List.map mkOp
        let cancelAfter = cancelAfter |> Option.map ms
        ChoiceWorkflow(ops, cancelAfter)

    /// runs specified choice workflow and checks that
    /// Async.Choice spec is satisfied
    let runChoice (ChoiceWorkflow(ops, cancelAfter)) =
        // Step 1. build a choice workflow from the abstract representation
        let completed = ref 0
        let returnAfter time f = async {
            do! Async.Sleep time
            let _ = Interlocked.Increment completed
            return f ()
        }

        let mkOp (index : int) = function
            | NoneResultAfter t -> returnAfter t (fun () ->  None)
            | SomeResultAfter t -> returnAfter t (fun () -> Some index)
            | ExceptionAfter t -> returnAfter t (fun () -> raise (ChoiceExn index))

        let choiceWorkflow = ops |> List.mapi mkOp |> Async.Choice

        // Step 2. run the choice workflow and keep the results
        let result =
            let cancellationToken =
                match cancelAfter with
                | Some ca -> 
                    let cts = new CancellationTokenSource()
                    cts.CancelAfter(ca)
                    Some cts.Token
                | None -> None

            try Async.RunSynchronously(choiceWorkflow, ?cancellationToken = cancellationToken) |> Choice1Of2 
            with e -> Choice2Of2 e

        // Step 3. check that results are up to spec
        let getMinTime() =
            seq {
                yield Int32.MaxValue // "infinity": avoid exceptions if list is empty

                for op in ops do 
                    match op with
                    | NoneResultAfter _ -> ()
                    | op -> yield op.Timeout

                match cancelAfter with Some t -> yield t | None -> ()
            } |> Seq.min

        let verifyIndex index =
            if index < 0 || index >= ops.Length then
                Assert.Fail "Returned choice index is out of bounds."
        
        // Step 3a. check that output is up to spec
        match result with
        | Choice1Of2 (Some index) ->
            verifyIndex index
            match ops.[index] with
            | SomeResultAfter timeout -> Assert.AreEqual(getMinTime(), timeout)
            | op -> Assert.Fail <| sprintf "Should be 'Some' but got %A" op

        | Choice1Of2 None ->
            Assert.True(ops |> List.forall (function NoneResultAfter _ -> true | _ -> false))

        | Choice2Of2 (:? OperationCanceledException) ->
            match cancelAfter with
            | None -> Assert.Fail "Got unexpected cancellation exception."
            | Some ca -> Assert.AreEqual(getMinTime(), ca)

        | Choice2Of2 (ChoiceExn index) ->
            verifyIndex index
            match ops.[index] with
            | ExceptionAfter timeout -> Assert.AreEqual(getMinTime(), timeout)
            | op -> Assert.Fail <| sprintf "Should be 'Exception' but got %A" op

        | Choice2Of2 e -> Assert.Fail(sprintf "Unexpected exception %O" e)

        // Step 3b. check that nested cancellation happens as expected
        if not <| List.isEmpty ops then
            let minTimeout = getMinTime()
            let minTimeoutOps = ops |> Seq.filter (fun op -> op.Timeout <= minTimeout) |> Seq.length
            Assert.IsTrue(!completed <= minTimeoutOps)

module LeakUtils =
    // when testing for liveness, the things that we want to observe must always be created in
    // a nested function call to avoid the GC (possibly) treating them as roots past the last use in the block.
    // We also need something non trivial to dissuade the compiler from inlining in Release builds.
    type ToRun<'a>(f : unit -> 'a) =
        member this.Invoke() = f()
   
    let run (toRun : ToRun<'a>) = toRun.Invoke()

// ---------------------------------------------------

[<TestFixture>]
type AsyncModule() =
    
    /// Simple asynchronous task that delays 200ms and returns a list of the current tick count
    let getTicksTask =
        async {
            do! Async.SwitchToThreadPool()
            let tickstamps = ref [] // like timestamps but for ticks :)
            
            for i = 1 to 10 do
                tickstamps := DateTime.UtcNow.Ticks :: !tickstamps
                do! Async.Sleep(20)
                
            return !tickstamps
        }

    let wait (wh : System.Threading.WaitHandle) (timeoutMilliseconds : int) = 
        wh.WaitOne(timeoutMilliseconds, exitContext=false)

    let dispose(d : #IDisposable) = d.Dispose()

    let testErrorAndCancelRace computation = 
        for _ in 1..20 do
            let cts = new System.Threading.CancellationTokenSource()
            use barrier = new System.Threading.ManualResetEvent(false)
            async { cts.Cancel() } 
            |> Async.Start

            let c = ref 0
            let incr () = System.Threading.Interlocked.Increment(c) |> ignore

            Async.StartWithContinuations(
                computation,
                (fun _ -> failwith "success not expected"),
                (fun _ -> incr()),
                (fun _ -> incr()),
                cts.Token
            )

            wait barrier 100
            |> ignore
            if !c = 2 then Assert.Fail("both error and cancel continuations were called")

    [<Test>]
    member this.AwaitIAsyncResult() =

        let beginOp, endOp, cancelOp = Async.AsBeginEnd(fun() -> getTicksTask)

        // Begin the async operation and wait
        let operationIAR = beginOp ((), new AsyncCallback(fun iar -> ()), null)
        match Async.AwaitIAsyncResult(operationIAR) |> Async.RunSynchronously with
        | true  -> ()
        | false -> Assert.Fail("Timed out. Expected to succeed.")

        // When the operation has already completed
        let operationIAR = beginOp ((), new AsyncCallback(fun iar -> ()), null)
        sleep(250)
        
        let result = Async.AwaitIAsyncResult(operationIAR) |> Async.RunSynchronously        
        match result with
        | true  -> ()
        | false -> Assert.Fail("Timed out. Expected to succeed.")

        // Now with a timeout
        let operationIAR = beginOp ((), new AsyncCallback(fun iar -> ()), null)
        let result = Async.AwaitIAsyncResult(operationIAR, 1) |> Async.RunSynchronously
        match result with
        | true  -> Assert.Fail("Timeout expected")
        | false -> ()

    [<Test>]
    member this.``AwaitWaitHandle.Timeout``() = 
        use waitHandle = new System.Threading.ManualResetEvent(false)
        let startTime = DateTime.UtcNow

        let r = 
            Async.AwaitWaitHandle(waitHandle, 500)
            |> Async.RunSynchronously

        Assert.IsFalse(r, "Timeout expected")

        let endTime = DateTime.UtcNow
        let delta = endTime - startTime
        Assert.IsTrue(delta.TotalMilliseconds < 1100.0, sprintf "Expected faster timeout than %.0f ms" delta.TotalMilliseconds)

    [<Test>]
    member this.``AwaitWaitHandle.TimeoutWithCancellation``() = 
        use barrier = new System.Threading.ManualResetEvent(false)
        use waitHandle = new System.Threading.ManualResetEvent(false)
        let cts = new System.Threading.CancellationTokenSource()

        Async.AwaitWaitHandle(waitHandle, 5000)
        |> Async.Ignore
        |> fun c -> 
                    Async.StartWithContinuations(
                        c, 
                        (failwithf "Unexpected success %A"), 
                        (failwithf "Unexpected error %A"), 
                        (fun _ -> barrier.Set() |> ignore), 
                        cts.Token
                    )

        // wait a bit then signal cancellation
        let timeout = wait barrier 500
        Assert.IsFalse(timeout, "timeout=true is not expected")

        cts.Cancel()

        // wait 10 seconds for completion
        let ok = wait barrier 10000
        if not ok then Assert.Fail("Async computation was not completed in given time")

    [<Test>]
    member this.``AwaitWaitHandle.DisposedWaitHandle1``() = 
        let wh = new System.Threading.ManualResetEvent(false)
        
        dispose wh
        let test = async {
            try
                let! timeout = Async.AwaitWaitHandle wh
                Assert.Fail(sprintf "Unexpected success %A" timeout)
            with
                | :? ObjectDisposedException -> ()
                | e -> Assert.Fail(sprintf "Unexpected error %A" e)
            }
        Async.RunSynchronously test
    
    [<Test>]
    member this.``OnCancel.RaceBetweenCancellationHandlerAndDisposingHandlerRegistration``() = 
        let test() = 
            use flag = new ManualResetEvent(false)
            use cancelHandlerRegistered = new ManualResetEvent(false)
            let cts = new System.Threading.CancellationTokenSource()
            let go = async {
                use! holder = Async.OnCancel(fun() -> lock flag (fun() -> flag.Set()) |> ignore)
                let _ = cancelHandlerRegistered.Set()
                while true do
                    do! Async.Sleep 50
                }

            Async.Start (go, cancellationToken = cts.Token)
            //wait until we are sure the Async.OnCancel has run:
            Assert.IsTrue(cancelHandlerRegistered.WaitOne(TimeSpan.FromSeconds 5.))
            //now cancel:
            cts.Cancel()
            //cancel handler should have run:
            Assert.IsTrue(flag.WaitOne(TimeSpan.FromSeconds 5.))

        for _i = 1 to 300 do test()

    [<Test>]
    member this.``OnCancel.RaceBetweenCancellationAndDispose``() = 
        let flag = ref 0
        let cts = new System.Threading.CancellationTokenSource()
        let go = async {
            use disp =
                cts.Cancel()
                { new IDisposable with
                    override __.Dispose() = incr flag }
            while true do
                do! Async.Sleep 50
            }
        try
            Async.RunSynchronously (go, cancellationToken = cts.Token)
        with
            :? System.OperationCanceledException -> ()
        Assert.AreEqual(1, !flag)

    [<Test>]
    member this.``OnCancel.CancelThatWasSignalledBeforeRunningTheComputation``() = 
        let test() = 
            let cts = new System.Threading.CancellationTokenSource()
            let go e (flag : bool ref) = async {
                let! _ = Async.AwaitWaitHandle e
                sleep 500
                use! _holder = Async.OnCancel(fun () -> flag := true)
                while true do
                    do! Async.Sleep 100
                }

            let evt = new System.Threading.ManualResetEvent(false)
            let finish = new System.Threading.ManualResetEvent(false)
            let cancelledWasCalled = ref false
            Async.StartWithContinuations(go evt cancelledWasCalled, ignore, ignore, (fun _ -> finish.Set() |> ignore),  cancellationToken = cts.Token)
            sleep 500
            evt.Set() |> ignore
            cts.Cancel()

            let ok = wait finish 3000
            Assert.IsTrue(ok, "Computation should be completed")
            Assert.IsFalse(!cancelledWasCalled, "Cancellation handler should not be called")

        for _i = 1 to 3 do test()


#if EXPENSIVE
    [<Test; Category("Expensive"); Explicit>]
    member this.``Async.AwaitWaitHandle does not leak memory`` () =
        // This test checks that AwaitWaitHandle does not leak continuations (described in #131),
        // We only test the worst case - when the AwaitWaitHandle is already set.
        use manualResetEvent = new System.Threading.ManualResetEvent(true)
        
        let tryToLeak() = 
            let resource = 
                LeakUtils.ToRun (fun () ->
                    let resource = obj()
                    let work = 
                        async { 
                            let! _ = Async.AwaitWaitHandle manualResetEvent
                            GC.KeepAlive(resource)
                            return ()
                        }

                    work |> Async.RunSynchronously |> ignore
                    WeakReference(resource))
                  |> LeakUtils.run

            Assert.IsTrue(resource.IsAlive)

            GC.Collect()
            GC.WaitForPendingFinalizers()
            GC.Collect()
            GC.WaitForPendingFinalizers()
            GC.Collect()

            Assert.IsFalse(resource.IsAlive)
        
        // The leak hangs on a race condition which is really hard to trigger in F# 3.0, hence the 100000 runs...
        for _ in 1..10 do tryToLeak()
#endif

    [<Test>]
    member this.``AwaitWaitHandle.DisposedWaitHandle2``() = 
        let wh = new System.Threading.ManualResetEvent(false)
        let barrier = new System.Threading.ManualResetEvent(false)

        let test = async {
            let! timeout = Async.AwaitWaitHandle(wh, 10000)
            Assert.IsFalse(timeout, "Timeout expected")
            barrier.Set() |> ignore
            }
        Async.Start test

        // await 3 secs then dispose waithandle - nothing should happen
        let timeout = wait barrier 3000
        Assert.IsFalse(timeout, "Barrier was reached too early")
        dispose wh
        
        let ok = wait barrier 10000
        if not ok then Assert.Fail("Async computation was not completed in given time")

    [<Test>]
    member this.``RunSynchronously.NoThreadJumpsAndTimeout``() = 
            let longRunningTask = async { sleep(5000) }
            try
                Async.RunSynchronously(longRunningTask, timeout = 500)
                Assert.Fail("TimeoutException expected")
            with
                :? System.TimeoutException -> ()

    [<Test>]
    member this.``RunSynchronously.NoThreadJumpsAndTimeout.DifferentSyncContexts``() = 
        let run syncContext =
            let old = System.Threading.SynchronizationContext.Current
            System.Threading.SynchronizationContext.SetSynchronizationContext(syncContext)
            let longRunningTask = async { sleep(5000) }
            let failed = ref false
            try
                Async.RunSynchronously(longRunningTask, timeout = 500)
                failed := true
            with
                :? System.TimeoutException -> ()
            System.Threading.SynchronizationContext.SetSynchronizationContext(old)
            if !failed then Assert.Fail("TimeoutException expected")
        run null
        run (System.Threading.SynchronizationContext())

    [<Test>]
    member this.``RaceBetweenCancellationAndError.AwaitWaitHandle``() = 
        let disposedEvent = new System.Threading.ManualResetEvent(false)
        dispose disposedEvent

        testErrorAndCancelRace(Async.AwaitWaitHandle disposedEvent)

    [<Test>]
    member this.``RaceBetweenCancellationAndError.Sleep``() =
        testErrorAndCancelRace (Async.Sleep (-5))

#if EXPENSIVE
#if NET46
    [<Test; Category("Expensive"); Explicit>] // takes 3 minutes!
    member this.``Async.Choice specification test``() =
        ThreadPool.SetMinThreads(100,100) |> ignore
        Check.One ({Config.QuickThrowOnFailure with EndSize = 20}, normalize >> runChoice)
#endif
#endif

    [<Test>]
    member this.``dispose should not throw when called on null``() =
        let result = async { use x = null in return () } |> Async.RunSynchronously

        Assert.AreEqual((), result)

    [<Test>]
    member this.``dispose should not throw when called on null struct``() =
        let result = async { use x = new Dummy(1) in return () } |> Async.RunSynchronously

        Assert.AreEqual((), result)

    [<Test>]
    member this.``error on one workflow should cancel all others``() =
        let counter = 
            async {
                let counter = ref 0
                let job i = async { 
                    if i = 55 then failwith "boom" 
                    else 
                        do! Async.Sleep 1000 
                        incr counter 
                }

                let! _ = Async.Parallel [ for i in 1 .. 100 -> job i ] |> Async.Catch
                do! Async.Sleep 5000
                return !counter
            } |> Async.RunSynchronously

        Assert.AreEqual(0, counter)

    [<Test>]
    member this.``AwaitWaitHandle.ExceptionsAfterTimeout``() = 
        let wh = new System.Threading.ManualResetEvent(false)
        let test = async {
            try
                let! timeout = Async.AwaitWaitHandle(wh, 1000)
                do! Async.Sleep 500
                raise (new InvalidOperationException("EXPECTED"))
                return Assert.Fail("Should not get here")
            with
                :? InvalidOperationException as e when e.Message = "EXPECTED" -> return ()
            }
        Async.RunSynchronously(test)
        
    [<Test>]
    member this.``FromContinuationsCanTailCallCurrentThread``() = 
        let cnt = ref 0
        let origTid = System.Threading.Thread.CurrentThread.ManagedThreadId 
        let finalTid = ref -1
        let rec f n =
            if n = 0 then
                async { 
                    finalTid := System.Threading.Thread.CurrentThread.ManagedThreadId
                    return () }
            else
                async {
                    incr cnt
                    do! Async.FromContinuations(fun (k,_,_) -> k())
                    do! f (n-1) 
                }
        // 5000 is big enough that does-not-stackoverflow means we are tailcalling thru FromContinuations
        f 5000 |> Async.StartImmediate 
        Assert.AreEqual(origTid, !finalTid)
        Assert.AreEqual(5000, !cnt)

    [<Test>]
    member this.``AwaitWaitHandle With Cancellation``() = 
        let run wh = async {
            let! r = Async.AwaitWaitHandle wh
            Assert.IsTrue(r, "Timeout not expected")
            return() 
            }
        let test () = 
            let wh = new System.Threading.ManualResetEvent(false)
            let cts = new System.Threading.CancellationTokenSource()
            let asyncs =
                [ 
                  yield! List.init 100 (fun _ -> run wh)
                  yield async { cts.Cancel() }
                  yield async { wh.Set() |> ignore }
                ]
            try
                asyncs
                  |> Async.Parallel
                  |> fun c -> Async.RunSynchronously(c, cancellationToken = cts.Token)
                  |> ignore
            with
                :? System.OperationCanceledException -> () // OK
        for _ in 1..1000 do test()

    [<Test>]
    member this.``StartWithContinuationsVersusDoBang``() = 
        // worthwhile to note these three
        // case 1
        let r = ref ""
        async {
            try
                do! Async.FromContinuations(fun (s, _, _) -> s())
                return failwith "boom"
            with
                e-> r := e.Message 
            } |> Async.RunSynchronously 
        Assert.AreEqual("boom", !r)
        // case 2
        r := ""
        try
            Async.StartWithContinuations(Async.FromContinuations(fun (s, _, _) -> s()), (fun () -> failwith "boom"), (fun e -> r := e.Message), (fun oce -> ()))
        with
            e -> r := "EX: " + e.Message
        Assert.AreEqual("EX: boom", !r)
        // case 3
        r := ""
        Async.StartWithContinuations(async { return! failwith "boom" }, (fun x -> ()), (fun e -> r := e.Message), (fun oce -> ()))
        Assert.AreEqual("boom", !r)


#if IGNORED
    [<Test; Ignore("See https://github.com/Microsoft/visualfsharp/issues/4887")>]
    member this.``SleepContinuations``() = 
        let okCount = ref 0
        let errCount = ref 0
        let test() = 
            let cts = new System.Threading.CancellationTokenSource() 
 
            System.Threading.ThreadPool.QueueUserWorkItem(fun _-> 
                System.Threading.Thread.Sleep 50 
                try 
                    Async.StartWithContinuations( 
                        Async.Sleep(1000), 
                        (fun _ -> printfn "ok"; incr okCount), 
                        (fun _ -> printfn "error"; incr errCount), 
                        (fun _ -> printfn "cancel"; failwith "BOOM!"), 
                        cancellationToken = cts.Token 
                    ) 
                with _ -> () 
            ) |> ignore 
            System.Threading.Thread.Sleep 50 
            try cts.Cancel() with _ -> () 
            System.Threading.Thread.Sleep 1500 
            printfn "====" 
        for i = 1 to 3 do test()
        Assert.AreEqual(0, !okCount)
        Assert.AreEqual(0, !errCount)
#endif

    [<Test>]
    member this.``Async caching should work``() = 
        let x = ref 0
        let someSlowFunc _mykey = async { 
            Console.WriteLine "Simulated downloading..."
            do! Async.Sleep 400
            Console.WriteLine "Simulated downloading Done."
            x := !x + 1 // Side effect!
            return "" }

        let memFunc : string -> Async<string> = Utils.memoizeAsync <| someSlowFunc

        async {
            Console.WriteLine "Do the same memoized thing many ways...."
            do! memFunc "a" |> Async.Ignore
            do! memFunc "a" |> Async.Ignore
            do! memFunc "a" |> Async.Ignore
            do! [|1 .. 30|] |> Seq.map(fun _ -> (memFunc "a")) 
                |> Async.Parallel |> Async.Ignore

            Console.WriteLine "Still more ways...."
            for _i = 1 to 30 do
                Async.Start( memFunc "a" |> Async.Ignore )
                Async.Start( memFunc "a" |> Async.Ignore )
            do! Async.Sleep 500
            do! memFunc "a" |> Async.Ignore
            do! memFunc "a" |> Async.Ignore
            Console.WriteLine "Still more ways again...."
            for _i = 1 to 30 do
                Async.Start( memFunc "a" |> Async.Ignore )

            Console.WriteLine "Still more ways again again...."
            do! [|1 .. 30|] |> Seq.map(fun _ -> (memFunc "a")) 
                |> Async.Parallel |> Async.Ignore
        } |> Async.RunSynchronously
        Console.WriteLine "Checking result...."
        Assert.AreEqual(1, !x)
