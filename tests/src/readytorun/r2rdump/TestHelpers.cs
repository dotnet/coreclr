using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Xunit.Abstractions;
using System.Text;

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
                string index = "";
                XmlAttribute indexAttribute = node.Attributes["Index"];
                if (indexAttribute != null) {
                    index = indexAttribute.Value;
                }
                fullName = node.Name + index + "." + fullName;
                node = node.ParentNode;
            }
            return fullName;
        }

        public static XmlNodeList GetTestXmlNodes(string r2rdump, string imageFilename, bool raw, bool header, bool disasm, bool unwind, bool gc, bool sc)
        {
            disasm = false; // TODO: this requires the cordistools nuget package with the recent changes to be pushed

            StringBuilder sb = new StringBuilder();
            sb.Append($"{r2rdump} --in {imageFilename} -x");
            if (raw)
                sb.Append(" --raw");
            if (header)
                sb.Append(" --header");
            if (disasm)
                sb.Append(" -d");
            if (unwind)
                sb.Append(" --unwind");
            if (gc)
                sb.Append(" --gc");
            if (sc)
                sb.Append(" --sc");

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = sb.ToString();
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            string stdout = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return ReadXmlNodes(stdout, false);
        }

        public static XmlNodeList ReadXmlNodes(string filenameOrXmlString, bool fromFile)
        {
            XmlDocument expectedXml = new XmlDocument();
            if (fromFile)
            {
                expectedXml.Load(filenameOrXmlString);
            }
            else
            {
                expectedXml.LoadXml(filenameOrXmlString);
            }
            return expectedXml.SelectNodes("//*");
        }
    }
}
