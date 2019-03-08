// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Runtime.Serialization
{
    // This class duplicates a class on CoreFX. We are keeping it here -- just this one method --
    // as it was widely invoked by reflection to workaround it being missing in .NET Core 1.0
    internal static class FormatterServices
    {
        // Gets a new instance of the object.  The entire object is initalized to 0 and no 
        // constructors have been run. **THIS MEANS THAT THE OBJECT MAY NOT BE IN A STATE
        // CONSISTENT WITH ITS INTERNAL REQUIREMENTS** This method should only be used for
        // deserialization when the user intends to immediately populate all fields.  This method
        // will not create an unitialized string because it is non-sensical to create an empty
        // instance of an immutable type.
        //
        public static object GetUninitializedObject(Type type) => RuntimeHelpers.GetUninitializedObject(type);
    }
}





