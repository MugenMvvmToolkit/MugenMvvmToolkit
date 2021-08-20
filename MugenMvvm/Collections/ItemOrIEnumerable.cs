using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;

namespace MugenMvvm.Collections
{
    public static class ItemOrIEnumerable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIEnumerable<T> FromItem<T>(T? item) where T : class? => item == null ? default : new ItemOrIEnumerable<T>(item, null, 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIEnumerable<T> FromItem<T>(T? item, bool hasItem) => hasItem ? new ItemOrIEnumerable<T>(item, null, 1) : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIEnumerable<T> FromList<T>(T[]? array)
        {
            if (array == null || array.Length == 0)
                return default;
            if (array.Length == 1)
                return new ItemOrIEnumerable<T>(array[0], null, 1);
            return new ItemOrIEnumerable<T>(default, array, array.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIEnumerable<T> FromList<T>(List<T>? list)
        {
            if (list == null)
                return default;
            return FromList<List<T>, T>(list);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIEnumerable<T> FromList<T>(IReadOnlyList<T>? readOnlyList)
        {
            if (readOnlyList == null)
                return default;
            if (readOnlyList is T[] array)
                return FromList(array);
            return FromList<IReadOnlyList<T>, T>(readOnlyList);
        }

        public static ItemOrIEnumerable<T> FromList<T>(IEnumerable<T>? enumerable)
        {
            if (enumerable == null)
                return default;
            if (enumerable is T[] array)
                return FromList(array);
            if (enumerable is List<T> list)
                return FromList<List<T>, T>(list);
            if (enumerable is IReadOnlyList<T> readOnlyList)
                return FromList<IReadOnlyList<T>, T>(readOnlyList);
            if (enumerable is IReadOnlyCollection<T> readOnlyCollection && readOnlyCollection.Count == 0)
                return default;
            return new ItemOrIEnumerable<T>(default, enumerable, 0);
        }

        [Preserve(Conditional = true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIEnumerable<T> FromRawValue<T>(object? value)
        {
            if (value == null)
                return default;
            if (value is IEnumerable<T> list)
                return FromList(list);
            return new ItemOrIEnumerable<T>((T)value, null, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ItemOrIEnumerable<T> FromList<TList, T>(TList list) where TList : IReadOnlyList<T>
        {
            var count = list.Count;
            if (count == 0)
                return default;
            if (count == 1)
                return new ItemOrIEnumerable<T>(list[0], null, 1);
            return new ItemOrIEnumerable<T>(default, list, 0);
        }
    }
}