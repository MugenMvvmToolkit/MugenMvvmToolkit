#if !NET5_0
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    public class ReferenceEqualityComparer : IEqualityComparer<object?>
    {
        public static readonly IEqualityComparer<object> Instance;

        static ReferenceEqualityComparer()
        {
            Instance = new ReferenceEqualityComparer();
        }

        internal ReferenceEqualityComparer()
        {
        }

        bool IEqualityComparer<object?>.Equals(object? x, object? y) => ReferenceEquals(x, y);

        int IEqualityComparer<object?>.GetHashCode(object? obj)
        {
            if (obj == null)
                return 0;
            return RuntimeHelpers.GetHashCode(obj) * 397;
        }

    }
}

#endif