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
    ci.HandleCommand("histinit", res)
    if res.Succeeded():
        result = (res.GetOutput().find("STRESS LOG:") != -1)
    else:
        print("HistInit command failed:")
        print(res.GetOutput())
        print(res.GetError())

    process.Continue()
    return result
