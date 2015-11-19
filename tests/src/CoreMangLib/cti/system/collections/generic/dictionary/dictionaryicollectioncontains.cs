using System;
using System.Collections.Generic;

/// <summary>
/// System.Collections.Generic.ICollection&lt;System.Collections.Generic.KeyValuePair&lt;TKey,TValue&gt;&gt;.Contains(System.Collections.Generic.KeyValuePair&lt;TKey,TValue&gt;)
/// </summary>

public class DictionaryICollectionContains
{
    #region Public Methods
    public bool RunTests()
    {
        bool retVal = true;

        TestLibrary.TestFramework.LogInformation("[Positive]");
        retVal = PosTest1() && retVal;
        retVal = PosTest2() && retVal;
        
        //
        // TODO: Add your negative test cases here
        //
        // TestLibrary.TestFramework.LogInformation("[Negative]");
        // retVal = NegTest1() && retVal;

        return retVal;
    }

    #region Positive Test Cases
    public bool PosTest1()
    {
        bool retVal = true;

        // Add your scenario description here
        TestLibrary.TestFramework.BeginScenario("PosTest1: Verify method ICollectionContains when k/v existed.");

        try
        {
            ICollection<KeyValuePair<String, String>> dictionary = new Dictionary<String, String>();

            KeyValuePair<string, string> kvp1 = new KeyValuePair<String, String>("txt", "notepad.exe");
            KeyValuePair<string, string> kvp2 = new KeyValuePair<String, String>("bmp", "paint.exe");
            KeyValuePair<string, string> kvp3 = new KeyValuePair<String, String>("dib", "paint.exe");
            KeyValuePair<string, string> kvp4 = new KeyValuePair<String, String>("rtf", "wordpad.exe");

            dictionary.Add(kvp1);
            dictionary.Add(kvp2);
            dictionary.Add(kvp3);
            dictionary.Add(kvp4);

            bool actual  = dictionary.Contains(kvp1) &&
                           dictionary.Contains(kvp1) &&
                           dictionary.Contains(kvp1) &&
                           dictionary.Contains(kvp1);
            bool expected = true;

            if (actual != expected)
            {
                TestLibrary.TestFramework.LogError("001.1", "Method ICollectionContains Err.");
                TestLibrary.TestFramework.LogInformation("WARNING [LOCAL VARIABLE] actual = " + actual + ", expected = " + expected);
                retVal = false;
            }

        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("001.2", "Unexpected exception: " + e);
            TestLibrary.TestFramework.LogInformation(e.StackTrace);
            retVal = false;
        }

        return retVal;
    }

    public bool PosTest2()
    {
        bool retVal = true;

        // Add your scenario description here
        TestLibrary.TestFramework.BeginScenario("PosTest2: Verify method ICollectionContains when no k/v existed .");

        try
        {
            ICollection<KeyValuePair<String, String>> dictionary = new Dictionary<String, String>();

            KeyValuePair<string, string> kvp1 = new KeyValuePair<String, String>("txt", "notepad.exe");

            bool actual = dictionary.Contains(kvp1);
            bool expected = false;

            if (actual != expected)
            {
                TestLibrary.TestFramework.LogError("002.1", "Method ICollectionContains Err.");
                TestLibrary.TestFramework.LogInformation("WARNING [LOCAL VARIABLE] actual = " + actual + ", expected = " + expected);
                retVal = false;
            }

        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("002.2", "Unexpected exception: " + e);
            TestLibrary.TestFramework.LogInformation(e.StackTrace);
            retVal = false;
        }

        return retVal;
    }
    #endregion

    #region Nagetive Test Cases
    //public bool NegTest1()
    //{
    //    bool retVal = true;

    //    TestLibrary.TestFramework.BeginScenario("NegTest1: ");

    //    try
    //    {
    //          //
    //          // Add your test logic here
    //          //
    //    }
    //    catch (Exception e)
    //    {
    //        TestLibrary.TestFramework.LogError("101", "Unexpected exception: " + e);
    //        TestLibrary.TestFramework.LogInformation(e.StackTrace);
    //        retVal = false;
    //    }

    //    return retVal;
    //}
    #endregion
    #endregion

    public static int Main()
    {
        DictionaryICollectionContains test = new DictionaryICollectionContains();

        TestLibrary.TestFramework.BeginTestCase("DictionaryICollectionContains");

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
