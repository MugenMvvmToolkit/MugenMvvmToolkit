using System;
using System.Collections.Generic;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public sealed class TestEqualityComparer<T> : IEqualityComparer<T>
    {
        #region Properties

        public new Func<T, T, bool>? Equals { get; set; }

        public new Func<T, int>? GetHashCode { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IEqualityComparer<T>.Equals(T x, T y)
        {
            if (Equals == null)
                return object.Equals(x, y);
            return Equals(x, y);
        }

        int IEqualityComparer<T>.GetHashCode(T obj)
        {
            if (GetHashCode == null)
                return obj!.GetHashCode();
            return GetHashCode(obj);
        }

        #endregion
    }
}