// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Reflection.Emit
{
    // This is a package private class. This class hold all of the managed
    // data member for ModuleBuilder. Note that what ever data members added to
    // this class cannot be accessed from the EE.
    internal class ModuleBuilderData
    {
        internal const string MultiByteValueClass = "$ArrayType$";

        internal ModuleBuilderData(ModuleBuilder module, string moduleName)
        {
            _globalTypeBuilder = new TypeBuilder(module);
            _moduleName = moduleName;
        }

        internal string _moduleName;
        internal bool _hasGlobalBeenCreated;
        internal TypeBuilder _globalTypeBuilder;
    }
}
