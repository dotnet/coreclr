using System;
using System.Collections.Generic;
/// <summary>
///Ctor
/// </summary>
public class NotSupportedExceptionCtor1
{
    public static int Main()
    {
        NotSupportedExceptionCtor1 NotSupportedExceptionCtor1 = new NotSupportedExceptionCtor1();

        TestLibrary.TestFramework.BeginTestCase("NotSupportedExceptionCtor1");
        if (NotSupportedExceptionCtor1.RunTests())
        {
            TestLibrary.TestFramework.EndTestCase();
            TestLibrary.TestFramework.LogInformation("PASS");
            return 100;
        }
        else
        {
            TestLibrary.TestFramework.EndTestCase();
            TestLibrary.TestFramework.LogInformation("FAIL");
            return 0;
        }
    }
    public bool RunTests()
    {
        bool retVal = true;
        TestLibrary.TestFramework.LogInformation("[Positive]");
        retVal = PosTest1() && retVal;
        return retVal;
    }
    // Returns true if the expected result is right
    // Returns false if the expected result is wrong
    public bool PosTest1()
    {
        bool retVal = true;
        TestLibrary.TestFramework.BeginScenario("PosTest1: Create a new instance of NotSupportedException.");
        try
        {
            NotSupportedException myException = new NotSupportedException();
            if (myException == null)
            {
                TestLibrary.TestFramework.LogError("001.1", "NotSupportedException instance can not create correctly.");
                retVal = false;
            }
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("001.0", "Unexpected exception: " + e);
            retVal = false;
        }
        return retVal;
    }
}
