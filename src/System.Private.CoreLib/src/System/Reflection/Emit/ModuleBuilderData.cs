// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;

namespace System.Reflection.Emit
{
    // This is a package private class. This class hold all of the managed
    // data member for ModuleBuilder. Note that what ever data members added to
    // this class cannot be accessed from the EE.
    internal class ModuleBuilderData
    {
        internal const string MultiByteValueClass = "$ArrayType$";

        internal ModuleBuilderData(ModuleBuilder module, string moduleName, string strFileName, int tkFile)
        {
            _globalTypeBuilder = new TypeBuilder(module);
            _module = module;
            _tkFile = tkFile;

            InitNames(moduleName, strFileName);
        }

        // Initialize module and file names.
        private void InitNames(string moduleName, string fileName)
        {
            _moduleName = moduleName;
            if (fileName == null)
            {
                // fake a transient module file name
                _fileName = moduleName;
            }
            else
            {
                string strExtension = Path.GetExtension(fileName);
                if (strExtension == null || strExtension == string.Empty)
                {
                    // This is required by our loader. It cannot load module file that does not have file extension.
                    throw new ArgumentException(SR.Format(SR.Argument_NoModuleFileExtension, fileName));
                }

                _fileName = fileName;
            }
        }

        internal string _moduleName;     // scope name (can be different from file name)
        internal string _fileName;
        internal bool _hasGlobalBeenCreated;
        internal bool _hasGlobalMethod;
        internal TypeBuilder _globalTypeBuilder;
        internal ModuleBuilder _module;

        private readonly int _tkFile;
        internal string _strResourceFileName;
        internal NativeVersionInfo _nativeVersion;
        internal byte[] _resourceBytes;
    }
}
