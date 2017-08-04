using System;
using System.Threading.Tasks;
using System.IO;

using EventPipe;

namespace eventpipe_onoff
{
    class EventPipeSmoke
    {
        private static int allocIterations = 10000;
        private static int trivialSize = 0x100000;
        private static bool keepOutput = false;

        static int Main(string[] args)
        {
            string outputFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".netperf";
            TraceConfiguration config = CreateConfiguration(outputFilename);

            Console.WriteLine("\tStart: Enable tracing.");
            TraceControl.Enable(config);
            Console.WriteLine("\tEnd: Enable tracing.\n");

            Console.WriteLine("\tStart: Allocation.");
            // Allocate for allocIterations iterations.
            for(int i=0; i<allocIterations; i++)
            {
                GC.KeepAlive(new object());
            }
            Console.WriteLine("\tEnd: Allocation.\n");

            Console.WriteLine("\tStart: Disable tracing.");
            TraceControl.Disable();
            Console.WriteLine("\tEnd: Disable tracing.\n");

            FileInfo outputMeta = new FileInfo(outputFilename);
            Console.WriteLine("\tCreated {0} bytes of data", outputMeta.Length);

            bool pass = false;
            if (outputMeta.Length > trivialSize){
                pass = true;
            }

            if (keepOutput)
            {
                Console.WriteLine(String.Format("\tOutput file: {0}", outputFilename));
            }
            else
            {
                System.IO.File.Delete(outputFilename);
            }

            return pass ? 100 : 0;
        }

        private static TraceConfiguration CreateConfiguration(string outputFilename)
        {
            // Setup the configuration values.
            uint circularBufferMB = 1024; // 1 GB
            uint level = 5; // Verbose

            // Create a new instance of EventPipeConfiguration.
            TraceConfiguration config = new TraceConfiguration(outputFilename, circularBufferMB);
            // Setup the provider values.
            // Public provider.
            string providerName = "e13c0d23-ccbc-4e12-931b-d9cc2eee27e4";
            UInt64 keywords = 0x4c14fccbd;

            // Enable the provider.
            config.EnableProvider(providerName, keywords, level);

            // Private provider.
            providerName = "763fd754-7086-4dfe-95eb-c01a46faf4ca";
            keywords = 0x4002000b;

            // Enable the provider.
            config.EnableProvider(providerName, keywords, level);

            // Sample profiler.
            providerName = "3c530d44-97ae-513a-1e6d-783e8f8e03a9";
            keywords = 0x0;

            // Enable the provider.
            config.EnableProvider(providerName, keywords, level);

            return config;
        }
    }
}
