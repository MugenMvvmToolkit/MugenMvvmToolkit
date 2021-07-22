using System.Collections.Generic;

namespace MugenMvvm.Internal
{
    internal sealed class WrapperObjectComparer<T> : IComparer<object?>
    {
        private readonly IComparer<T> _comparer;

        public WrapperObjectComparer(IComparer<T> comparer)
        {
            Should.NotBeNull(comparer, nameof(comparer));
            _comparer = comparer;
        }

        public int Compare(object? x, object? y) => _comparer.Compare((T) x!, (T) y!);
    }
}