using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Globalization;
using OrderInfoList = System.Collections.Generic.List<JitOrderParser.OrderInfo>;

namespace JitOrderParser {
    struct OrderInfo
    {
        public string filename;
        public UInt32 mdToken;
        public int profileCount;
        public string region;
        public UInt32 methodHash;
        public bool hasEH;
        public string frameKind;
        public bool hasLoop;
        public int normalCallCount;
        public int indirectCallCount;
        public int basicBlockCount;
        public int localVarCount;
        public int assertionCount;
        public int cseCount;
        public double perfScore;
        public int IL_bytes;
        public int nativeHotBytes;
        public int nativeColdBytes;
        public string methodName;

        private const char SeperatorChar = '|';
        private const char BlankChar = ' ';

        public OrderInfo(string line, string filenameArg)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            char[] separators = { SeperatorChar };
            string[] splits = line.Split(separators, StringSplitOptions.None);
            int splitCount = splits.Length;

            if ((splitCount < 17) || (splitCount > 18))
            {
                throw new ArgumentException("invalid input");
            }

            filename = filenameArg; 

            int curIndex = 0;

            if (!UInt32.TryParse(splits[curIndex++], NumberStyles.HexNumber, provider, out mdToken))
            {
                mdToken = 0;
            }

            if (!Int32.TryParse(splits[curIndex++], out profileCount))
            {
                profileCount = -1;
            }

            region = splits[curIndex++].Trim(BlankChar);

            if (!UInt32.TryParse(splits[curIndex++], NumberStyles.HexNumber, provider, out methodHash))
            {
                methodHash = 0;
            }

            hasEH = (splits[curIndex++].Trim(BlankChar) == "EH");

            frameKind = splits[curIndex++].Trim(BlankChar);
            
            hasLoop = (splits[curIndex++].Trim(BlankChar) == "LOOP");

            if (!Int32.TryParse(splits[curIndex++], out normalCallCount))
            {
                normalCallCount = -1;
            }

            if (!Int32.TryParse(splits[curIndex++], out indirectCallCount))
            {
                indirectCallCount = -1;
            }

            if (!Int32.TryParse(splits[curIndex++], out basicBlockCount))
            {
                basicBlockCount = -1;
            }

            if (!Int32.TryParse(splits[curIndex++], out localVarCount))
            {
                localVarCount = -1;
            }

            if (splitCount == 18)
            {
                if (!Int32.TryParse(splits[curIndex++], out assertionCount))
                {
                    assertionCount = -1;
                }
                if (!Int32.TryParse(splits[curIndex++], out cseCount))
                {
                    cseCount = -1;
                }
            }
            else
            {
                assertionCount = -1;
                cseCount = -1;
                curIndex++;
            }

            if (!Double.TryParse(splits[curIndex++], out perfScore))
            {
                perfScore = -1.0;
            }
            if (!Int32.TryParse(splits[curIndex++], out IL_bytes))
            {
                IL_bytes = -1;
            }

            if (!Int32.TryParse(splits[curIndex++], out nativeHotBytes))
            {
                nativeHotBytes = -1;
            }

            if (!Int32.TryParse(splits[curIndex++], out nativeColdBytes))
            {
                nativeColdBytes = -1;
            }

            methodName = splits[curIndex++];
        }

