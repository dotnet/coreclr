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
    ci.HandleCommand("dumpheap", res)
    if res.Succeeded():
        bt = res.GetOutput()
        result = (bt.find("Address") != -1)
    else:
        print("DumpHeap failed:")
        print(res.GetOutput())
        print(res.GetError())

    process.Continue()
    return result
