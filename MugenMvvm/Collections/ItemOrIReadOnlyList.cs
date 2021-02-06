using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;

namespace MugenMvvm.Collections
{
    public static class ItemOrIReadOnlyList
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyList<T> FromItem<T>(T? item) where T : class? => new(item, item != null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyList<T> FromItem<T>(T? item, bool hasItem) => new(item, hasItem);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyList<T> FromList<T>(T[]? array) => new(array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyList<T> FromList<T>(List<T>? list) => new(list);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyList<T> FromList<T>(IReadOnlyList<T>? readOnlyList) => new(readOnlyList);

        [Preserve(Conditional = true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyList<T> FromRawValue<T>(object? value)
        {
            if (value is IReadOnlyList<T> list)
                return new ItemOrIReadOnlyList<T>(list);
            if (value == null)
                return default;
            return new ItemOrIReadOnlyList<T>((T) value, true);
        }
    }
}