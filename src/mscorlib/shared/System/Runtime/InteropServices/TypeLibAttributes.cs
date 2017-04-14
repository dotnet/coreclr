// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;   

namespace System.Runtime.InteropServices
{
    [AttributeUsage(AttributeTargets.Interface, Inherited = false)]
    public sealed class TypeLibImportClassAttribute : Attribute
    {
        internal String _importClassName;
        public TypeLibImportClassAttribute(Type importClass)
        {
            _importClassName = importClass.ToString();
        }
        public String Value { get { return _importClassName; } }
    }       
    
    [Serializable]
    [Flags()]
    public enum TypeLibTypeFlags
    {
        FAppObject      = 0x0001,
        FCanCreate      = 0x0002,
        FLicensed       = 0x0004,
        FPreDeclId      = 0x0008,
        FHidden         = 0x0010,
        FControl        = 0x0020,
        FDual           = 0x0040,
        FNonExtensible  = 0x0080,
        FOleAutomation  = 0x0100,
        FRestricted     = 0x0200,
        FAggregatable   = 0x0400,
        FReplaceable    = 0x0800,
        FDispatchable   = 0x1000,
        FReverseBind    = 0x2000,
    }
    
    [Serializable]
    [Flags()]
    public enum TypeLibFuncFlags
    {   
        FRestricted         = 0x0001,
        FSource             = 0x0002,
        FBindable           = 0x0004,
        FRequestEdit        = 0x0008,
        FDisplayBind        = 0x0010,
        FDefaultBind        = 0x0020,
        FHidden             = 0x0040,
        FUsesGetLastError   = 0x0080,
        FDefaultCollelem    = 0x0100,
        FUiDefault          = 0x0200,
        FNonBrowsable       = 0x0400,
        FReplaceable        = 0x0800,
        FImmediateBind      = 0x1000,
    }

    [Serializable]
    [Flags()]
    public enum TypeLibVarFlags
    {   
        FReadOnly           = 0x0001,
        FSource             = 0x0002,
        FBindable           = 0x0004,
        FRequestEdit        = 0x0008,
        FDisplayBind        = 0x0010,
        FDefaultBind        = 0x0020,
        FHidden             = 0x0040,
        FRestricted         = 0x0080,
        FDefaultCollelem    = 0x0100,
        FUiDefault          = 0x0200,
        FNonBrowsable       = 0x0400,
        FReplaceable        = 0x0800,
        FImmediateBind      = 0x1000,
    }
                     
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Struct, Inherited = false)]
    public sealed class  TypeLibTypeAttribute : Attribute
    {
        internal TypeLibTypeFlags _val;
        public TypeLibTypeAttribute(TypeLibTypeFlags flags)
        {
            _val = flags;
        }
        public TypeLibTypeAttribute(short flags)
        {
            _val = (TypeLibTypeFlags)flags;
        }
        public TypeLibTypeFlags Value { get {return _val;} }    
    }    

    [AttributeUsage(AttributeTargets.Method, Inherited = false)] 
    public sealed class TypeLibFuncAttribute : Attribute
    {
        internal TypeLibFuncFlags _val;
        public TypeLibFuncAttribute(TypeLibFuncFlags flags)
        {
            _val = flags;
        }
        public TypeLibFuncAttribute(short flags)
        {
            _val = (TypeLibFuncFlags)flags;
        }
        public TypeLibFuncFlags Value { get {return _val;} }    
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false)] 
    public sealed class TypeLibVarAttribute : Attribute
    {
        internal TypeLibVarFlags _val;
        public TypeLibVarAttribute(TypeLibVarFlags flags)
        {
            _val = flags;
        }
        public TypeLibVarAttribute(short flags)
        {
            _val = (TypeLibVarFlags)flags;
        }
        public TypeLibVarFlags Value { get {return _val;} } 
    }   

    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)] 
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class TypeLibVersionAttribute : Attribute
    {
        internal int _major;
        internal int _minor;
        
        public TypeLibVersionAttribute(int major, int minor)
        {
            _major = major;
            _minor = minor;
        }
        
        public int MajorVersion { get {return _major;} }
        public int MinorVersion { get {return _minor;} }
    }    

    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)] 
    public sealed class ImportedFromTypeLibAttribute : Attribute
    {
        internal String _val;
        public ImportedFromTypeLibAttribute(String tlbFile)
        {
            _val = tlbFile;
        }
        public String Value { get {return _val;} }
    }        

    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)] 
    public sealed class PrimaryInteropAssemblyAttribute : Attribute
    {
        internal int _major;
        internal int _minor;
        
        public PrimaryInteropAssemblyAttribute(int major, int minor)
        {
            _major = major;
            _minor = minor;
        }
        
        public int MajorVersion { get {return _major;} }
        public int MinorVersion { get {return _minor;} }
    }      
}
