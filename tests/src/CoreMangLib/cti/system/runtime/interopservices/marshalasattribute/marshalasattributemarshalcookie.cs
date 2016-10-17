// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Runtime.InteropServices;
/// <summary>
/// MarshalAsAttribute.MarshalCookie [v-minch]
/// </summary>
public class MarshalAsAttributeMarshalCookie
{
    public static int Main()
    {
        MarshalAsAttributeMarshalCookie test = new MarshalAsAttributeMarshalCookie();
        TestLibrary.TestFramework.BeginTestCase("MarshalAsAttribute.MarshalCookie");
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
    public bool RunTests()
    {
        bool retVal = true;
        TestLibrary.TestFramework.LogInformation("[Positive]");
        retVal = PosTest1() && retVal;
        retVal = PosTest2() && retVal;
        return retVal;
    }
    #region PositiveTest
    public bool PosTest1()
    {
        bool retVal = true;
        TestLibrary.TestFramework.BeginScenario("PosTest1: Return the field MarshalCookie in MarshalAsAttribute class 1");
        try
        {
            short unmanagedType = Int16.MaxValue;
            MarshalAsAttribute myMarshalAsAttribute = new MarshalAsAttribute(unmanagedType);
            string myMarshalCookie = myMarshalAsAttribute.MarshalCookie;
            if (myMarshalCookie != null)
            {
                TestLibrary.TestFramework.LogError("001", "the ExpectResult is null but the ActualResult is " + myMarshalCookie );
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
        TestLibrary.TestFramework.BeginScenario("PosTest2: Return the field MarshalCookie in MarshalAsAttribute class 2");
        try
        {
            MarshalAsAttribute myMarshalAsAttribute = new MarshalAsAttribute(UnmanagedType.ByValArray | UnmanagedType.LPArray);
            string myMarshalCookie = myMarshalAsAttribute.MarshalCookie;
            if (myMarshalCookie != null)
            {
                TestLibrary.TestFramework.LogError("003", "the ExpectResult is null but the ActualResult is " + myMarshalCookie );
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
    #endregion

}