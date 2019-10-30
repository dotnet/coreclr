// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

using TestLibrary;

namespace BinderTracingTests
{
    [AttributeUsage(System.AttributeTargets.Method)]
    class BinderTestAttribute : Attribute
    {
        public bool Isolate { get; private set; }
        public BinderTestAttribute(bool isolate = false)
        {
            Isolate = isolate;
        }
    }

    class BinderTracingTest
    {
        public class CustomALC : AssemblyLoadContext
        {
            public CustomALC(string name) : base(name)
            { }
        }

        private const string DefaultALC = "Default";

        [BinderTest]
        public static BindOperation LoadFile()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            Assembly asm = Assembly.LoadFile(executingAssembly.Location);

            return new BindOperation()
            {
                AssemblyName = executingAssembly.GetName(),
                AssemblyPath = executingAssembly.Location,
                AssemblyLoadContext = AssemblyLoadContext.GetLoadContext(asm).ToString(),
                Success = true,
                ResultAssemblyName = asm.GetName(),
                ResultAssemblyPath = asm.Location,
                Cached = false
            };
        }

        [BinderTest]
        public static BindOperation LoadBytes()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            Byte[] bytes = File.ReadAllBytes(executingAssembly.Location);
            Assembly asm = Assembly.Load(bytes);

