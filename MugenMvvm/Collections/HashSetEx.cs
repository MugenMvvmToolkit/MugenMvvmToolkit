using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    internal sealed class HashSetEx<T> : Dictionary<T, int>, IReadOnlyCollection<T> where T : notnull
    {
        public HashSetEx() : base(InternalEqualityComparer.GetReferenceComparer<T>())
        {
        }

        public new int Count { get; private set; }

        int IReadOnlyCollection<T>.Count => Count;

        public void Add(T item)
        {
            TryGetValue(item, out var i);
            this[item] = i + 1;
            ++Count;
        }

        public new bool Remove(T item)
        {
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
        }

        public bool Contains(T item) => ContainsKey(item);

        private IEnumerator<T> GetEnumeratorInternal()
        {
            foreach (var pair in this)
            {
                for (var i = 0; i < pair.Value; i++)
                    yield return pair.Key;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorInternal();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumeratorInternal();
    }
}