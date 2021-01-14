using System;
using System.Collections.Generic;

namespace MugenMvvm.UnitTests.Internal.Internal
{
    public sealed class TestEqualityComparer<T> : IEqualityComparer<T>
    {
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public Func<T, T, bool>? Equals { get; set; }
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        public new Func<T, int>? GetHashCode { get; set; }

        bool IEqualityComparer<T>.Equals(T? x, T? y)
        {
            if (Equals == null)
                return object.Equals(x, y);
            return Equals(x!, y!);
        }

        int IEqualityComparer<T>.GetHashCode(T obj)
        {
            if (GetHashCode == null)
                return obj!.GetHashCode();
            return GetHashCode(obj!);
        }
    }
}