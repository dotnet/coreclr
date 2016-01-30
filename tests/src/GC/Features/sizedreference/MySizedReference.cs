// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/******************************
 * 
 * Helper class to avoid using reflection for SizedReference.
 * 
 * 
 * *****************************/

using System;
using System.Reflection;

public class MySizedReference
{
    private Type _sizedRefType;
    private object _sizedRefObject;
    private MethodInfo _approxSizeMeth;
    private MethodInfo _disposeMeth;

    public MySizedReference(object obj)
    {
        ConstructorInfo constructor;

        _sizedRefType = Type.GetType("System.SizedReference");
        if (_sizedRefType == null)
        {
            Console.WriteLine("Error! Can't get type");
        }
        constructor = _sizedRefType.GetConstructor(new Type[] { typeof(object) });
        _sizedRefObject = constructor.Invoke(new object[] { obj });
        _approxSizeMeth = _sizedRefType.GetMethod("get_ApproximateSize");
        _disposeMeth = _sizedRefType.GetMethod("Dispose");
    }

    public long ApproximateSize
    {
        get
        {
            GC.Collect();
            return (long)_approxSizeMeth.Invoke(_sizedRefObject, null);
        }
    }

    //same as ApproximateSize, but don't do a GC collect
    public long ApproximateSizeNC
    {
        get
        {
            return (long)_approxSizeMeth.Invoke(_sizedRefObject, null);
        }
    }

    public void Dispose()
    {
        _disposeMeth.Invoke(_sizedRefObject, null);
    }
}

