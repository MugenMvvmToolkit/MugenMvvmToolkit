using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    internal struct IndexMapList<T>
    {
        public int[] Keys;
        public int Size;
        public T[] Values;

        private IndexMapList(bool _)
        {
            Keys = Array.Empty<int>();
            Values = Array.Empty<T>();
            Size = 0;
        }

        public static IndexMapList<T> Get() => new(true);

        public readonly void UpdateIndexes(int index, int value)
        {
            if (Size == 0 || index > Keys[Size - 1])
                return;

            if (index == Keys[Size - 1])
            {
                Keys[Size - 1] += value;
                return;
            }

            if (index == 0 || index < Keys[0])
            {
                for (var i = 0; i < Size; i++)
                    Keys[i] += value;
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
                Keys[i] += value;
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
            Array.Clear(Keys, 0, Size);
            Array.Clear(Values, 0, Size);
            Size = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int BinarySearch(int key)
        {
            if (Size == 0)
                return -1;
            return Array.BinarySearch(Keys, 0, Size, key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOfKey(int key)
        {
            if (Size == 0)
                return -1;
            var num = Array.BinarySearch(Keys, 0, Size, key);
            if (num < 0)
                return -1;
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T GetValue(int index)
        {
            if (index >= Size)
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

            return Values[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void SetValue(int index, T value)
        {
            if (index >= Size)
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

            Values[index] = value;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Size)
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

            --Size;
            if (index < Size)
            {
                Array.Copy(Keys, index + 1, Keys, index, Size - index);
                Array.Copy(Values, index + 1, Values, index, Size - index);
            }

            Keys[Size] = default;
            Values[Size] = default!;
        }

        public void EnsureCapacity(int min)
        {
            if (Keys.Length < min)
            {
                var num = Keys.Length == 0 ? 4 : Keys.Length * 2;
                if (num < min)
                    num = min;
                SetCapacity(num);
            }
        }

        private int Insert(int index, int key, T value)
        {
            if (Size == Keys.Length)
                EnsureCapacity(Size + 1);
            if (index < Size)
            {
                Array.Copy(Keys, index, Keys, index + 1, Size - index);
                Array.Copy(Values, index, Values, index + 1, Size - index);
            }

            Keys[index] = key;
            Values[index] = value;
            ++Size;
            return index;
        }

        private void SetCapacity(int value)
        {
            if (value == Keys.Length)
                return;
            if (value < Size)
                ExceptionManager.ThrowCapacityLessThanCollection(nameof(value));

            if (value > 0)
            {
                var keyArray = new int[value];
                var objArray = new T[value];
                if (Size > 0)
                {
                    Array.Copy(Keys, 0, keyArray, 0, Size);
                    Array.Copy(Values, 0, objArray, 0, Size);
                }

                Keys = keyArray;
                Values = objArray;
            }
            else
            {
                Keys = Array.Empty<int>();
                Values = Array.Empty<T>();
            }
        }
    }
}