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
    ci.HandleCommand("dumplog", res)
    if res.Succeeded():
        bt = res.GetOutput()
        result = (bt.find(" dump ") != -1)
    else:
        print("DumpLog failed:")
        print(res.GetOutput())
        print(res.GetError())

    process.Continue()
    return result
