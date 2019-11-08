// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

using TestLibrary;

namespace BinderTracingTests
{
    partial class BinderTracingTest
    {
        [BinderTest]
        public static BindOperation ALCResolvingEvent_ReturnNull()
        {
            var assemblyName = new AssemblyName(SubdirectoryAssemblyName);
            using (var handlers = new Handlers(HandlerReturn.Null, AssemblyLoadContext.Default))
            {
                try
                {
                    Assembly.Load(assemblyName);
                }
                catch { }

                Assert.AreEqual(1, handlers.Invocations.Count);
                return new BindOperation()
                {
                    AssemblyName = assemblyName,
                    AssemblyLoadContext = DefaultALC,
                    RequestingAssembly = Assembly.GetExecutingAssembly().GetName(),
                    RequestingAssemblyLoadContext = DefaultALC,
                    Success = false,
                    Cached = false,
                    ALCResolvingHandlers = handlers.Invocations
                };
            }
        }

        [BinderTest]
        public static BindOperation ALCResolvingEvent_LoadAssembly()
        {
            var assemblyName = new AssemblyName(SubdirectoryAssemblyName);
            CustomALC alc = new CustomALC(nameof(ALCResolvingEvent_LoadAssembly));
            using (var handlers = new Handlers(HandlerReturn.RequestedAssembly, alc))
            {
                Assembly asm = alc.LoadFromAssemblyName(assemblyName);

                Assert.AreEqual(1, handlers.Invocations.Count);
                return new BindOperation()
                {
                    AssemblyName = assemblyName,
                    AssemblyLoadContext = alc.ToString(),
                    Success = true,
                    ResultAssemblyName = asm.GetName(),
                    ResultAssemblyPath = asm.Location,
                    Cached = false,
                    ALCResolvingHandlers = handlers.Invocations
                };
            }
        }

        [BinderTest]
        public static BindOperation ALCResolvingEvent_NameMismatch()
        {
            var assemblyName = new AssemblyName(SubdirectoryAssemblyName);
            CustomALC alc = new CustomALC(nameof(ALCResolvingEvent_NameMismatch));
            using (var handlers = new Handlers(HandlerReturn.MismatchAssembly, alc))
            {
                Assert.Throws<FileLoadException>(() => alc.LoadFromAssemblyName(assemblyName));

                Assert.AreEqual(1, handlers.Invocations.Count);
                return new BindOperation()
                {
                    AssemblyName = assemblyName,
                    AssemblyLoadContext = alc.ToString(),
                    Success = false,
                    Cached = false,
                    ALCResolvingHandlers = handlers.Invocations
                };
            }
        }

        [BinderTest]
        public static BindOperation ALCResolvingEvent_MultipleHandlers()
        {
            var assemblyName = new AssemblyName(SubdirectoryAssemblyName);
            CustomALC alc = new CustomALC(nameof(ALCResolvingEvent_NameMismatch));
            using (var handlerNull = new Handlers(HandlerReturn.Null, alc))
            using (var handlerLoad = new Handlers(HandlerReturn.RequestedAssembly, alc))
            {
                Assembly asm = alc.LoadFromAssemblyName(assemblyName);

                Assert.AreEqual(1, handlerNull.Invocations.Count);
                Assert.AreEqual(1, handlerLoad.Invocations.Count);
                return new BindOperation()
                {
                    AssemblyName = assemblyName,
                    AssemblyLoadContext = alc.ToString(),
                    Success = true,
                    ResultAssemblyName = asm.GetName(),
                    ResultAssemblyPath = asm.Location,
                    Cached = false,
                    ALCResolvingHandlers = handlerNull.Invocations.Concat(handlerLoad.Invocations).ToList()
                };
            }
        }

        private enum HandlerReturn
        {
            Null,
            RequestedAssembly,
            MismatchAssembly
        }

        private class Handlers : IDisposable
        {
            private HandlerReturn handlerReturn;
            private AssemblyLoadContext alc;

            internal readonly List<HandlerInvocation> Invocations = new List<HandlerInvocation>();

            public Handlers(HandlerReturn handlerReturn, AssemblyLoadContext alc)
            {
                this.handlerReturn = handlerReturn;
                this.alc = alc;
                this.alc.Resolving += OnALCResolving;
            }

            public void Dispose()
            {
                this.alc.Resolving -= OnALCResolving;
            }

            private Assembly OnALCResolving(AssemblyLoadContext context, AssemblyName assemblyName)
            {
                Assembly asm = null;
                if (handlerReturn != HandlerReturn.Null)
                {
                    string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string fileName = handlerReturn == HandlerReturn.RequestedAssembly ? $"{assemblyName.Name}.dll" : $"{assemblyName.Name}Mismatch.dll";
                    string assemblyPath = Path.Combine(appPath, "DependentAssemblies", fileName);
                    asm = File.Exists(assemblyPath)
                        ? context.LoadFromAssemblyPath(assemblyPath)
                        : null;
                }

                var invocation = new HandlerInvocation()
                {
                    AssemblyName = assemblyName,
                    HandlerName = nameof(OnALCResolving),
                    AssemblyLoadContext = context == AssemblyLoadContext.Default ? "Default" : context.ToString(),
                };
                if (asm != null)
                {
                    invocation.ResultAssemblyName = asm.GetName();
                    invocation.ResultAssemblyPath = asm.Location;
                }

                Invocations.Add(invocation);
                return asm;
            }
        }
    }
}
