import lldb
import re
import os
import testutils


def runScenario(assemblyName, debugger, target):
    process = target.GetProcess()
    res = lldb.SBCommandReturnObject()
    ci = debugger.GetCommandInterpreter()

    testutils.stop_in_main(ci, process, assemblyName)
    jit_addr = testutils.exec_and_find(
        ci, "name2ee " + assemblyName + " Test.Main",
        "JITTED Code Address:\s+([0-9a-fA-F]+)")

    result = False
    if jit_addr:
        ci.HandleCommand("ip2md " + jit_addr, res)
        if res.Succeeded():
            result = res.GetOutput().find("MethodDesc:") != -1
        else:
            print("ClrU failed:")
            print(res.GetOutput())
            print(res.GetError())

    process.Continue()
    return result
