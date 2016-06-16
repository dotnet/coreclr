import lldb
import re
import os
import testutils


def runScenario(assemblyName, debugger, target):
    process = target.GetProcess()
    res = lldb.SBCommandReturnObject()
    ci = debugger.GetCommandInterpreter()

    ci.HandleCommand("bpmd " + assemblyName + " Test.Main", res)
    process.Continue()

    result = res.Succeeded() and (process.GetState() == lldb.eStateStopped)

    if not result:
        print("command bpmd failed: process is not running (exited?)")

    process.Continue()
    return result