        static public bool isValidLine(string line)
        {
            int seperatorCount = 0;
            foreach (char currentChar in line)
            {
                if (currentChar == SeperatorChar)
                {
                    seperatorCount++;
                }
            }
            if ((seperatorCount == 17) || (seperatorCount == 16))
            {
                CultureInfo provider = CultureInfo.InvariantCulture;
                string firstSplit = line.Substring(0, 8);

                UInt32 token = 0;
                if (UInt32.TryParse(firstSplit, NumberStyles.HexNumber, provider, out token))
                {
                    return true;
                }
            }
            return false;
        }
    };

    class Program
    {
        static string s_firstFile = null;
        static List<string> s_OtherFiles = null;
        static Dictionary<UInt32, OrderInfo> s_rootMap;
        static Dictionary<UInt32, OrderInfoList> s_othersMap;

        static void ParseOrderFile(string filename, bool isRoot)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    if (OrderInfo.isValidLine(line))
                    {
                        OrderInfo currOrderInfo = new OrderInfo(line, filename);
                        UInt32 key = currOrderInfo.methodHash;
                        if (isRoot)
                        {
                            OrderInfo existingItem;
                            if (s_rootMap.TryGetValue(key, out existingItem))
                            {
                                Console.WriteLine("Duplicate methodHash: {0:x8} {1}", key, currOrderInfo.methodName);
                            }
                            else
                            {
                                s_rootMap.Add(key, currOrderInfo);
                            }
                        }
                        else
                        {
                            OrderInfoList currList = null;
                            if (!s_othersMap.TryGetValue(key, out currList))
                            {
                                currList = new OrderInfoList();
                                currList.Add(currOrderInfo);
                                s_othersMap.Add(currOrderInfo.methodHash, currList);
                            }
                            else
                            {
                                Console.WriteLine("Duplicate methodHash: {0:x8} {1}", key, currOrderInfo.methodName);
                                currList.Add(currOrderInfo);
                                s_othersMap[key] = currList;
                            }                            
                        }
                    }
                }
            }
        }

        static void ProduceReport()
        {
            foreach (OrderInfo currRootOrderInfo in s_rootMap.Values)
            {
                UInt32 key = currRootOrderInfo.methodHash;

                OrderInfoList currList = null;
                if (!s_othersMap.TryGetValue(key, out currList))
                {
                    Console.WriteLine("FAIL: No match for Root methodHash: {0:x8} :: {1}", 
                        currRootOrderInfo.methodHash, currRootOrderInfo.methodName);
                }
                else
                {
                    if (currList.Count > 1)
                    {                    
                        Console.WriteLine("FAIL: Multiple matches ({2}) for Root methodHash: {0:x8} :: {1}", 
                            currRootOrderInfo.methodHash, currRootOrderInfo.methodName, currList.Count);
                    }
                    else
                    {
                        // Note there is only one matching value, but using a foreach is the easiest way to retrieve it.
                        foreach (OrderInfo matchOrderInfo in currList)
                        {
                            double diffScore = matchOrderInfo.perfScore - currRootOrderInfo.perfScore;
                            int codeSizeDiff = matchOrderInfo.nativeHotBytes - currRootOrderInfo.nativeHotBytes;

                            if (Math.Abs(diffScore) < 0.015)
                            {
                                if (codeSizeDiff == 0)
                                {
                                    Console.WriteLine("SAME: Root PerfScore of {2,7:F2} was the same as the other PerfScore for {0:x8} :: {1}",
                                        currRootOrderInfo.methodHash, currRootOrderInfo.methodName, currRootOrderInfo.perfScore);
                                }
                                else if (Math.Abs(codeSizeDiff) < 6)
                                {
                                    Console.WriteLine("CLOSE: Root PerfScore was the same PerfScore of {2,7:F2} but had code size diff of {3} for {0:x8} :: {1}",
                                        currRootOrderInfo.methodHash, currRootOrderInfo.methodName, matchOrderInfo.perfScore, codeSizeDiff);
                                }
                                else
                                {
                                    Console.WriteLine("DIFF: Same PerfScore of {2,7:F2} but had code size diff of {3} for {0:x8} :: {1}",
                                        currRootOrderInfo.methodHash, currRootOrderInfo.methodName, currRootOrderInfo.perfScore, codeSizeDiff);
                                }
                            }
                            else
                            {
                                double pctScore = diffScore / currRootOrderInfo.perfScore;
                                // Is it within 2%
                                if ((Math.Abs(pctScore) < 0.02) && (Math.Abs(codeSizeDiff) < 6))
                                {
                                    Console.WriteLine("CLOSE: Root PerfScore was differrent by {2,5:F2}% than the orig PerfScore of {3,7:F2}  for {0:x8} :: {1}",
                                        currRootOrderInfo.methodHash, currRootOrderInfo.methodName, pctScore*100.0, matchOrderInfo.perfScore);
                                }
                                else
                                {
                                    Console.WriteLine("DIFF: Root PerfScore was differrent by {2,7:F2} than the other PerfScore for {0:x8} :: {1}",
                                        currRootOrderInfo.methodHash, currRootOrderInfo.methodName, diffScore);
                                }
                            }
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            int len = args.Length;
            bool invalidArgs = false;
            bool firstArg = true;

            s_OtherFiles = new List<string>(args.Length-1);

            foreach (string currentArg in args)
            {
                if (currentArg[1] == '-')
                {
                    // Parse options
                    Console.WriteLine("Unknown option: " + currentArg);
                    invalidArgs = true;
                    break;
                }
                if (!File.Exists(currentArg))
                {
                    Console.WriteLine("Unable to access file: " + currentArg);
                    invalidArgs = true;
                    break;
                }
                if (firstArg)
                {
                    s_firstFile = currentArg;
                    firstArg = false;
                }
                else
                {
                    s_OtherFiles.Add(currentArg);
                }
            }

            if (!invalidArgs)
            {
                s_rootMap = new Dictionary<UInt32, OrderInfo>();
                s_othersMap = new Dictionary<UInt32, OrderInfoList>();

                Console.WriteLine("Parsing (root) : " + s_firstFile);

                ParseOrderFile(s_firstFile, true);

                foreach(string otherFilename in s_OtherFiles)
                {
                    Console.WriteLine("Parsing (non-root) : " + otherFilename);
                    ParseOrderFile(otherFilename, false);
                }

                ProduceReport();
            }
        }
    }
}
