using System;
using Xunit;
using R2RDump;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace R2RDumpTest
{
    public class BasicTest
    {
		static int Main(string[] args)
		{
			Console.WriteLine("Starting the test");
			
			NoSections();
			Simple();
			
			Console.WriteLine("PASSED");
			return 100;
		}

        static void NoSections()
        {
            R2RReader r2r = new R2RReader("c:/Code/r2rdump/peFiles/NoSections.ni.dll");
            Assert.Equal(0, r2r.R2RHeader.Sections.Count);
            Assert.Equal(0, r2r.R2RMethods.Count);
        }

        static void Simple()
        {
            List<XmlNode> testXmlNodes = TestHelpers.GetTestXmlNodes("c:/Code/r2rdump/peFiles/HelloWorld.ni.dll", true, true, true, true, true, true).Cast<XmlNode>().ToList();
            List<XmlNode> expectedXmlNodes = TestHelpers.GetExpectedXmlNodes("c:/Code/r2rdump/out/HelloWorld.xml").Cast<XmlNode>().ToList();
            bool identical = TestHelpers.XmlDiff(testXmlNodes, expectedXmlNodes);
            Assert.True(identical);
        }
    }
}
