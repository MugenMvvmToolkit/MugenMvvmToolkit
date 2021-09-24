using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MugenMvvm.Collections
{
    //note don't forget to sync with IndexMapList
    [StructLayout(LayoutKind.Auto)]
    internal struct IndexMapAwareList<T> where T : IndexMapAware
    {
        public int Size;
        public Entry[] Indexes;

        private IndexMapAwareList(bool _)
        {
            Indexes = Array.Empty<Entry>();
            Size = 0;
        }

        public static IndexMapAwareList<T> Get() => new(true);

        public readonly bool Move(int oldIndex, int newIndex, [NotNullWhen(true)] out T? value)
        {
            int? i1 = null;
            int? i2 = null;
            return Move(ref oldIndex, ref newIndex, out value, ref i1, ref i2);
        }

        public readonly bool Move(int oldIndex, int newIndex, out T? value, [NotNull] ref int? binarySearchOldIndex, [NotNull] ref int? binarySearchNewIndex) =>
            Move(ref oldIndex, ref newIndex, out value, ref binarySearchOldIndex, ref binarySearchNewIndex);

        public readonly bool Move(ref int oldIndex, ref int newIndex, [NotNullWhen(true)] out T? value)
        {
            int? i1 = null;
            int? i2 = null;
            return Move(ref oldIndex, ref newIndex, out value, ref i1, ref i2);
        }

        public readonly bool Move(ref int oldIndex, ref int newIndex, [NotNullWhen(true)] out T? value, [NotNull] ref int? binarySearchOldIndex,
            [NotNull] ref int? binarySearchNewIndex)
        {
            if (Size == 0)
            {
                binarySearchOldIndex = -1;
                binarySearchNewIndex = -1;
                value = default;
                return false;
            }

            var originalNewIndex = newIndex;
            oldIndex = binarySearchOldIndex ??= BinarySearch(oldIndex);
            newIndex = binarySearchNewIndex ??= BinarySearch(newIndex);
            if (oldIndex == newIndex)
            {
                if (oldIndex >= 0)
                    Indexes[oldIndex] = new Entry(originalNewIndex, Indexes[oldIndex].Value);
                value = default;
                return false;
            }

            bool hasItem;
            if (oldIndex < 0)
            {
                hasItem = false;
                value = default;
                oldIndex = ~oldIndex;
            }
            else
            {
                value = Indexes[oldIndex].Value;
                hasItem = true;
            }

            if (newIndex < 0)
                newIndex = ~newIndex;

            if (oldIndex < newIndex)
            {
                for (var i = oldIndex; i < newIndex; i++)
                    Indexes[i].Index -= 1;

                if (newIndex >= Size || Indexes[newIndex]._index > originalNewIndex)
                    --newIndex;
                else if (Indexes[newIndex]._index == originalNewIndex)
                    --Indexes[newIndex].Index;

                if (hasItem)
                {
                    if (newIndex != oldIndex)
                        Array.Copy(Indexes, oldIndex + 1, Indexes, oldIndex, newIndex - oldIndex);
                    Indexes[newIndex] = new Entry(originalNewIndex, value!);
                }
            }
            else
            {
                for (var i = newIndex; i < oldIndex; i++)
                    Indexes[i].Index += 1;

                if (Indexes[newIndex]._index == originalNewIndex)
                    --Indexes[newIndex].Index;

                if (hasItem)
                {
                    if (newIndex != oldIndex)
                        Array.Copy(Indexes, newIndex, Indexes, newIndex + 1, oldIndex - newIndex);
                    Indexes[newIndex] = new Entry(originalNewIndex, value!);
                }
            }

            return hasItem;
        }

        public readonly void UpdateIndexesBinary(int binarySearchIndex, int value)
        {
            if (Size == 0)
                return;
            if (binarySearchIndex < 0)
                binarySearchIndex = ~binarySearchIndex;
            for (var i = binarySearchIndex; i < Size; i++)
                Indexes[i].Index += value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRaw(int key, T value) => Insert(Size, key, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Add(int key, T value, int binarySearchIndex)
        {
            if (binarySearchIndex < 0)
                return Insert(~binarySearchIndex, key, value);
            return Insert(binarySearchIndex, key, value);
        }

        public int Add(int key, T value)
        {
            var binarySearchIndex = BinarySearch(key);
            if (binarySearchIndex >= 0)
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(key));
            return Insert(~binarySearchIndex, key, value);
        }

        public void Clear()
        {
            Array.Clear(Indexes, 0, Size);
            Size = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int BinarySearch(int key)
        {
            if (Size == 0)
                return -1;
            return Array.BinarySearch(Indexes, 0, Size, new Entry(key));
        }

        public void RemoveAt(int index)
        {
            --Size;
            if (index < Size)
                Array.Copy(Indexes, index + 1, Indexes, index, Size - index);
            Indexes[Size] = default;
        }

        public void EnsureCapacity(int min)
        {
            if (Indexes.Length < min)
            {
                var num = Indexes.Length == 0 ? 4 : Indexes.Length * 2;
                if (num < min)
                    num = min;
                SetCapacity(num);
            }
        }

        private int Insert(int index, int key, T value)
        {
            if (Size == Indexes.Length)
                EnsureCapacity(Size + 1);
            if (index < Size)
                Array.Copy(Indexes, index, Indexes, index + 1, Size - index);

            Indexes[index] = new Entry(key, value);
            ++Size;
            return index;
        }

        private void SetCapacity(int value)
        {
            if (value == Indexes.Length)
                return;
            if (value < Size)
                ExceptionManager.ThrowCapacityLessThanCollection(nameof(value));

            if (value > 0)
            {
                var keyArray = new Entry[value];
                if (Size > 0)
                    Array.Copy(Indexes, 0, keyArray, 0, Size);

                Indexes = keyArray;
            }
            else
                Indexes = Array.Empty<Entry>();
        }

        [StructLayout(LayoutKind.Auto)]
        public struct Entry : IComparable<Entry>
        {
            public int _index;
            public T Value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Entry(int index, T value)
            {
                Value = value;
                _index = index;
                value.Index = index;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Entry(int index)
            {
                _index = index;
                Value = null!;
            }

            public int Index
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => _index;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    _index = value;
                    Value.Index = value;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly int CompareTo(Entry other) => _index.CompareTo(other._index);

#if DEBUG
            public override readonly string ToString() => $"{Index} - {(Value == null ? "null" : Value.ToString())}";
#endif
        }
    }
}