using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Threading;

class Test
{
	[DllImport("libcoreclr")]
	extern static int GetCurrentThreadId();

	static void Collector()
	{
		try {
			while(true) GC.Collect();
		} catch { }
	}

	public static int Main()
	{
		//Debugger.Break();
		var t = new Thread(Collector);
		t.Start();

		for(int i = 0; i < 25; i++)
		{
			var a = GetCurrentThreadId();
			Console.WriteLine(a);
		}

		t.Abort();

		return 100;
	}
}
