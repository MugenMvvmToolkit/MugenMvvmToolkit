using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
#pragma warning disable 8714
    internal sealed class HashSetEx<T> : Dictionary<T, int>, IReadOnlyCollection<T>
#pragma warning restore 8714
    {
        private int _nullCount;

        public HashSetEx(IEqualityComparer<T>? comparer) : base(comparer)
        {
        }

        public new int Count { get; private set; }

        int IReadOnlyCollection<T>.Count => Count;

        public void Add(T item)
        {
            if (item == null)
                _nullCount++;
            else
            {
                TryGetValue(item, out var i);
                this[item] = i + 1;
            }

            ++Count;
        }

        public new bool Remove(T item)
        {
            if (item == null)
            {
                if (_nullCount == 0)
                    return false;

                --_nullCount;
                --Count;
                return true;
            }

            if (!TryGetValue(item, out var i))
                return false;
            if (i == 1)
                base.Remove(item);
            else
                this[item] = i - 1;
            --Count;
            return true;
        }

        public new void Clear()
        {
            base.Clear();
            Count = 0;
            _nullCount = 0;
        }

        public bool Contains(T item)
        {
            if (item == null)
                return _nullCount > 0;
            return ContainsKey(item);
        }

        private IEnumerator<T> GetEnumeratorInternal()
        {
            foreach (var pair in this)
            {
                for (var i = 0; i < pair.Value; i++)
                    yield return pair.Key;
            }

            for (int i = 0; i < _nullCount; i++)
                yield return default!;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorInternal();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumeratorInternal();
    }
}