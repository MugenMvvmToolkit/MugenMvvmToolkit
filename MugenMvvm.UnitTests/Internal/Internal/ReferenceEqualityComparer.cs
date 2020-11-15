#if !NET5_0

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MugenMvvm.UnitTests.Internal.Internal
{
    public class ReferenceEqualityComparer : IEqualityComparer<object?>
    {
        #region Fields

        public static readonly IEqualityComparer<object> Instance;

        #endregion

        #region Constructors

        static ReferenceEqualityComparer()
        {
            Instance = new ReferenceEqualityComparer();
        }

        internal ReferenceEqualityComparer()
        {
        }

        #endregion

        #region Implementation of IEqualityComparer<in object>

        bool IEqualityComparer<object?>.Equals(object? x, object? y) => ReferenceEquals(x, y);

        int IEqualityComparer<object?>.GetHashCode(object? obj)
        {
            if (obj == null)
                return 0;
            return RuntimeHelpers.GetHashCode(obj) * 397;
        }

        #endregion
    }
}

#endif