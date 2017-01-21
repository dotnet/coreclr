// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System.Runtime.CompilerServices;

// We need this to be able to typeforward to internal types
[assembly: InternalsVisibleTo("mscorlib, PublicKey=00240000048000009400000006020000002400005253413100040000010001008d56c76f9e8649383049f383c44be0ec204181822a6c31cf5eb7ef486944d032188ea1d3920763712ccb12d75fb77e9811149e6148e5d32fbaab37611c1878ddc19e20ef135d0cb2cff2bfec3d115810c3d9069638fe4be215dbf795861920e5ab6f7db2e2ceef136ac23d5dd2bf031700aec232f6c6b1c785b4305c123b37ab", AllInternalsVisible=false)]

// For now we are only moving to using this file over AssemblyAttributes.cspp in CoreSys, ideally we would move away from the centralized 
// AssemblyAttributes.cspp model for the other build types at a future point in time.
#if FEATURE_CORESYSTEM

// Depends on things like SuppressUnmanagedCodeAttribute and WindowsRuntimeImportAttribute
[assembly: InternalsVisibleTo("System.Runtime.WindowsRuntime, PublicKey=" + _InternalsVisibleToKeys.EcmaPublicKeyFull, AllInternalsVisible=false)]

// Depends on WindowsRuntimeImportAttribute
[assembly: InternalsVisibleTo("System.Runtime.WindowsRuntime.UI.Xaml, PublicKey=" + _InternalsVisibleToKeys.EcmaPublicKeyFull, AllInternalsVisible=false)]

// Depends on internals of Assembly and IResourceManager
[assembly: InternalsVisibleTo("System.Resources.ResourceManager, PublicKey=002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad236132102900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc093344d5ad293", AllInternalsVisible = false)]

internal class _InternalsVisibleToKeys
{
  // Token = b77a5c561934e089
  internal const string EcmaPublicKeyFull = "00000000000000000400000000000000";
}

#endif // FEATURE_CORESYS
