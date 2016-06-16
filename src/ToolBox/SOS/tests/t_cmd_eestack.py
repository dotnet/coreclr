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
    ci.HandleCommand("eestack", res)
    if res.Succeeded():
        bt = res.GetOutput()
        result = (bt.find("Current frame:") != -1)
    else:
        print("EEStack failed:")
        print(res.GetOutput())
        print(res.GetError())

    process.Continue()
    return result
