using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;

namespace MugenMvvm.Collections
{
    public static class ItemOrIReadOnlyCollection
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyCollection<T> FromItem<T>(T? item) where T : class? => item == null ? default : new ItemOrIReadOnlyCollection<T>(item, null, 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyCollection<T> FromItem<T>(T? item, bool hasItem) => hasItem ? new ItemOrIReadOnlyCollection<T>(item, null, 1) : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyCollection<T> FromList<T>(T[]? array)
        {
            if (array == null || array.Length == 0)
                return default;
            if (array.Length == 1)
                return new ItemOrIReadOnlyCollection<T>(array[0], null, 1);
            return new ItemOrIReadOnlyCollection<T>(default, array, array.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyCollection<T> FromList<T>(List<T>? list)
        {
            if (list == null)
                return default;
            return FromList<List<T>, T>(list);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyCollection<T> FromList<T>(IReadOnlyList<T>? readOnlyList)
        {
            if (readOnlyList == null)
                return default;
            if (readOnlyList is T[] array)
                return FromList(array);
            return FromList<IReadOnlyList<T>, T>(readOnlyList);
        }

        public static ItemOrIReadOnlyCollection<T> FromList<T>(IReadOnlyCollection<T>? readOnlyCollection)
        {
            if (readOnlyCollection == null)
                return default;
            if (readOnlyCollection is T[] array)
                return FromList(array);
            if (readOnlyCollection is List<T> list)
                return FromList<List<T>, T>(list);
            if (readOnlyCollection is IReadOnlyList<T> readOnlyList)
                return FromList<IReadOnlyList<T>, T>(readOnlyList);

            if (readOnlyCollection.Count == 0)
                return default;
            return new ItemOrIReadOnlyCollection<T>(default, readOnlyCollection, 0);
        }

        [Preserve(Conditional = true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyCollection<T> FromRawValue<T>(object? value)
        {
            if (value == null)
                return default;
            if (value is IReadOnlyCollection<T> list)
                return FromList(list);
            return new ItemOrIReadOnlyCollection<T>((T)value, null, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ItemOrIReadOnlyCollection<T> FromList<TList, T>(TList list) where TList : IReadOnlyList<T>
        {
            var count = list.Count;
            if (count == 0)
                return default;
            if (count == 1)
                return new ItemOrIReadOnlyCollection<T>(list[0], null, 1);
            return new ItemOrIReadOnlyCollection<T>(default, list, 0);
        }
    }
}