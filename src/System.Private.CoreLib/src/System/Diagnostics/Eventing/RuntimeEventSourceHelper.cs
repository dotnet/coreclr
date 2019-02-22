// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;


namespace System.Diagnostics.Tracing
{
	internal sealed class RuntimeEventSourceHelper
	{
		internal static long GetProcessTimes()
		{
			long _creation;
			long _exit;
			long _user;
			long _kernel;
			
			Interop.Kernel32.GetProcessTimes(Interop.Kernel32.GetCurrentProcess(), out _creation, out _exit, out _user, out _kernel);
			return _user + _kernel;
		}
	}
}