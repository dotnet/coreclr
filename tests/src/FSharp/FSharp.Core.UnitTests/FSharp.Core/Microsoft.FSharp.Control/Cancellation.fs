// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

namespace FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Control
#nowarn "52"
open System
open FSharp.Core.UnitTests.LibraryTestFx
open NUnit.Framework
open System.Threading


[<TestFixture>]
type CancellationType() =

    [<Test>]
    member this.CancellationNoCallbacks() =
        let _ : CancellationTokenSource = null // compilation test
        use cts1 = new CancellationTokenSource()
        let token1 = cts1.Token
        Assert.IsFalse (token1.IsCancellationRequested)
        use cts2 = new CancellationTokenSource()
        let token2 = cts2.Token
        Assert.IsFalse (token2.IsCancellationRequested)
        cts1.Cancel()
        Assert.IsTrue(token1.IsCancellationRequested)
        Assert.IsFalse (token2.IsCancellationRequested)
        cts2.Cancel()
        Assert.IsTrue(token2.IsCancellationRequested)
        
    [<Test>]
    member this.CancellationRegistration() =
        let cts = new CancellationTokenSource()
        let token = cts.Token
        let called = ref false
        let r = token.Register(Action<obj>(fun _ -> called := true), null)
        Assert.IsFalse(!called)
        r.Dispose()
        cts.Cancel()
        Assert.IsFalse(!called)
        
    [<Test>]
    member this.CancellationWithCallbacks() =
        let cts1 = new CancellationTokenSource()
        let cts2 = new CancellationTokenSource()
        let is1Called = ref false
        let is2Called = ref false
        let is3Called = ref false
        let assertAndOff (expected:bool) (r:bool ref) = Assert.AreEqual(expected,!r); r := false
        let r1 = cts1.Token.Register(Action<obj>(fun _ -> is1Called := true), null)
        let r2 = cts1.Token.Register(Action<obj>(fun _ -> is2Called := true), null)
        let r3 = cts2.Token.Register(Action<obj>(fun _ -> is3Called := true), null) 
        Assert.IsFalse(!is1Called)
        Assert.IsFalse(!is2Called)
        r2.Dispose()
        
        // Cancelling cts1: r2 is disposed and r3 is for cts2, only r1 should be called
        cts1.Cancel()
        assertAndOff true   is1Called
        assertAndOff false  is2Called
        assertAndOff false  is3Called
        Assert.IsTrue(cts1.Token.IsCancellationRequested)
        
        let isAnotherOneCalled = ref false
        let _ = cts1.Token.Register(Action<obj>(fun _ -> isAnotherOneCalled := true), null)
        assertAndOff true isAnotherOneCalled
        
        // Cancelling cts2: only r3 should be called
        cts2.Cancel()
        assertAndOff false  is1Called
        assertAndOff false  is2Called
        assertAndOff true   is3Called
        Assert.IsTrue(cts2.Token.IsCancellationRequested)
        
        
        // Cancelling cts1 again: no one should be called
        cts1.Cancel()
        assertAndOff false is1Called
        assertAndOff false is2Called
        assertAndOff false is3Called
        
        // Disposing
        let token = cts2.Token
        cts2.Dispose()
        Assert.IsTrue(token.IsCancellationRequested)
        let () =
            let mutable odeThrown = false
            try
                r3.Dispose()
            with
            |   :? ObjectDisposedException -> odeThrown <- true
            Assert.IsFalse(odeThrown)
            
        let () =
            let mutable odeThrown = false
            try
                cts2.Token.Register(Action<obj>(fun _ -> ()), null) |> ignore
            with
            |   :? ObjectDisposedException -> odeThrown <- true
            Assert.IsTrue(odeThrown)
        ()
        
    [<Test>]    
    member this.CallbackOrder() = 
        use cts = new CancellationTokenSource()
        let current = ref 0
        let action (o:obj) = Assert.AreEqual(!current, (unbox o : int)); current := !current + 1
        cts.Token.Register(Action<obj>(action), box 2) |> ignore
        cts.Token.Register(Action<obj>(action), box 1) |> ignore
        cts.Token.Register(Action<obj>(action), box 0) |> ignore
        cts.Cancel()
        
    [<Test>]
    member this.CallbackExceptions() =
        use cts = new CancellationTokenSource()
        let action (o:obj) = new InvalidOperationException(String.Format("{0}", o)) |> raise
        cts.Token.Register(Action<obj>(action), box 0) |> ignore
        cts.Token.Register(Action<obj>(action), box 1) |> ignore
        cts.Token.Register(Action<obj>(action), box 2) |> ignore
        let mutable exnThrown = false
        try
            cts.Cancel()
        with
        | :? AggregateException as ae ->
                exnThrown <- true
                ae.InnerExceptions |> Seq.iter (fun e -> (e :? InvalidOperationException) |> Assert.IsTrue)
                let msgs = ae.InnerExceptions |> Seq.map (fun e -> e.Message) |> Seq.toList
                Assert.AreEqual(["2";"1";"0"], msgs)
        Assert.IsTrue exnThrown
        Assert.IsTrue cts.Token.IsCancellationRequested
        
    [<Test>]
    member this.LinkedSources() =
        let () =
            use cts1 = new CancellationTokenSource()
            use cts2 = new CancellationTokenSource()
            use ctsLinked = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, cts2.Token)
            let linkedToken = ctsLinked.Token
            Assert.IsFalse(linkedToken.IsCancellationRequested)
            cts1.Cancel()
            Assert.IsTrue(linkedToken.IsCancellationRequested)
            
        let () = 
            use cts1 = new CancellationTokenSource()
            use cts2 = new CancellationTokenSource()
            use ctsLinked = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, cts2.Token)
            let linkedToken = ctsLinked.Token
            Assert.IsFalse(linkedToken.IsCancellationRequested)
            cts2.Cancel()
            Assert.IsTrue(linkedToken.IsCancellationRequested)
            
        let () =            
            use cts1 = new CancellationTokenSource()
            use cts2 = new CancellationTokenSource()
            cts1.Cancel()
            use ctsLinked = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, cts2.Token)
            let linkedToken = ctsLinked.Token            
            Assert.IsTrue(linkedToken.IsCancellationRequested)
            let doExec = ref false
            linkedToken.Register(Action<obj>(fun _ -> doExec := true), null) |> ignore
            Assert.IsTrue(!doExec)
            
        let () =
            use cts1 = new CancellationTokenSource()
            use cts2 = new CancellationTokenSource()
            use ctsLinked = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, cts2.Token)
            let linkedToken = ctsLinked.Token            
            let doExec = ref false
            linkedToken.Register(Action<obj>(fun _ -> doExec := true), null) |> ignore
            Assert.IsFalse(!doExec)
            cts1.Cancel()
            Assert.IsTrue(!doExec)
            
        let () =
            use cts1 = new CancellationTokenSource()
            use cts2 = new CancellationTokenSource()
            let token1 = cts1.Token
            let token2 = cts2.Token
            use ctsLinked = CancellationTokenSource.CreateLinkedTokenSource(token1, token2)                        
            let linkedToken = ctsLinked.Token    
            Assert.IsFalse(linkedToken.IsCancellationRequested)            
            ctsLinked.Cancel()
            Assert.IsTrue(linkedToken.IsCancellationRequested)
            Assert.IsFalse(token1.IsCancellationRequested)            
            Assert.IsFalse(token2.IsCancellationRequested)            
            
        ()
    
    [<Test>]  
    member this.TestCancellationRace() =
        use cts = new CancellationTokenSource()
        let token = cts.Token
        let callbackRun = ref false
        let reg = token.Register(Action<obj>(fun _ ->
                lock callbackRun (fun() ->
                    Assert.IsFalse(!callbackRun, "Callback should run only once")
                    callbackRun := true
                )
            ), null)
        Assert.IsFalse(!callbackRun)
        let asyncs = seq { for i in 1..1000 do yield async { cts.Cancel() } }
        asyncs |> Async.Parallel |> Async.RunSynchronously |> ignore
        Assert.IsTrue(!callbackRun, "Callback should run at least once")

    [<Test>]
    member this.TestRegistrationRace() =
        let asyncs =
            seq { for _ in 1..1000 do
                    let cts = new CancellationTokenSource()
                    let token = cts.Token
                    yield async { cts.Cancel() } 
                    let callback (_:obj) =
                        Assert.IsTrue(token.IsCancellationRequested)
                    yield async { 
                            do token.Register(Action<obj>(callback), null) |> ignore 
                        }                     
            }               
        (asyncs |> Async.Parallel |> Async.RunSynchronously |> ignore)

    [<Test>]
    member this.LinkedSourceCancellationRace() =
        let asyncs =
            seq { for _ in 1..1000 do
                    let cts1 = new CancellationTokenSource()
                    let token1 = cts1.Token
                    let cts2 = new CancellationTokenSource()
                    let token2 = cts2.Token
                    let linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token1, token2)
                    yield async { do cts1.Cancel() } 
                    yield async { do linkedCts.Dispose() }                     
            }               
        asyncs |> Async.Parallel |> Async.RunSynchronously |> ignore

    [<Test>]
    member this.AwaitTaskCancellationAfterAsyncTokenCancellation() =
        let StartCatchCancellation cancellationToken (work) =
            Async.FromContinuations(fun (cont, econt, _) ->
              // When the child is cancelled, report OperationCancelled
              // as an ordinary exception to "error continuation" rather
              // than using "cancellation continuation"
              let ccont e = econt e
              // Start the workflow using a provided cancellation token
              Async.StartWithContinuations( work, cont, econt, ccont,
                                            ?cancellationToken=cancellationToken) )

        /// Like StartAsTask but gives the computation time to so some regular cancellation work
        let StartAsTaskProperCancel taskCreationOptions  cancellationToken (computation : Async<_>) : System.Threading.Tasks.Task<_> =
            let token = defaultArg cancellationToken Async.DefaultCancellationToken
            let taskCreationOptions = defaultArg taskCreationOptions System.Threading.Tasks.TaskCreationOptions.None
            let tcs = new System.Threading.Tasks.TaskCompletionSource<_>("StartAsTaskProperCancel", taskCreationOptions)

            let a =
                async {
                    try
                        // To ensure we don't cancel this very async (which is required to properly forward the error condition)
                        let! result = StartCatchCancellation (Some token) computation
                        do
                            tcs.SetResult(result)
                    with exn ->
                        tcs.SetException(exn)
                }
            Async.Start(a)
            tcs.Task

        let cts = new CancellationTokenSource()
        let tcs = System.Threading.Tasks.TaskCompletionSource<_>()
        let t =
            async {
                do! tcs.Task |> Async.AwaitTask
            }
            |> StartAsTaskProperCancel None (Some cts.Token)

        // First cancel the token, then set the task as cancelled.
        async {
            do! Async.Sleep 100
            cts.Cancel()
            do! Async.Sleep 100
            tcs.TrySetException (TimeoutException "Task timed out after token.")
                |> ignore
        } |> Async.Start

        try
            let res = t.Wait(1000)
            Assert.Fail (sprintf "Excepted TimeoutException wrapped in an AggregateException, but got %A" res)
        with :? AggregateException as agg -> ()

    [<Test>]
    member this.Equality() =
        let cts1 = new CancellationTokenSource()
        let cts2 = new CancellationTokenSource()
        let t1a = cts1.Token
        let t1b = cts1.Token
        let t2 = cts2.Token        
        Assert.IsTrue((t1a = t1b))
        Assert.IsFalse(t1a <> t1b)
        Assert.IsTrue(t1a <> t2)
        Assert.IsFalse((t1a = t2))
        
        let r1a = t1a.Register(Action<obj>(fun _ -> ()), null)
        let r1b = t1b.Register(Action<obj>(fun _ -> ()), null)
        let r2 = t2.Register(Action<obj>(fun _ -> ()), null)
        let r1a' = r1a
        Assert.IsTrue((r1a = r1a'))
        Assert.IsFalse((r1a = r1b))
        Assert.IsFalse((r1a = r2))
        
        Assert.IsFalse((r1a <> r1a'))
        Assert.IsTrue((r1a <> r1b))
        Assert.IsTrue((r1a <> r2))


