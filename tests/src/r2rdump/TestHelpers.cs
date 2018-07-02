using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Xunit.Abstractions;

namespace R2RDump.Test
{
    class TestHelpers
    {
        public static bool XmlDiff(ITestOutputHelper output, List<XmlNode> testXmlNodes, List<XmlNode> expectedXmlNodes)
        {
            testXmlNodes.RemoveAll(node => !IsLeaf(node));
            expectedXmlNodes.RemoveAll(node => !IsLeaf(node));
            Dictionary<string, XmlNode> diffExpected = testXmlNodes.Except(expectedXmlNodes, new XElementEqualityComparer()).ToDictionary(node => XmlNodeFullName(node));
            Dictionary<string, XmlNode> diffTest = expectedXmlNodes.Except(testXmlNodes, new XElementEqualityComparer()).ToDictionary(node => XmlNodeFullName(node));

            foreach (KeyValuePair<string, XmlNode> diff in diffTest)
            {
                XmlNode testNode = diff.Value;
                output.WriteLine("Test:");
                output.WriteLine("\t" + XmlNodeFullName(testNode) + ": " + testNode.InnerText);
                if (diffExpected.ContainsKey(diff.Key))
                {
                    XmlNode expectedNode = diffExpected[diff.Key];
                    output.WriteLine("Expected:");
                    output.WriteLine("\t" + XmlNodeFullName(expectedNode) + ": " + expectedNode.InnerText);
                }
                else
                {
                    output.WriteLine("Expected:");
                    output.WriteLine("\tnone");
                }
                output.WriteLine("");
            }
            foreach (KeyValuePair<string, XmlNode> diff in diffExpected)
            {
                if (!diffTest.ContainsKey(diff.Key))
                {
                    output.WriteLine("Test:");
                    output.WriteLine("\tnone");
                    output.WriteLine("Expected:");
                    output.WriteLine("\t" + XmlNodeFullName(diff.Value) + ": " + diff.Value.InnerText);
                }
                output.WriteLine("");
            }

            return diffExpected.Count == 0 && diffTest.Count == 0;
        }

        private class XElementEqualityComparer : IEqualityComparer<XmlNode>
        {
            public bool Equals(XmlNode x, XmlNode y)
            {
                return x.OuterXml.Equals(y.OuterXml);
            }
            public int GetHashCode(XmlNode obj)
            {
                return 0;
            }
        }

        private static bool IsLeaf(XmlNode node)
        {
            return !node.HasChildNodes || node.FirstChild.NodeType == XmlNodeType.Text;
        }

        private static string XmlNodeFullName(XmlNode node)
        {
            string fullName = "";
            XmlNode n = node;
            while (node != null && node.NodeType != XmlNodeType.Document)
            {
                fullName = node.Name + "." + fullName;
                node = node.ParentNode;
            }
            return fullName;
        }

        public static XmlNodeList GetTestXmlNodes(string filename, bool raw, bool header, bool disasm, bool unwind, bool gc, bool sc)
        {
            R2RReader r2r = new R2RReader(filename);
            IntPtr disassembler = CoreDisTools.GetDisasm(r2r.Machine);
            XmlDumper dumper = new XmlDumper(r2r, null, raw, header, disasm, disassembler, unwind, gc, sc);
            return dumper.GetXmlDocument().SelectNodes("//*");
        }

        public static XmlNodeList GetExpectedXmlNodes(string filename)
        {
            XmlDocument expectedXml = new XmlDocument();
            expectedXml.Load(filename);
            return expectedXml.SelectNodes("//*");
        }
    }
}
