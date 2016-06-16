import lldb
import re
import os
import testutils


def runScenario(assemblyName, debugger, target):
    process = target.GetProcess()
    res = lldb.SBCommandReturnObject()
    ci = debugger.GetCommandInterpreter()

    testutils.stop_in_main(ci, process, assemblyName)
    ci.HandleCommand("sos", res)
    result = res.Succeeded()

    if not result:
        print("command sos failed:")
        print(res.GetOutput())
        print(res.GetError())

    process.Continue()
    return result
