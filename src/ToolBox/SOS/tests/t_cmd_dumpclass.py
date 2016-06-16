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
        ci, "name2ee " + assemblyName + " Test.DumpClass",
        "MethodDesc:\s+([0-9a-fA-F]+)")
    class_addr = testutils.exec_and_find(
        ci, "dumpmd  " + md_addr, "Class:\s+([0-9a-fA-F]+)")

    result = False
    if class_addr:
        ci.HandleCommand("dumpclass " + class_addr, res)
        if res.Succeeded():
            result = True
        else:
            print("DumpIL failed:")
            print(res.GetOutput())
            print(res.GetError())

    process.Continue()
    return result
