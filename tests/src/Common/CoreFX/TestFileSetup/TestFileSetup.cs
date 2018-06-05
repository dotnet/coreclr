// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace CoreFX.TestUtils.TestFileSetup
{
    /// <summary>
    /// Defines the set of flags that represent exit codes
    /// </summary>
    [Flags]
    public enum ExitCode : int
    {
        Success = 0,
        HttpError = 1,
        IOError = 2,
        JsonSchemaValidationError = 3,
        UnknownError = 10

    }

    /// <summary>
    /// This helper class is used to fetch CoreFX tests from a specified URL, unarchive them and create a flat directory structure
    /// through which to iterate.
    /// </summary>
    public static class TestFileSetup
    {
        private static HttpClient httpClient;
        private static bool cleanTestBuild = false;

        private static string outputDir;
        private static string testUrl;
        private static string testListPath;

        public static void Main(string[] args)
        {
            ExitCode exitCode = ExitCode.UnknownError;
            ArgumentSyntax argSyntax = ParseCommandLine(args);

            try
            {
                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                if (cleanTestBuild)
                {
                    CleanBuild(outputDir);
                }

                // Map test names to their definitions
                Dictionary<string, XUnitTestAssembly> testAssemblyDefinitions = DeserializeTestJson(testListPath);

                SetupTests(testUrl, outputDir, testAssemblyDefinitions).Wait();
                exitCode = ExitCode.Success;
            }

            catch (AggregateException e)
            {
                e.Handle(innerExc =>
                {

                    if (innerExc is HttpRequestException)
                    {
                        exitCode = ExitCode.HttpError;
                        Console.WriteLine("Error downloading tests from: " + testUrl);
                        Console.WriteLine(innerExc.Message);
                        return true;
                    }
                    else if (innerExc is IOException)
                    {
                        exitCode = ExitCode.IOError;
                        Console.WriteLine(innerExc.Message);
                        return true;
                    }
                    else if (innerExc is JSchemaValidationException || innerExc is JsonSerializationException)
                    {
                        exitCode = ExitCode.JsonSchemaValidationError;
                        Console.WriteLine("Error validating test list: ");
                        Console.WriteLine(innerExc.Message);
                        return true;
                    }
                    return false;
                });
            }

            Environment.Exit((int)exitCode);
        }

        private static ArgumentSyntax ParseCommandLine(string[] args)
        {
            ArgumentSyntax argSyntax = ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.DefineOption("out|outDir|outputDirectory", ref outputDir, "Directory where tests are downloaded");
                syntax.DefineOption("testUrl", ref testUrl, "URL, pointing to the list of tests");
                syntax.DefineOption("testListJsonPath", ref testListPath, "JSON-formatted list of test assembly names to download");
                syntax.DefineOption("clean|cleanOutputDir", ref cleanTestBuild, "Clean test assembly output directory");
            });

            return argSyntax;
        }

        private static Dictionary<string, XUnitTestAssembly> DeserializeTestJson(string testDefinitionFilePath)
        {
            JSchemaGenerator jsonGenerator = new JSchemaGenerator();

            JSchema testDefinitionSchema = jsonGenerator.Generate(typeof(IList<XUnitTestAssembly>));
            IList<XUnitTestAssembly> testAssemblies = new List<XUnitTestAssembly>();

            IList<string> validationMessages = new List<string>();

            using (var sr = new StreamReader(testDefinitionFilePath))
            using (var jsonReader = new JsonTextReader(sr))
            using (var jsonValidationReader = new JSchemaValidatingReader(jsonReader))
            {
                // Create schema validator
                jsonValidationReader.Schema = testDefinitionSchema;
                jsonValidationReader.ValidationEventHandler += (o, a) => validationMessages.Add(a.Message);

                // Deserialize json test assembly definitions
                JsonSerializer serializer = new JsonSerializer();
                try
                {
                    testAssemblies = serializer.Deserialize<List<XUnitTestAssembly>>(jsonValidationReader);
                }
                catch (JsonSerializationException ex)
                {
                    throw new AggregateException(ex);
                }
            }

            // TODO - ABORT AND WARN
            if (validationMessages.Count != 0)
            {
                StringBuilder aggregateExceptionMessage = new StringBuilder();
                foreach (string validationMessage in validationMessages)
                {
                    aggregateExceptionMessage.Append("JSON Validation Error: ");
                    aggregateExceptionMessage.Append(validationMessage);
                    aggregateExceptionMessage.AppendLine();
                }

                throw new AggregateException(new JSchemaValidationException(aggregateExceptionMessage.ToString()));

            }

            var nameToTestAssemblyDef = new Dictionary<string, XUnitTestAssembly>();

            // Map test names to their definitions
            foreach (XUnitTestAssembly assembly in testAssemblies)
            {
                nameToTestAssemblyDef.Add(assembly.Name, assembly);
            }

            return nameToTestAssemblyDef;
        }

        private static async Task SetupTests(string jsonUrl, string destinationDirectory, Dictionary<string, XUnitTestAssembly> testDefinitions = null, bool runAllTests = false)
        {
            Debug.Assert(Directory.Exists(destinationDirectory));
            Debug.Assert(runAllTests || testDefinitions != null);

            string tempDirPath = Path.Combine(destinationDirectory, "temp");
            if (!Directory.Exists(tempDirPath))
            {
                Directory.CreateDirectory(tempDirPath);
            }
            Dictionary<string, XUnitTestAssembly> testPayloads = await GetTestUrls(jsonUrl, testDefinitions, runAllTests);

            if (testPayloads == null)
            {
                return;
            }

            await GetTestArchives(testPayloads, tempDirPath);
            ExpandArchivesInDirectory(tempDirPath, destinationDirectory);

            RSPGenerator rspGenerator = new RSPGenerator();
            foreach (XUnitTestAssembly assembly in testDefinitions.Values)
            {
                rspGenerator.GenerateRSPFile(assembly, Path.Combine(destinationDirectory, assembly.Name));
            }

            Directory.Delete(tempDirPath);
        }

        private static async Task<Dictionary<string, XUnitTestAssembly>> GetTestUrls(string jsonUrl, Dictionary<string, XUnitTestAssembly> testDefinitions = null, bool runAllTests = false)
        {
            if (httpClient is null)
            {
                httpClient = new HttpClient();
            }

            Debug.Assert(runAllTests || testDefinitions != null);
            // Set up the json stream reader
            using (var responseStream = await httpClient.GetStreamAsync(jsonUrl))
            using (var streamReader = new StreamReader(responseStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                // Manual parsing - we only need to key-value pairs from each object and this avoids deserializing all of the work items into objects
                string markedTestName = string.Empty;
                string currentPropertyName = string.Empty;

                while (jsonReader.Read())
                {
                    if (jsonReader.Value != null)
                    {
                        switch (jsonReader.TokenType)
                        {
                            case JsonToken.PropertyName:
                                currentPropertyName = jsonReader.Value.ToString();
                                break;
                            case JsonToken.String:
                                if (currentPropertyName.Equals("WorkItemId"))
                                {
                                    string currentTestName = jsonReader.Value.ToString();

                                    if (runAllTests || testDefinitions.ContainsKey(currentTestName))
                                    {
                                        markedTestName = currentTestName;
                                    }
                                }
                                else if (currentPropertyName.Equals("PayloadUri") && markedTestName != string.Empty)
                                {
                                    if (!testDefinitions.ContainsKey(markedTestName))
                                    {
                                        testDefinitions[markedTestName] = new XUnitTestAssembly() { Name = markedTestName };
                                    }
                                    testDefinitions[markedTestName].Url = jsonReader.Value.ToString();
                                    markedTestName = string.Empty;
                                }
                                break;
                        }
                    }
                }

            }
            return testDefinitions;
        }

        private static async Task GetTestArchives(Dictionary<string, XUnitTestAssembly> testPayloads, string downloadDir)
        {
            if (httpClient is null)
            {
                httpClient = new HttpClient();
            }

            foreach (string testName in testPayloads.Keys)
            {
                string payloadUri = testPayloads[testName].Url;

                if (!Uri.IsWellFormedUriString(payloadUri, UriKind.Absolute))
                    continue;

                using (var response = await httpClient.GetStreamAsync(payloadUri))
                {
                    if (response.CanRead)
                    {
                        // Create the test setup directory if it doesn't exist
                        if (!Directory.Exists(downloadDir))
                        {
                            Directory.CreateDirectory(downloadDir);
                        }

                        // CoreFX test archives are output as .zip regardless of platform
                        string archivePath = Path.Combine(downloadDir, testName + ".zip");

                        // Copy to a temp folder 
                        using (FileStream file = new FileStream(archivePath, FileMode.Create))
                        {
                            await response.CopyToAsync(file);
                        }

                    }
                }
            }
        }

        private static void ExpandArchivesInDirectory(string archiveDirectory, string destinationDirectory, bool cleanup = true)
        {
            Debug.Assert(Directory.Exists(archiveDirectory));
            Debug.Assert(Directory.Exists(destinationDirectory));

            string[] archives = Directory.GetFiles(archiveDirectory, "*.zip", SearchOption.TopDirectoryOnly);

            foreach (string archivePath in archives)
            {
                string destinationDirName = Path.Combine(destinationDirectory, Path.GetFileNameWithoutExtension(archivePath));

                ZipFile.ExtractToDirectory(archivePath, destinationDirName);


                // Delete archives
                if (cleanup)
                {
                    File.Delete(archivePath);
                }
            }
        }

        private static void CleanBuild(string directoryToClean)
        {
            Debug.Assert(Directory.Exists(directoryToClean));
            DirectoryInfo dirInfo = new DirectoryInfo(directoryToClean);

            foreach (FileInfo file in dirInfo.EnumerateFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in dirInfo.EnumerateDirectories())
            {
                dir.Delete(true);
            }
        }

    }
}
