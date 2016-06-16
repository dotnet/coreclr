import unittest
import argparse
import re
import tempfile
import subprocess
import threading
import os
import os.path
import sys

lldb = ''
corerun = ''
sosplugin = ''
assembly = ''
fail_flag = '/tmp/fail_flag'
timeout = 0

# helper functions


def prepareScenarioFile(moduleName):
    global assembly
    #create a temporary scenario file
    fd, scenarioFileName = tempfile.mkstemp()
    scenarioFile = open(scenarioFileName, 'w')
    scenarioFile.write('script from runprocess import run\n')
    scenarioFile.write('script run("'+assembly+'", "'+moduleName+'")\n')
    scenarioFile.write('quit\n')
    scenarioFile.close()
    os.close(fd)
    return scenarioFileName


def runWithTimeout(cmd, timeout):
    d = {'process': None}

    def run():
        d['process'] = subprocess.Popen(cmd, shell=True)
        d['process'].communicate()

    thread = threading.Thread(target=run)
    thread.start()

    thread.join(timeout)
    if thread.is_alive():
        d['process'].terminate()
        thread.join()


# Test class
class TestSosCommands(unittest.TestCase):

    def do_test(self, command):
        global corerun
        global sosplugin
        global fail_flag

        # set the flag, if it is not set
        if not os.access(fail_flag, os.R_OK):
            open(fail_flag, "a").close()

        filename = prepareScenarioFile(command)
        cmd = ("%s -b --one-line-before-file \"plugin load %s \" --source %s "
               "-K \"OnCrash.do\" -- %s %s > %s.log 2> %s.log.2" %
               (lldb, sosplugin, filename, corerun, assembly, command,
                command))
        runWithTimeout(cmd, timeout)
        os.unlink(filename)
        self.assertFalse(os.path.isfile(fail_flag))

    def test_bpmd(self):
        self.do_test("t_cmd_bpmd")

    def test_clrstack(self):
        self.do_test("t_cmd_clrstack")

    def test_clrthreads(self):
        self.do_test("t_cmd_clrthreads")

    def test_clru(self):
        self.do_test("t_cmd_clru")

    def test_dumpclass(self):
        self.do_test("t_cmd_dumpclass")

    def test_dumpheap(self):
        self.do_test("t_cmd_dumpheap")

    def test_dumpil(self):
        self.do_test("t_cmd_dumpil")

    def test_dumplog(self):
        self.do_test("t_cmd_dumplog")

    def test_dumpmd(self):
        self.do_test("t_cmd_dumpmd")

    def test_dumpmodule(self):
        self.do_test("t_cmd_dumpmodule")

    def test_dumpmt(self):
        self.do_test("t_cmd_dumpmt")

    def test_dumpobj(self):
        self.do_test("t_cmd_dumpobj")

    def test_dumpstack(self):
        self.do_test("t_cmd_dumpstack")

    def test_dso(self):
        self.do_test("t_cmd_dso")

    def test_eeheap(self):
        self.do_test("t_cmd_eeheap")

    def test_eestack(self):
        self.do_test("t_cmd_eestack")

    def test_gcroot(self):
        self.do_test("t_cmd_gcroot")

    def test_ip2md(self):
        self.do_test("t_cmd_ip2md")

    def test_name2ee(self):
        self.do_test("t_cmd_name2ee")

    def test_pe(self):
        self.do_test("t_cmd_pe")

    def test_histclear(self):
        self.do_test("t_cmd_histclear")

    def test_histinit(self):
        self.do_test("t_cmd_histinit")

    def test_histobj(self):
        self.do_test("t_cmd_histobj")

    def test_histobjfind(self):
        self.do_test("t_cmd_histobjfind")

    def test_histroot(self):
        self.do_test("t_cmd_histroot")

    def test_sos(self):
        self.do_test("t_cmd_sos")

    def test_soshelp(self):
        self.do_test("t_cmd_soshelp")


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--lldb', default='lldb')
    parser.add_argument('--corerun', default='')
    parser.add_argument('--sosplugin', default='')
    parser.add_argument('--assembly', default='test.exe')
    parser.add_argument('--timeout', default=120)
    parser.add_argument('unittest_args', nargs='*')

    args = parser.parse_args()

    lldb = args.lldb
    corerun = args.corerun
    sosplugin = args.sosplugin
    assembly = args.assembly
    timeout = int(args.timeout)
    print("lldb: " + lldb)
    print("corerun: " + corerun)
    print("sosplugin: " + sosplugin)
    print("assembly: " + assembly)
    print("timeout: " + str(timeout))

    sys.argv[1:] = args.unittest_args
    suite = unittest.TestLoader().loadTestsFromTestCase(TestSosCommands)
    unittest.TextTestRunner(verbosity=2).run(suite)

    try:
        os.unlink(fail_flag)
    except:
        pass
