// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("// Licensed to the .NET Foundation under one or more agreements.");
        Console.WriteLine("// The .NET Foundation licenses this file to you under the MIT license.");
        Console.WriteLine("// See the LICENSE file in the project root for more information.");
        Console.WriteLine();

        Console.WriteLine("#include \"stdafx.h\"");

        Console.WriteLine();
        Console.WriteLine("//");
        Console.WriteLine("// THIS FILE IS GENERATED. DO NOT HAND EDIT.");
        Console.WriteLine("//");
        Console.WriteLine();

        Console.WriteLine("static const wint_t InvariantUnicodeLowerCaseData[] = {");

        string sourceFileName = args[0];

        var runs = new List<(int RunStart, int RunLength)>();
        int runStart = 0;
        int runLength = -1;
        int previousCode = 0;

        using (StreamReader sourceFile = File.OpenText(sourceFileName))
            while (sourceFile.ReadLine() is string line)
            {
                var fields = line.Split(';');

                var code = int.Parse(fields[0], NumberStyles.HexNumber);
                bool hasLowerCaseMapping = fields[13].Length != 0;
                bool isSkippedCode = code != previousCode + 1;
                previousCode = code;

                if (!hasLowerCaseMapping || isSkippedCode)
                {
                    if (runLength > 0)
                        runs.Add((runStart, runLength));

                    runLength = -1;
                }

                if (!hasLowerCaseMapping)
                    continue;

                // These won't fit in 16 bits - no point carrying them
                if (code > 0xFFFF)
                    continue;

                var lowerCaseMapping = int.Parse(fields[13], NumberStyles.HexNumber);

                if (runLength == -1)
                {
                    runStart = code;
                    runLength = 0;
                }

                Console.WriteLine($"  0x{lowerCaseMapping:X},");
                runLength++;
            }

        Console.WriteLine("};");

        if (runLength > 0)
            runs.Add((runStart, runLength));

        Console.WriteLine();

        Console.WriteLine("wint_t towlowerinvariant(wint_t c)");
        Console.WriteLine("{");

        int arrayOffset = 0;
        foreach (var run in runs)
        {
            Console.WriteLine($"  if (c < 0x{run.RunStart:X})");
            Console.WriteLine($"      return c;");
            Console.WriteLine($"  if (c < 0x{run.RunStart + run.RunLength:X})");
            Console.WriteLine($"      return InvariantUnicodeLowerCaseData[c - 0x{run.RunStart:X} + 0x{arrayOffset:X}];");
            arrayOffset += run.RunLength;
        }
        Console.WriteLine($"  return c;");
        Console.WriteLine("}");
    }
}
