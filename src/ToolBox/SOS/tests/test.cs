using System;

class Test
{
    static void DumpClass()
    {
        Console.WriteLine("test dumpclass");
    }

    static void DumpIL()
    {
        Console.WriteLine("test dumpil");
    }

    static void DumpMD()
    {
        Console.WriteLine("test dumpmd");
    }

    static void DumpModule()
    {
        Console.WriteLine("test dumpmodule");
    }

    static void DumpObject()
    {
        Console.WriteLine("test dumpobject");
    }

    static void DumpStackObjects()
    {
        Console.WriteLine("test dso");
    }

    static void Name2EE()
    {
        Console.WriteLine("test name2ee");
    }


    static int Main()
    {
        DumpIL();
        DumpModule();

        return 0;
    }
}