            return new BindOperation()
            {
                AssemblyName = executingAssembly.GetName(),
                AssemblyLoadContext = AssemblyLoadContext.GetLoadContext(asm).ToString(),
                Success = true,
                ResultAssemblyName = asm.GetName(),
                ResultAssemblyPath = asm.Location,
                Cached = false
            };
        }

        [BinderTest]
        public static BindOperation LoadFromStream()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            Stream stream = File.OpenRead(executingAssembly.Location);
            CustomALC alc = new CustomALC(nameof(LoadFromStream));
            Assembly asm = alc.LoadFromStream(stream);

            return new BindOperation()
            {
                AssemblyName = executingAssembly.GetName(),
                AssemblyLoadContext = alc.ToString(),
                Success = true,
                ResultAssemblyName = asm.GetName(),
                ResultAssemblyPath = asm.Location,
                Cached = false
            };
        }

        [BinderTest]
        public static BindOperation LoadFromAssemblyPath()
        {
            CustomALC alc = new CustomALC(nameof(LoadFromAssemblyPath));
            var executingAssembly = Assembly.GetExecutingAssembly();
            Assembly asm = alc.LoadFromAssemblyPath(executingAssembly.Location);

            return new BindOperation()
            {
                AssemblyName = executingAssembly.GetName(),
                AssemblyPath = executingAssembly.Location,
                AssemblyLoadContext = alc.ToString(),
                Success = true,
                ResultAssemblyName = asm.GetName(),
                ResultAssemblyPath = asm.Location,
                Cached = false
            };
        }

        [BinderTest(isolate: true)]
        public static BindOperation LoadFrom()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            Assembly asm = Assembly.LoadFrom(executingAssembly.Location);

            return new BindOperation()
            {
                AssemblyName = executingAssembly.GetName(),
                AssemblyPath = executingAssembly.Location,
                AssemblyLoadContext = DefaultALC,
                Success = true,
                ResultAssemblyName = asm.GetName(),
                ResultAssemblyPath = asm.Location,
                Cached = false
            };
        }

        [BinderTest(isolate: true)]
        public static BindOperation PlatformAssembly()
        {
            string assemblyName = "System.Xml";
            Assembly asm = Assembly.Load(assemblyName);
            
            return new BindOperation()
            {
                AssemblyName = new AssemblyName(assemblyName),
                AssemblyLoadContext = DefaultALC,
                Success = true,
                ResultAssemblyName = asm.GetName(),
                ResultAssemblyPath = asm.Location,
                Cached = false
            };
        }

        [BinderTest]
        public static BindOperation NonExistentAssembly()
        {
            string assemblyName = "DoesNotExist";
            try
            {
                Assembly.Load(assemblyName);
            }
            catch { }

            return new BindOperation()
            {
                AssemblyName = new AssemblyName(assemblyName),
                AssemblyLoadContext = DefaultALC,
                Success = false,
                Cached = false
            };
        }

        [BinderTest(isolate: true)]
        public static BindOperation Reflection()
        {
            string assemblyName = "AssemblyToLoad";
            var t = Type.GetType($"AssemblyToLoad.Program, {assemblyName}");

            return new BindOperation()
            {
                AssemblyName = new AssemblyName(assemblyName),
                AssemblyLoadContext = DefaultALC,
                RequestingAssembly = Assembly.GetExecutingAssembly().GetName(),
                Success = true,
                ResultAssemblyName = t.Assembly.GetName(),
                ResultAssemblyPath = t.Assembly.Location,
                Cached = false,
            };
        }

        [BinderTest(isolate: true)]
        public static BindOperation JITLoad()
        {
            Assembly asm = UseDependentAssembly();

            return new BindOperation()
            {
                AssemblyName = asm.GetName(),
                AssemblyLoadContext = DefaultALC,
                RequestingAssembly = Assembly.GetExecutingAssembly().GetName(),
                Success = true,
                ResultAssemblyName = asm.GetName(),
                ResultAssemblyPath = asm.Location,
                Cached = false,
            };
        }

        public static bool RunAllTests()
        {
            MethodInfo[] methods = typeof(BinderTracingTest)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.GetCustomAttributes(typeof(BinderTestAttribute), false).Length > 0 && m.ReturnType == typeof(BindOperation))
                .ToArray();

            foreach (var method in methods)
            {
                BinderTestAttribute attribute = (BinderTestAttribute)method.GetCustomAttributes(typeof(BinderTestAttribute)).First();
                bool success = attribute.Isolate
                    ? RunTestInSeparateProcess(method)
                    : RunSingleTest(method);
                if (!success)
                {
                    return false;
                }
            }

            return true;
        }

        public static int Main(string[] args)
        {
            bool success;
            try
            {
                if (args.Length == 0)
                {
                    success = RunAllTests();
                }
                else
                {
                    // Run specific test - first argument should be the test method name
                    MethodInfo method = typeof(BinderTracingTest)
                        .GetMethod(args[0], BindingFlags.Public | BindingFlags.Static);
                    Assert.IsTrue(method.GetCustomAttributes(typeof(BinderTestAttribute), false).Length > 0 && method.ReturnType == typeof(BindOperation));
                    success = RunSingleTest(method);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Test Failure: {e}");
                return 101;
            }

            return success ? 100 : 101;
        }

        private static Assembly UseDependentAssembly()
        {
            var p = new AssemblyToLoad.Program();
            return Assembly.GetAssembly(p.GetType());
        }

        private static bool RunSingleTest(MethodInfo method)
        {
            Console.WriteLine($"Running {method.Name}...");
            try
            {
                Func<BindOperation> func = (Func<BindOperation>)method.CreateDelegate(typeof(Func<BindOperation>));
                using (var listener = new BinderEventListener())
                {
                    BindOperation expected = func();
                    ValidateSingleBind(listener, expected.AssemblyName.Name, expected);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Test {method.Name} failed: {e}");
                return false;
            }

            return true;
        }

        private static bool RunTestInSeparateProcess(MethodInfo method)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                Arguments = $"{Assembly.GetExecutingAssembly().Location} {method.Name}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Console.WriteLine($"Launching process for {method.Name}...");
            using (Process p = Process.Start(startInfo))
            {
                p.OutputDataReceived += (_, args) => Console.WriteLine(args.Data);
                p.BeginOutputReadLine();

                p.ErrorDataReceived += (_, args) => Console.Error.WriteLine(args.Data);
                p.BeginErrorReadLine();

                p.WaitForExit();
                return p.ExitCode == 100;
            }
        }

        private static void ValidateSingleBind(BinderEventListener listener, string assemblyName, BindOperation expected)
        {
            BindOperation[] binds = listener.WaitAndGetEventsForAssembly(assemblyName);
            Assert.IsTrue(binds.Length == 1, $"Bind event count for {assemblyName} - expected: 1, actual: {binds.Length}");
            BindOperation actual = binds[0];
            
            ValidateAssemblyName(expected.AssemblyName, actual.AssemblyName, nameof(BindOperation.AssemblyName));
            Assert.AreEqual(expected.AssemblyPath ?? string.Empty, actual.AssemblyPath, $"Unexpected value for {nameof(BindOperation.AssemblyPath)} on event");
            Assert.AreEqual(expected.AssemblyLoadContext, actual.AssemblyLoadContext, $"Unexpected value for {nameof(BindOperation.AssemblyLoadContext)} on event");
            ValidateAssemblyName(expected.RequestingAssembly, actual.RequestingAssembly, nameof(BindOperation.RequestingAssembly));

            Assert.AreEqual(expected.Success, actual.Success, $"Unexpected value for {nameof(BindOperation.Success)} on event");
            Assert.AreEqual(expected.ResultAssemblyPath ?? string.Empty, actual.ResultAssemblyPath, $"Unexpected value for {nameof(BindOperation.ResultAssemblyPath)} on event");
            Assert.AreEqual(expected.Cached, actual.Cached, $"Unexpected value for {nameof(BindOperation.Cached)} on event");
            ValidateAssemblyName(expected.ResultAssemblyName, actual.ResultAssemblyName, nameof(BindOperation.ResultAssemblyName));
        }

        private static void ValidateAssemblyName(AssemblyName expected, AssemblyName actual, string propertyName)
        {
            if (expected == null)
            {
                return;
            }
            
            if (expected.Version != null)
            {
                Assert.AreEqual(expected.FullName, actual.FullName, $"Unexpected value for {propertyName} on event");
            }
            else 
            {
                Assert.AreEqual(expected.Name, actual.Name, $"Unexpected value for {propertyName} on event");
            }
        }
    }
}
