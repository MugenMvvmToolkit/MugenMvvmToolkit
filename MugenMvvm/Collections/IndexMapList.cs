using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    internal struct IndexMapList<T>
    {
        private static readonly Action<T, int>? UpdateIndex = GetUpdateIndexDelegate();
        public int Size;
        public Entry[] Indexes;

        private IndexMapList(bool _)
        {
            Indexes = Array.Empty<Entry>();
            Size = 0;
        }

        public static IndexMapList<T> Get() => new(true);

        public readonly bool Move(int oldIndex, int newIndex, out T? value)
        {
            int? i1 = null;
            int? i2 = null;
            return Move(ref oldIndex, ref newIndex, out value, ref i1, ref i2);
        }

        public readonly bool Move(int oldIndex, int newIndex, out T? value, [NotNull] ref int? binarySearchOldIndex, [NotNull] ref int? binarySearchNewIndex) =>
            Move(ref oldIndex, ref newIndex, out value, ref binarySearchOldIndex, ref binarySearchNewIndex);

        public readonly bool Move(ref int oldIndex, ref int newIndex, out T? value)
        {
            int? i1 = null;
            int? i2 = null;
            return Move(ref oldIndex, ref newIndex, out value, ref i1, ref i2);
        }

        public readonly bool Move(ref int oldIndex, ref int newIndex, out T? value, [NotNull] ref int? binarySearchOldIndex, [NotNull] ref int? binarySearchNewIndex)
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

                if (newIndex >= Size || Indexes[newIndex].Index > originalNewIndex)
                    --newIndex;
                else if (Indexes[newIndex].Index == originalNewIndex)
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

                if (Indexes[newIndex].Index == originalNewIndex)
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

        public readonly void UpdateIndexes(int index, int value)
        {
            if (Size == 0 || index > Indexes[Size - 1].Index)
                return;

            if (index == Indexes[Size - 1].Index)
            {
                Indexes[Size - 1].Index += value;
                return;
            }

            if (index == 0 || index < Indexes[0].Index)
            {
                for (var i = 0; i < Size; i++)
                    Indexes[i].Index += value;
                return;
            }

            UpdateIndexesBinary(BinarySearch(index), value);
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
            return Array.BinarySearch(Indexes, 0, Size, new Entry(key, default!));
        }

        public bool TryGetValue(int index, [NotNullWhen(true)] out T? value)
        {
            var search = BinarySearch(index);
            if (search < 0)
            {
                value = default;
                return false;
            }

            value = Indexes[search].Value!;
            return true;
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
        
        private static Action<T, int>? GetUpdateIndexDelegate()
        {
            if (typeof(T) == typeof(FlattenCollectionItemBase))
                return (Action<T, int>)(object)new Action<FlattenCollectionItemBase, int>(UpdateIndexImpl);
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UpdateIndexImpl(FlattenCollectionItemBase item, int i) => item.Index = i;

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
            public T Value;
            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Entry(int index, T value)
            {
                Value = value;
                _index = index;
                if (value != null && UpdateIndex != null)
                    UpdateIndex.Invoke(value, index);
            }

            public int Index
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => _index;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    _index = value;
                    if (UpdateIndex != null)
                        UpdateIndex.Invoke(Value, value);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CompareTo(Entry other) => Index.CompareTo(other.Index);

            public override string ToString() => $"{Index} - {Value}";
        }
    }
}