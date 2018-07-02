using System;
using Xunit;
using R2RDump;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace R2RDump.Test
{
    public class BasicTest
    {
        private readonly ITestOutputHelper _output;
        public BasicTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void NoSections()
        {
            R2RReader r2r = new R2RReader("c:/Code/r2rdump/peFiles/NoSections.ni.dll");
            Assert.Equal(0, r2r.R2RHeader.Sections.Count);
            Assert.Equal(0, r2r.R2RMethods.Count);
        }

        [Fact]
        public void Simple()
        {
            List<XmlNode> testXmlNodes = TestHelpers.GetTestXmlNodes("c:/Code/r2rdump/peFiles/HelloWorld.ni.dll", true, true, true, true, true, true).Cast<XmlNode>().ToList();
            List<XmlNode> expectedXmlNodes = TestHelpers.GetExpectedXmlNodes("c:/Code/r2rdump/out/HelloWorld.xml").Cast<XmlNode>().ToList();
            bool identical = TestHelpers.XmlDiff(_output, testXmlNodes, expectedXmlNodes);
            Assert.True(identical);
        }
    }
}
