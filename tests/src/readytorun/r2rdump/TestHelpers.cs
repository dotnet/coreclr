using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Xunit.Abstractions;
using R2RDump;

namespace R2RDumpTest
{
    class TestHelpers
    {
        public static bool XmlDiff(List<XmlNode> testXmlNodes, List<XmlNode> expectedXmlNodes)
        {
            testXmlNodes.RemoveAll(node => !IsLeaf(node));
            expectedXmlNodes.RemoveAll(node => !IsLeaf(node));
            Dictionary<string, XmlNode> diffExpected = testXmlNodes.Except(expectedXmlNodes, new XElementEqualityComparer()).ToDictionary(node => XmlNodeFullName(node));
            Dictionary<string, XmlNode> diffTest = expectedXmlNodes.Except(testXmlNodes, new XElementEqualityComparer()).ToDictionary(node => XmlNodeFullName(node));

            foreach (KeyValuePair<string, XmlNode> diff in diffTest)
            {
                XmlNode testNode = diff.Value;
                Console.WriteLine("Test:");
                Console.WriteLine("\t" + XmlNodeFullName(testNode) + ": " + testNode.InnerText);
                if (diffExpected.ContainsKey(diff.Key))
                {
                    XmlNode expectedNode = diffExpected[diff.Key];
                    Console.WriteLine("Expected:");
                    Console.WriteLine("\t" + XmlNodeFullName(expectedNode) + ": " + expectedNode.InnerText);
                }
                else
                {
                    Console.WriteLine("Expected:");
                    Console.WriteLine("\tnone");
                }
                Console.WriteLine("");
            }
            foreach (KeyValuePair<string, XmlNode> diff in diffExpected)
            {
                if (!diffTest.ContainsKey(diff.Key))
                {
                    Console.WriteLine("Test:");
                    Console.WriteLine("\tnone");
                    Console.WriteLine("Expected:");
                    Console.WriteLine("\t" + XmlNodeFullName(diff.Value) + ": " + diff.Value.InnerText);
                }
                Console.WriteLine("");
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
