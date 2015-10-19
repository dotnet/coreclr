using System;
using System.Reflection;

/// <summary>
/// StringFormatMask [v-yishi]
/// </summary>
public class TypeAttributesStringFormatMask
{
    #region Public Methods
    public bool RunTests()
    {
        bool retVal = true;

        TestLibrary.TestFramework.LogInformation("[Positive]");
        retVal = PosTest1() && retVal;

        return retVal;
    }

    #region Positive Test Cases
    public bool PosTest1()
    {
        bool retVal = true;

        TestLibrary.TestFramework.BeginScenario("PosTest1: Verify StringFormatMask's value is 0x00030000");

        try
        {
            int expected = 0x00030000;
            int actual = (int)TypeAttributes.StringFormatMask;

            if (expected != actual)
            {
                TestLibrary.TestFramework.LogError("001.1", "StringFormatMask's value is not 0x00030000");
                TestLibrary.TestFramework.LogInformation("WARNING [LOCAL VARIABLES] expected = " + expected + ", actual = " + actual);
                retVal = false;
            }
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("001.0", "Unexpected exception: " + e);
            TestLibrary.TestFramework.LogInformation(e.StackTrace);
            retVal = false;
        }

        return retVal;
    }
    #endregion
    #endregion

    public static int Main()
    {
        TypeAttributesStringFormatMask test = new TypeAttributesStringFormatMask();

        TestLibrary.TestFramework.BeginTestCase("TypeAttributesStringFormatMask");

        if (test.RunTests())
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
}
