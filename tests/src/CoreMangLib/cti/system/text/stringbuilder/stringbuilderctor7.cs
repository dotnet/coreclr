using System;
using System.Text;
/// <summary>
/// StringBuilder.ctor(StringBuilder)
/// </summary>
public class StringBuilderctor7
{
    public static int Main()
    {
        StringBuilderctor7 sbctor7 = new StringBuilderctor7();
        TestLibrary.TestFramework.BeginTestCase("StringBuilderctor7");
        if (sbctor7.RunTests())
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
    #region PositiveTest
    public bool PosTest1()
    {
        bool retVal = true;
        TestLibrary.TestFramework.BeginScenario("PosTest1:Construct the StringBuilder with null StringBuilder");
        try
        {
            StringBuilder strValue = null;
            StringBuilder sb = new StringBuilder(strValue);
            if (sb == null || sb.Length != 0 || sb.Capacity != 16)
            {
                TestLibrary.TestFramework.LogError("001", "The ExpectResult is not the ActualResult");
                retVal = false;
            }
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("002", "Unexpect exception:" + e);
            retVal = false;
        }
        return retVal;
    }
    public bool PosTest2()
    {
        bool retVal = true;
        TestLibrary.TestFramework.BeginScenario("PosTest2:Construct the StringBuilder with empty StringBuilder");
        try
        {
            StringBuilder strValue = new StringBuilder();
            StringBuilder sb = new StringBuilder(strValue);
            if (sb == null || sb.Length != 0 || sb.Capacity != 16)
            {
                TestLibrary.TestFramework.LogError("003", "The ExpectResult is not the ActualResult");
                retVal = false;
            }
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("004", "Unexpect exception:" + e);
            retVal = false;
        }
        return retVal;
    }
    public bool PosTest3()
    {
        bool retVal = true;
        TestLibrary.TestFramework.BeginScenario("PosTest3:Construct the StringBuilder with random string StringBuilder");
        try
        {
            string strValue = TestLibrary.Generator.GetString(-55, false, 8, 256);
            StringBuilder sb = new StringBuilder(strValue);
            if (sb == null || sb.Length != strValue.Length)
            {
                TestLibrary.TestFramework.LogError("005", "The string ExpectResult is not the ActualResult");
                retVal = false;
            }
            
            StringBuilder sb2 = new StringBuilder(sb);
            if (sb2 == null || sb2.Length != sb.Length)
            {
                TestLibrary.TestFramework.LogError("006", "The StringBuilder ExpectResult is not the ActualResult");
                retVal = false;
            }
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("007", "Unexpect exception:" + e);
            retVal = false;
        }
        return retVal;
    }
    #endregion
}
