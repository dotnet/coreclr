namespace ComponentDependencyResolverTests
{
    class XPlatformUtils
    {
#if WINDOWS
        public const string NativeLibraryPrefix = "";
        public const string NativeLibrarySuffix = ".dll";
#else
        public const string NativeLibraryPrefix = "lib";
#if OSX
        public const string NativeLibrarySuffix = ".dylib";
#else
        public const string NativeLibrarySuffix = ".so";
#endif
#endif

        public static string GetStandardNativeLibraryFileName(string simpleName)
        {
            return NativeLibraryPrefix + simpleName + NativeLibrarySuffix;
        }
    }
}
