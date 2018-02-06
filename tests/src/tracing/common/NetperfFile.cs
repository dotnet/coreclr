using System;

namespace Tracing.Tests.Common
{
    public class NetperfFile : IDisposable
    {
        public string Path { get; }
        private bool keepOutput { get; }

        private NetperfFile(string fileName, bool keep)
        {
            Path = fileName;
            keepOutput = keep;
        }

        public void Dispose()
        {
            if (keepOutput)
                Console.WriteLine("\n\tOutput file: {0}", Path);
            else
                System.IO.File.Delete(Path);
        }

        public static NetperfFile Create(string[] args)
        {
            if (args.Length >= 1)
                return new NetperfFile(args[0], true);

            return new NetperfFile(System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".netperf", false);
        }
    }
}
