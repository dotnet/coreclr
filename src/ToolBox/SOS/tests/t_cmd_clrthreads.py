import lldb
import re
import os
import testutils


def runScenario(assemblyName, debugger, target):
    process = target.GetProcess()
    res = lldb.SBCommandReturnObject()
    ci = debugger.GetCommandInterpreter()

    testutils.stop_in_main(ci, process, assemblyName)

    result = False
    print(debugger.StateAsCString(process.GetState()))
    ci.HandleCommand("clrthreads", res)
    print(debugger.StateAsCString(process.GetState()))
    if res.Succeeded():
        bt = res.GetOutput()
        result = (bt.find("ThreadCount:") != -1)
    else:
        print("ClrThreads failed:")
        print(res.GetOutput())
        print(res.GetError())

    process.Continue()
    return result
