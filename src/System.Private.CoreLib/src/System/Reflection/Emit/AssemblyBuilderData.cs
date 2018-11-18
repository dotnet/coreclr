// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;

namespace System.Reflection.Emit
{
    /// <summary>
    /// This is a package private class. This class hold all of the managed
    /// data member for AssemblyBuilder. Note that what ever data members added to
    /// this class cannot be accessed from the EE.
    /// </summary>
    internal class AssemblyBuilderData
    {
        internal AssemblyBuilderData(InternalAssemblyBuilder assembly, string assemblyName, AssemblyBuilderAccess access)
        {
            _assembly = assembly;
            _assemblyName = assemblyName;
            _access = access;
            _moduleBuilderList = new List<ModuleBuilder>();
            _resWriterList = new List<ResWriterData>();

            _peFileKind = PEFileKinds.Dll;
        }
        
        /// <summary>
        /// Helper to add a dynamic module into the tracking list.
        /// </summary>
        internal void AddModule(ModuleBuilder dynModule) => _moduleBuilderList.Add(dynModule);

        /// <summary>
        /// Helper to track CAs to persist onto disk.
        /// </summary>
        internal void AddCustomAttribute(CustomAttributeBuilder customBuilder)
        {
            // make sure we have room for this CA
            if (_customAttributeBuilders == null)
            {
                _customAttributeBuilders = new CustomAttributeBuilder[InitialSize];
            }
            if (_numberOfCustomAttributeBuilders == _customAttributeBuilders.Length)
            {
                CustomAttributeBuilder[] tempCABuilders = new CustomAttributeBuilder[_numberOfCustomAttributeBuilders * 2];
                Array.Copy(_customAttributeBuilders, 0, tempCABuilders, 0, _numberOfCustomAttributeBuilders);
                _customAttributeBuilders = tempCABuilders;
            }
            _customAttributeBuilders[_numberOfCustomAttributeBuilders] = customBuilder;

            _numberOfCustomAttributeBuilders++;
        }
        
        /// <summary>
        /// Helper to track CAs to persist onto disk.
        /// </summary>
        internal void AddCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
        {
            // make sure we have room for this CA
            if (_customAttributeBytes == null)
            {
                _customAttributeBytes = new byte[InitialSize][];
                _customAttributeConstructors = new ConstructorInfo[InitialSize];
            }
            if (_iCAs == _customAttributeBytes.Length)
            {
                // enlarge the arrays
                byte[][] temp = new byte[_iCAs * 2][];
                ConstructorInfo[] tempCon = new ConstructorInfo[_iCAs * 2];
                for (int i = 0; i < _iCAs; i++)
                {
                    temp[i] = _customAttributeBytes[i];
                    tempCon[i] = _customAttributeConstructors[i];
                }
                _customAttributeBytes = temp;
                _customAttributeConstructors = tempCon;
            }

            byte[] attrs = new byte[binaryAttribute.Length];
            Buffer.BlockCopy(binaryAttribute, 0, attrs, 0, binaryAttribute.Length);
            _customAttributeBytes[_iCAs] = attrs;
            _customAttributeConstructors[_iCAs] = con;
            _iCAs++;
        }
        
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

            // Right now dynamic modules can only be added to dynamic assemblies in which
            // all modules are dynamic. Otherwise we would also need to check loaded types.
            // We only need to make this test for non-nested types since any
            // duplicates in nested types will be caught at the top level.
            //      if (enclosingType == null && _assembly.GetType(strTypeName, false, false) != null)
            //      {
            //          // Cannot have two types with the same name
            //          throw new ArgumentException(SR.Argument_DuplicateTypeName);
            //      }
        }

        internal List<ModuleBuilder> _moduleBuilderList;
        internal List<ResWriterData> _resWriterList;
        internal string _assemblyName;
        internal AssemblyBuilderAccess _access;
        private readonly InternalAssemblyBuilder _assembly;

        internal Type[] _publicComTypeList;
        internal int _iPublicComTypeCount;

        internal const int InitialSize = 16;

        // hard coding the assembly def token
        internal const int AssemblyDefToken = 0x20000001;

        // tracking AssemblyDef's CAs for persistence to disk
        internal CustomAttributeBuilder[] _customAttributeBuilders;
        internal int _numberOfCustomAttributeBuilders;
        internal byte[][] _customAttributeBytes;
        internal ConstructorInfo[] _customAttributeConstructors;
        internal int _iCAs;
        internal PEFileKinds _peFileKind;           // assembly file kind
        internal MethodInfo _entryPointMethod;
        internal Assembly _iSymWrapperAssembly;

        // For unmanaged resources
        internal string _strResourceFileName;
        internal NativeVersionInfo _nativeVersion;
        internal bool _hasUnmanagedVersionInfo;
        internal bool _overrideUnmanagedVersionInfo;
    }

    /// <summary>
    /// Internal structure to track the list of ResourceWriter for
    /// AssemblyBuilder & ModuleBuilder.
    /// </summary>
    internal class ResWriterData
    {
        internal string _name;
        internal string _fileName;
        internal string _fullFileName;
        internal Stream _memoryStream;
        internal ResWriterData _nextResWriter;
        internal ResourceAttributes _attribute;
    }

    internal class NativeVersionInfo
    {
        internal string _description;
        internal string _company;
        internal string _title;
        internal string _copyright;
        internal string _trademark;
        internal string _product;
        internal string _productVersion;
        internal string _fileVersion;
        internal int _lcid = -1;
    }
}
