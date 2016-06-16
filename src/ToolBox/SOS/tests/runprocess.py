import os
import lldb
import sys
import importlib
from test_libsosplugin import fail_flag


def run(assembly, module):
    global fail_flag
    debugger = lldb.debugger

    debugger.SetAsync(False)
    target = lldb.target

    debugger.HandleCommand("process launch -s")
    debugger.HandleCommand("breakpoint set -n LoadLibraryExW")

    target.GetProcess().Continue()

    debugger.HandleCommand("breakpoint delete 1")
    #run the scenario
    print("starting scenario...")
    i = importlib.import_module(module)
    scenarioResult = i.runScenario(os.path.basename(assembly), debugger,
                                   target)

    # clear the failed flag if the exit status is OK
    if scenarioResult is True and target.GetProcess().GetExitStatus() == 0:
        os.unlink(fail_flag)
