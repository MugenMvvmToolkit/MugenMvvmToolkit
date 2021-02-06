using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;

namespace MugenMvvm.Collections
{
    public static class ItemOrIEnumerable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIEnumerable<T> FromItem<T>(T? item) where T : class? => new(item, item != null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIEnumerable<T> FromItem<T>(T? item, bool hasItem) => new(item, hasItem);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIEnumerable<T> FromList<T>(T[]? array) => new(array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIEnumerable<T> FromList<T>(List<T>? list) => new(list);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIEnumerable<T> FromList<T>(IReadOnlyList<T>? readOnlyList) => new(readOnlyList);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIEnumerable<T> FromList<T>(IEnumerable<T>? enumerable) => new(enumerable);

        [Preserve(Conditional = true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIEnumerable<T> FromRawValue<T>(object? value)
        {
            if (value is IEnumerable<T> list)
                return new ItemOrIEnumerable<T>(list);
            if (value == null)
                return default;
            return new ItemOrIEnumerable<T>((T) value, true);
        }
    }
}