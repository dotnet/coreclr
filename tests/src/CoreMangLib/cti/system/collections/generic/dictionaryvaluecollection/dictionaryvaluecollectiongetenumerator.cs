using System;
using System.Collections.Generic;
/// <summary>
/// Dictionary.ValueCollection.GetEnumerator()
/// </summary>
public class DictionaryValueCollectionGetEnumerator
{
    public static int Main()
    {
        DictionaryValueCollectionGetEnumerator valuecollectGetEnum = new DictionaryValueCollectionGetEnumerator();
        TestLibrary.TestFramework.BeginTestCase("DictionaryValueCollectionGetEnumerator");
        if (valuecollectGetEnum.RunTests())
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
        TestLibrary.TestFramework.BeginScenario("PosTest1:Invoke the method GetEnumerator in ValueCollection 1");
        try
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("str1", "Test1");
            dic.Add("str2", "Test2");
            Dictionary<string, string>.ValueCollection values = new Dictionary<string, string>.ValueCollection(dic);
            Dictionary<string, string>.ValueCollection.Enumerator enumer = values.GetEnumerator();
            Dictionary<string, string>.Enumerator dicEnumer = dic.GetEnumerator();
            while (enumer.MoveNext() && dicEnumer.MoveNext())
            {
                if (!enumer.Current.Equals(dicEnumer.Current.Value))
                {
                    TestLibrary.TestFramework.LogError("001", "the ExpecResult is not the ActualResult");
                    retVal = false;
                }
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
        TestLibrary.TestFramework.BeginScenario("PosTest2:Invoke the method GetEnumerator in ValueCollection 2");
        try
        {
            Dictionary<TestClass, TestClass> dic = new Dictionary<TestClass, TestClass>();
            TestClass TKey1 = new TestClass();
            TestClass TVal1 = new TestClass();
            TestClass TKey2 = new TestClass();
            TestClass TVal2 = new TestClass();
            dic.Add(TKey1, TVal1);
            dic.Add(TKey2, TVal2);
            Dictionary<TestClass, TestClass>.ValueCollection values = new Dictionary<TestClass, TestClass>.ValueCollection(dic);
            Dictionary<TestClass, TestClass>.ValueCollection.Enumerator enumer = values.GetEnumerator();
            Dictionary<TestClass, TestClass>.Enumerator dicEnumer = dic.GetEnumerator();
            while (enumer.MoveNext() && dicEnumer.MoveNext())
            {
                if (!enumer.Current.Equals(dicEnumer.Current.Value))
                {
                    TestLibrary.TestFramework.LogError("003", "the ExpecResult is not the ActualResult");
                    retVal = false;
                }
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
    #region ForTestClass
    public class TestClass { }
    #endregion
}