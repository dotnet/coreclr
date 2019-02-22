// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.IO;


namespace System.Diagnostics.Tracing
{
	internal sealed class RuntimeEventSourceHelper
	{
		internal static long GetProcessTimes()
		{
            long pid = Environment.GetCurrentProcessId();
            using (FileStream fs = new FileStream($"/proc/{pid}/stat", FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1))
            {
                byte[] text = ReadAllBytesUnknownLength(fs);
                string s = System.Text.Encoding.UTF8.GetString(text, 0, text.Length);
                long _user = Convert.ToInt64(s.Split(" ")[13]);
                long _system = Convert.ToInt64(s.Split(" ")[14]);
                return _user + _system;
            }
		}

        private static byte[] ReadAllBytesUnknownLength(FileStream fs)
        {
            const int MaxByteArrayLength = 0x2000; // Unlikely that we'll see more than this from procfs
            byte[] rentedArray = null;
            Span<byte> buffer = stackalloc byte[512];
            try
            {
                int bytesRead = 0;
                while (true)
                {
                    if (bytesRead == buffer.Length)
                    {
                        uint newLength = (uint)buffer.Length * 2;
                        if (newLength > MaxByteArrayLength)
                        {
                            newLength = (uint)Math.Max(MaxByteArrayLength, buffer.Length + 1);
                        }

                        byte[] tmp = ArrayPool<byte>.Shared.Rent((int)newLength);
                        buffer.CopyTo(tmp);
                        if (rentedArray != null)
                        {
                            ArrayPool<byte>.Shared.Return(rentedArray);
                        }
                        buffer = rentedArray = tmp;
                    }

                    Debug.Assert(bytesRead < buffer.Length);
                    int n = fs.Read(buffer.Slice(bytesRead));
                    if (n == 0)
                    {
                        return buffer.Slice(0, bytesRead).ToArray();
                    }
                    bytesRead += n;
                }
            }
            finally
            {
                if (rentedArray != null)
                {
                    ArrayPool<byte>.Shared.Return(rentedArray);
                }
            }
        }
	}
}
