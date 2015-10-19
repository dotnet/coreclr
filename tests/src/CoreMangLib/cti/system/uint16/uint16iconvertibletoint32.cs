using System;

/// <summary>
/// UInt16.System.IConvertible.ToInt32(IFormatPrvoider)
/// </summary>
public class UInt16IConvertibleToInt32
{
    public static int Main()
    {
        UInt16IConvertibleToInt32 testObj = new UInt16IConvertibleToInt32();

        TestLibrary.TestFramework.BeginTestCase("for method: UInt16.System.IConvertible.ToInt32(IFormatProvider)");
        if(testObj.RunTests())
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
        retVal = PosTest2() && retVal;
        retVal = PosTest3() && retVal;

        return retVal;
    }

    #region Positive tests
    public bool PosTest1()
    {
        bool retVal = true;
        string errorDesc;

        UInt16 uintA;
        Int32 expectedValue;
        Int32 actualValue;
        IConvertible convert;

        uintA = (UInt16)(TestLibrary.Generator.GetInt32(-55) % (UInt16.MaxValue + 1));

        TestLibrary.TestFramework.BeginScenario("PosTest1: Random UInt16 value between 0 and UInt16.MaxValue.");
        try
        {
            convert = (IConvertible)uintA;

            actualValue = convert.ToInt32(null);
            expectedValue = (Int32)uintA;

            if (actualValue != expectedValue)
            {
                errorDesc = 
                    string.Format("The char value of {0} is not the value \\u{1:x} as expected: actual(\\u{2:x})", 
                    uintA, (int)expectedValue, (int)actualValue);
                TestLibrary.TestFramework.LogError("001", errorDesc);
                retVal = false;
            }
        }
        catch (Exception e)
        {
            errorDesc = "Unexpect exception:" + e;
            TestLibrary.TestFramework.LogError("002", errorDesc);
            retVal = false;
        }
        return retVal;
    }

    public bool PosTest2()
    {
        bool retVal = true;
        string errorDesc;

        UInt16 uintA;
        Int32 expectedValue;
        Int32 actualValue;
        IConvertible convert;

        uintA = UInt16.MaxValue;

        TestLibrary.TestFramework.BeginScenario("PosTest2: Value is UInt16.MaxValue.");
        try
        {
            convert = (IConvertible)uintA;

            actualValue = convert.ToInt32(null);
            expectedValue = (Int32)uintA;

            if (actualValue != expectedValue)
            {
                errorDesc =
                    string.Format("The char value of {0} is not the value \\u{1:x} as expected: actual(\\u{2:x})",
                    uintA, (int)expectedValue, (int)actualValue);
                TestLibrary.TestFramework.LogError("003", errorDesc);
                retVal = false;
            }
        }
        catch (Exception e)
        {
            errorDesc = "Unexpect exception:" + e;
            TestLibrary.TestFramework.LogError("004", errorDesc);
            retVal = false;
        }
        return retVal;
    }

    public bool PosTest3()
    {
        bool retVal = true;
        string errorDesc;

        UInt16 uintA;
        Int32 expectedValue;
        Int32 actualValue;
        IConvertible convert;

        uintA = UInt16.MinValue;

        TestLibrary.TestFramework.BeginScenario("PosTest3: Value is UInt16.MinValue.");
        try
        {
            convert = (Int32)uintA;

            actualValue = convert.ToInt32(null);
            expectedValue = (char)uintA;

            if (actualValue != expectedValue)
            {
                errorDesc =
                    string.Format("The char value of {0} is not the value \\u{1:x} as expected: actual(\\u{2:x})",
                    uintA, (int)expectedValue, (int)actualValue);
                TestLibrary.TestFramework.LogError("005", errorDesc);
                retVal = false;
            }
        }
        catch (Exception e)
        {
            errorDesc = "Unexpect exception:" + e;
            TestLibrary.TestFramework.LogError("006", errorDesc);
            retVal = false;
        }
        return retVal;
    }
    #endregion
}

