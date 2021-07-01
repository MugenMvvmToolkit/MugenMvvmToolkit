using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;

namespace MugenMvvm.Collections
{
    public static class ItemOrIReadOnlyList
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyList<T> FromItem<T>(T? item) where T : class? => item == null ? default : new ItemOrIReadOnlyList<T>(item, null, 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyList<T> FromItem<T>(T? item, bool hasItem) => hasItem ? new ItemOrIReadOnlyList<T>(item, null, 1) : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyList<T> FromList<T>(T[]? array)
        {
            if (array == null || array.Length == 0)
                return default;
            if (array.Length == 1)
                return new ItemOrIReadOnlyList<T>(array[0], null, 1);
            return new ItemOrIReadOnlyList<T>(default, array, array.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyList<T> FromList<T>(List<T>? list) => FromList<List<T>, T>(list);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyList<T> FromList<T>(IReadOnlyList<T>? readOnlyList)
        {
            if (readOnlyList is T[] array)
                return FromList(array);
            return FromList<IReadOnlyList<T>, T>(readOnlyList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyList<T> FromList<TList, T>(TList? list) where TList : IReadOnlyList<T>
        {
            if (list == null || list.Count == 0)
                return default;
            if (list.Count == 1)
                return new ItemOrIReadOnlyList<T>(list[0], null, 1);
            return new ItemOrIReadOnlyList<T>(default, list, 0);
        }

        [Preserve(Conditional = true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyList<T> FromRawValue<T>(object? value)
        {
            if (value == null)
                return default;
            if (value is IReadOnlyList<T> list)
                return FromList(list);
            return new ItemOrIReadOnlyList<T>((T)value, null, 1);
        }
    }
}