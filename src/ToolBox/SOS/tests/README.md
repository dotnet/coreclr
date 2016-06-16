Testing libsosplugin
=====================================

**Test assembly**  
Compile test assembly file using any C# compiler you have, for example:  
- `gmcs test.cs`  
- `corerun csc.exe /nologo /r:System.Private.CoreLib.dll test.cs`


**Running tests**  
Make sure that python's lldb module is accessible. To run the tests, use the following command:  
`python2 test_libsosplugin.py --corerun=corerun --sosplugin=sosplugin --assembly=assembly --timeout=timeout`  
- `corerun` is a command that would normally be used to launch `corerun`  
- `sosplugin` is a lldb libsos plugin (e.g. libsosplugin.so)  
- `assembly` is a compiled test assembly (e.g. test.exe)  
- `timeout` is a deadline for a single test (in seconds)

Log files for both failed and passed tests are `*.log` and `*.log.2` for standard output and error correspondingly.


**Writing tests**  
Tests start with the `TestSosCommands` class defined in `test_libsosplugin.py`. To add a test to the suite, start with implementing a new method inside this class whose name begins with `test_`. Most new commands will require only one line of code in this method: `self.do_test("scenarioname")`. This command will launch a new `lldb` instance, which in turn will call the `runScenario` method from `scenarioname` module. `scenarioname` is the name of the python module that will be running the scenario inside `lldb` (found in `tests` folder alongside with `test_libsosplugin.py` and named `scenarioname.py`). 
An example of a scenario looks like this:

	import lldb
	def runScenario(assemblyName, debugger, target):
		process = target.GetProcess()

		# do some work

		process.Continue()
		return True

 `runScenario` method does all the work related to running the scenario: setting breakpoints, running SOS commands and examining their output. It should return a boolean value indicating a success or a failure.  
***Note:*** `testutils.py` defines some useful commands that can be reused in many scenarios.


**Useful links**  
[Python scripting in LLDB](http://lldb.llvm.org/python-reference.html)  
[Python unittest framework](https://docs.python.org/2.7/library/unittest.html)
