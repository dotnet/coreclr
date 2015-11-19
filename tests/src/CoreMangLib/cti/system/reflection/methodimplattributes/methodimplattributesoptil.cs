using System;
using System.Reflection;

/// <summary>
/// System.Reflection.MethodAttributes.OPTIL[v-jisong]
/// </summary>
public class MethodImplAttributesOPTIL
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

        const string c_TEST_DESC = "PosTest1:check the MethodImplAttributes.OPTIL value is 0x0002...";
        const string c_TEST_ID = "P001";
        MethodImplAttributes FLAG_VALUE = (MethodImplAttributes)0x0002;

        TestLibrary.TestFramework.BeginScenario(c_TEST_DESC);

        try
        {

            if (MethodImplAttributes.OPTIL != FLAG_VALUE)
            {
                string errorDesc = "value " + FLAG_VALUE.ToString() + " is not as expected: Actual is " + MethodImplAttributes.OPTIL.ToString();
                TestLibrary.TestFramework.LogError("001" + " TestId-" + c_TEST_ID, errorDesc);
                retVal = false;
            }
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("002" + " TestId-" + c_TEST_ID, "Unexpected exception: " + e);
            retVal = false;
        }

        return retVal;
    }
    #endregion

    #endregion

    public static int Main()
    {
        MethodImplAttributesOPTIL test = new MethodImplAttributesOPTIL();

        TestLibrary.TestFramework.BeginTestCase("System.Reflection.MethodImplOPTIL");

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
