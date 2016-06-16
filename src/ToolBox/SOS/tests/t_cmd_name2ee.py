import lldb
import re
import os
import testutils


def runScenario(assemblyName, debugger, target):
    process = target.GetProcess()
    res = lldb.SBCommandReturnObject()
    ci = debugger.GetCommandInterpreter()

    testutils.stop_in_main(ci, process, assemblyName)
    md_addr = testutils.exec_and_find(
        ci, "name2ee " + assemblyName + " Test.Name2EE",
        "MethodDesc:\s+([0-9a-fA-F]+)")

    result = bool(md_addr)
    if not result:
        print("command Name2EE failed:")
        print(res.GetOutput())
        print(res.GetError())

    process.Continue()
    return result
