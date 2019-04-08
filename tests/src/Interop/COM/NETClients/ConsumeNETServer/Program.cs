// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NetClient
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    using TestLibrary;
    using Server.Contract;

    using CoClass = Server.Contract.Servers;

    class Program
    {
        static void Validate_Activation()
        {
            Console.WriteLine($"{nameof(Validate_Activation)}...");

            var consumeNETServerTesting = new CoClass.ConsumeNETServerTesting();

            // The CoClass should be the activated type, _not_ the activation interface.
            Assert.AreEqual(consumeNETServerTesting.GetType(), typeof(CoClass.ConsumeNETServerTestingClass));
        }

        static void Validate_CCW_Wasnt_Unwrapped()
        {
            Console.WriteLine($"{nameof(Validate_CCW_Wasnt_Unwrapped)}...");

            var consumeNETServerTesting = new CoClass.ConsumeNETServerTesting();

            // The CoClass should be the activated type, _not_ the implementation class.
            // This indicates the real implementation class is wrapped in its CCW and exposed
            // to the runtime as an RCW.
            Assert.AreNotEqual(consumeNETServerTesting.GetType(), typeof(ConsumeNETServerTesting));
        }

        static int Main(string[] doNotUse)
        {
            // RegFree COM is not supported on Windows Nano
            if (Utilities.IsWindowsNanoServer)
            {
                return 100;
            }

            // Initialize CoreShim and hostpolicymock
            HostPolicyMock.Initialize(Environment.CurrentDirectory, null);
            Environment.SetEnvironmentVariable("CORESHIM_COMACT_ASSEMBLYNAME", "NETServer");
            Environment.SetEnvironmentVariable("CORESHIM_COMACT_TYPENAME", "ConsumeNETServerTesting");

            try
            {
                using (HostPolicyMock.Mock_corehost_resolve_component_dependencies(
                    0,
                    string.Empty,
                    string.Empty,
                    string.Empty))
                {
                    Validate_Activation();
                    Validate_CCW_Wasnt_Unwrapped();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Test Failure: {e}");
                return 101;
            }

            return 100;
        }
    }
}
