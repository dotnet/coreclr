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
        mt_addr = testutils.exec_and_find(ci, "dumpobj " + obj,
                                          "MethodTable:\s+([0-9a-fA-F]+)")
        if res.Succeeded() and mt_addr:
            result = True
        else:
            print("DumpStackObjects failed:")
            print(res.GetOutput())
            print(res.GetError())

    process.Continue()
    return result
