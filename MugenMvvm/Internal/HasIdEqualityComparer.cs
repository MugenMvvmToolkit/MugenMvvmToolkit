using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal
{
    public sealed class HasIdEqualityComparer<TValue> : IEqualityComparer<IHasId<TValue>>
        where TValue : notnull
    {
        public static readonly HasIdEqualityComparer<TValue> Instance = new();

        private HasIdEqualityComparer()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(IHasId<TValue>? x, IHasId<TValue>? y) =>
            ReferenceEquals(x, y) || !ReferenceEquals(x, null) && !ReferenceEquals(y, null) && EqualityComparer<TValue>.Default.Equals(x.Id, y.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(IHasId<TValue>? obj) => obj == null ? 0 : obj.Id.GetHashCode();
    }
}