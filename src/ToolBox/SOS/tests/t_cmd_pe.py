import lldb
import re
import os
import testutils


def runScenario(assemblyName, debugger, target):
    process = target.GetProcess()
    res = lldb.SBCommandReturnObject()
    ci = debugger.GetCommandInterpreter()

    testutils.stop_in_main(ci, process, assemblyName)
    ci.HandleCommand("pe", res)

    result = res.Succeeded()
    if not result:
        print("command PrintException failed:")
        print(res.GetOutput())
        print(res.GetError())

    process.Continue()
    return result
