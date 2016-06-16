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
        ci, "name2ee " + assemblyName + " Test.DumpIL",
        "MethodDesc:\s+([0-9a-fA-F]+)")

    result = False
    if md_addr:
        ci.HandleCommand("dumpil " + md_addr, res)
        if res.Succeeded():
            insts = res.GetOutput()
            print(insts)
            result = True
        else:
            print("DumpIL failed:")
            print(res.GetOutput())
            print(res.GetError())

    process.Continue()
    return result
