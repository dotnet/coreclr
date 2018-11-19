// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace System.Reflection.Emit
{
    /// <summary>
    /// This is a package private class. This class hold all of the managed
    /// data member for AssemblyBuilder. Note that what ever data members added to
    /// this class cannot be accessed from the EE.
    /// </summary>
    internal class AssemblyBuilderData
    {
        internal AssemblyBuilderData(InternalAssemblyBuilder assembly, AssemblyBuilderAccess access)
        {
            _assembly = assembly;
            _access = access;
            _moduleBuilderList = new List<ModuleBuilder>();
        }
        
        /// <summary>
        /// Helper to add a dynamic module into the tracking list.
        /// </summary>
        internal void AddModule(ModuleBuilder dynModule) => _moduleBuilderList.Add(dynModule);
        
        /// <summary>
        /// Helper to ensure the type name is unique underneath assemblyBuilder.
        /// </summary>
        internal void CheckTypeNameConflict(string strTypeName, TypeBuilder enclosingType)
        {
            for (int i = 0; i < _moduleBuilderList.Count; i++)
            {
                ModuleBuilder curModule = _moduleBuilderList[i];
                curModule.CheckTypeNameConflict(strTypeName, enclosingType);
            }
        }

        internal List<ModuleBuilder> _moduleBuilderList;
        internal AssemblyBuilderAccess _access;
        private readonly InternalAssemblyBuilder _assembly;

        internal const int InitialSize = 16;

        // hard coding the assembly def token
        internal const int AssemblyDefToken = 0x20000001;

        // tracking AssemblyDef's CAs for persistence to disk
        internal MethodInfo _entryPointMethod;
    }
}
