using System;
using System.Globalization;
using TestLibrary;

/// <summary>
///Clone
/// </summary>
public class CultureInfoClone
{
    public static int Main()
    {
        CultureInfoClone CultureInfoClone = new CultureInfoClone();

        TestLibrary.TestFramework.BeginTestCase("CultureInfoClone");
        if (CultureInfoClone.RunTests())
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
        if (!Utilities.IsWindows)
        {
            // Neutral cultures not supported on Windows
            retVal = PosTest2() && retVal;
            retVal = PosTest3() && retVal;
        }
        return retVal;
    }
    // Returns true if the expected result is right
    // Returns false if the expected result is wrong
    public bool PosTest1()
    {
        bool retVal = true;

        TestLibrary.TestFramework.BeginScenario("PosTest1:CultureTypes.SpecificCultures");
        try
        {

            CultureInfo myCultureInfo = new CultureInfo("fr-FR");
            CultureInfo myClone = myCultureInfo.Clone() as CultureInfo;
            if (!myClone.Equals(myCultureInfo))
            {
                TestLibrary.TestFramework.LogError("001", "Should return true.");
                retVal = false;
            }
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("002", "Unexpected exception: " + e);
            retVal = false;
        }
        return retVal;
    }
    // Returns true if the expected result is right
    // Returns false if the expected result is wrong
    public bool PosTest2()
    {
        bool retVal = true;

        TestLibrary.TestFramework.BeginScenario("PosTest2: CultureTypes.NeutralCultures");
        try
        {

            CultureInfo myCultureInfo = new CultureInfo("en");
            CultureInfo myClone = myCultureInfo.Clone() as CultureInfo;
            if (!myClone.Equals(myCultureInfo))
            {
                TestLibrary.TestFramework.LogError("003", "Should return true.");
                retVal = false;
            }
       
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("004", "Unexpected exception: " + e);
            retVal = false;
        }
        return retVal;
    }
    // Returns true if the expected result is right
    // Returns false if the expected result is wrong
    public bool PosTest3()
    {
        bool retVal = true;

        TestLibrary.TestFramework.BeginScenario("PosTest3: invariant culture");
        try
        {

            CultureInfo myTestCulture = CultureInfo.InvariantCulture;
            CultureInfo myClone = myTestCulture.Clone() as CultureInfo;
            if (!myClone.Equals(myTestCulture))
            {
                TestLibrary.TestFramework.LogError("005", "Should return true.");
                retVal = false;
            }

        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("006", "Unexpected exception: " + e);
            retVal = false;
        }
        return retVal;
    }
}

