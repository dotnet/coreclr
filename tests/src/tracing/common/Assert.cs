using System;

namespace Tracing.Tests.Common
{
    public static class Assert
    {
        public static void Equal<T>(T left, T right) where T : IEquatable<T>
        {
            if (!left.Equals(right))
            {
                throw new Exception(
                    string.Format("Values are not equal! Left='{0}' Right='{1}'", left, right));
            }
        }

        public static void NotEqual<T>(T left, T right) where T : IEquatable<T>
        {
            if (left.Equals(right))
            {
                throw new Exception(
                    string.Format("Values are equal! Left='{0}' Right='{1}'", left, right));
            }
        }
    }
}
