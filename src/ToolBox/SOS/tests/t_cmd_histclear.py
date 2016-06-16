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
    ci.HandleCommand("histclear", res)
    if res.Succeeded():
        result = (res.GetOutput().find("Completed successfully.") != -1)
    else:
        print("HistClear command failed:")
        print(res.GetOutput())
        print(res.GetError())

    process.Continue()
    return result
