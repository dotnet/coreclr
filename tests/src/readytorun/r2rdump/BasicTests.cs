using System;
using Xunit;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace R2RDumpTest
{
    public class BasicTests
    {
		static int Main(string[] args)
		{
			Console.WriteLine("Starting the test");
			
			TestNoSections();
			TestHelloWorld();
			
			Console.WriteLine("PASSED");
			return 100;
		}

		static void TestNoSections()
        {
            /*R2RReader r2r = new R2RReader("c:/Code/r2rdump/peFiles/NoSections.ni.dll");
            Assert.Equal(0, r2r.R2RHeader.Sections.Count);
            Assert.Equal(0, r2r.R2RMethods.Count);*/
        }
		
        static void TestHelloWorld()
        {
            List<XmlNode> testXmlNodes = TestHelpers.GetTestXmlNodes("R2RDump.dll", "HelloWorld.ni.dll", true, false, true, true, true, true).Cast<XmlNode>().ToList();
            List<XmlNode> expectedXmlNodes = TestHelpers.ReadXmlNodes("HelloWorld.xml", true).Cast<XmlNode>().ToList();
            bool identical = TestHelpers.XmlDiff(testXmlNodes, expectedXmlNodes);
            Assert.True(identical);
        }
    }
}
