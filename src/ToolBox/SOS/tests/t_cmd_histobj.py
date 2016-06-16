import lldb
import re
import os
import testutils


def runScenario(assemblyName, debugger, target):
    process = target.GetProcess()
    res = lldb.SBCommandReturnObject()
    ci = debugger.GetCommandInterpreter()

    testutils.stop_in_main(ci, process, assemblyName)
    ci.HandleCommand("dso", res)

    result = False
    if res.Succeeded():
        obj = res.GetOutput().split()[-2]
        ci.HandleCommand("histobj " + obj, res)
        if res.Succeeded():
            result = (res.GetOutput().find("GCCount") != -1)
        else:
            print("HistObj command failed:")
            print(res.GetOutput())
            print(res.GetError())

    process.Continue()
    return result
