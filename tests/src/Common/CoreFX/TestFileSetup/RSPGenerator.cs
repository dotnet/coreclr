using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CoreFX.TestUtils.TestFileSetup
{
    public class RSPGenerator
    {
        public void GenerateRSPFile(XUnitTestAssembly testDefinition, string outputPath)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            string rspFilePath = Path.Combine(outputPath, testDefinition.Name + ".rsp");

            if (File.Exists(rspFilePath))
                File.Delete(rspFilePath);


            using (StreamWriter sr = File.CreateText(rspFilePath))
            {
                if (testDefinition.Exclusions == null)
                    return;

                // Method exclusions
                if (testDefinition.Exclusions.Methods != null)
                {
                    foreach (Exclusion exclusion in testDefinition.Exclusions.Methods)
                    {
                        if (String.IsNullOrWhiteSpace(exclusion.Name))
                            continue;
                        sr.Write("-skipmethod ");
                        sr.Write(exclusion.Name);
                        sr.WriteLine();
                    }
                }

                // Class exclusions
                if (testDefinition.Exclusions.Classes != null)
                {
                    foreach (Exclusion exclusion in testDefinition.Exclusions.Classes)
                    {
                        if (String.IsNullOrWhiteSpace(exclusion.Name))
                            continue;
                        sr.Write("-skipclass ");
                        sr.Write(exclusion.Name);
                        sr.WriteLine();
                    }

                }

                // Namespace exclusions
                if (testDefinition.Exclusions.Namespaces != null)
                {
                    foreach (Exclusion exclusion in testDefinition.Exclusions.Namespaces)
                    {
                        if (String.IsNullOrWhiteSpace(exclusion.Name))
                            continue;
                        sr.Write("-skipnamespace ");
                        sr.Write(exclusion.Name);
                        sr.WriteLine();
                    }
                }
            }
        }
    }
}
