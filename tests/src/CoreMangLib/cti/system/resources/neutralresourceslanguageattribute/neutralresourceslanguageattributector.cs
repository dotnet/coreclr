// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Resources;

/// <summary>
/// ctor(System.String) [v-yishi]
/// </summary>
public class NeutralResourcesLanguageAttributeCtor
{
    #region Public Methods
    public bool RunTests()
    {
        bool retVal = true;

        TestLibrary.TestFramework.LogInformation("[Positive]");
        retVal = PosTest1() && retVal;
        retVal = PosTest2() && retVal;

        TestLibrary.TestFramework.LogInformation("[Negative]");
        retVal = NegTest1() && retVal;

        return retVal;
    }

    #region Positive Test Cases
    public bool PosTest1()
    {
        bool retVal = true;

        TestLibrary.TestFramework.BeginScenario("PosTest1: Call ctor with string.Empty reference");

        try
        {
            NeutralResourcesLanguageAttribute attr = new NeutralResourcesLanguageAttribute(string.Empty);

            if (attr == null)
            {
                TestLibrary.TestFramework.LogError("001.1", "Calling ctor with string.Empty returns null reference");
                retVal = false;
            }

            string actual = attr.CultureName;
            if (actual != string.Empty)
            {
                TestLibrary.TestFramework.LogError("001.2", "Calling ctor with string.Empty does not returns null CultureName");
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

    public bool PosTest2()
    {
        bool retVal = true;

        TestLibrary.TestFramework.BeginScenario("PosTest2: Call ctor with rand string");

        try
        {
            string expected = TestLibrary.Generator.GetString(-55, false, 1, 10);
            NeutralResourcesLanguageAttribute attr = new NeutralResourcesLanguageAttribute(expected);

            if (attr == null)
            {
                TestLibrary.TestFramework.LogError("002.1", "Calling ctor with rand string returns null reference");
                retVal = false;
            }

            string actual = attr.CultureName;
            if (actual != expected)
            {
                TestLibrary.TestFramework.LogError("002.2", "Calling ctor with rand string does not returns expected CultureName");
                TestLibrary.TestFramework.LogInformation("WARNING [LOCAL VARIABLE] actual = " + actual + ", expected = " + expected);
                retVal = false;
            }
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("002.0", "Unexpected exception: " + e);
            TestLibrary.TestFramework.LogInformation(e.StackTrace);
            retVal = false;
        }

        return retVal;
    }
    #endregion

    #region Negtive Test Cases
    public bool NegTest1()
    {
        bool retVal = true;

        TestLibrary.TestFramework.BeginScenario("NegTest1: Call ctor with null reference");

        try
        {
            NeutralResourcesLanguageAttribute attr = new NeutralResourcesLanguageAttribute(null);
            TestLibrary.TestFramework.LogError("101.1", "ArgumentNullException exception does not thrown when calling ctor with null reference");
            retVal = false;
        }
        catch ( ArgumentNullException )
        {
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("101.0", "Unexpected exception: " + e);
            TestLibrary.TestFramework.LogInformation(e.StackTrace);
            retVal = false;
        }

        return retVal;
    }
    #endregion
    #endregion

    public static int Main()
    {
        NeutralResourcesLanguageAttributeCtor test = new NeutralResourcesLanguageAttributeCtor();

        TestLibrary.TestFramework.BeginTestCase("NeutralResourcesLanguageAttributeCtor");

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
