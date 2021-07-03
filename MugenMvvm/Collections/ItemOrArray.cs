using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;

namespace MugenMvvm.Collections
{
    public static class ItemOrArray
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrArray<T> Get<T>(int count)
        {
            if (count == 0)
                return default;
            if (count == 1)
                return new ItemOrArray<T>(default, null, 1);
            return new ItemOrArray<T>(default, new T[count], count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrArray<T> FromItem<T>(T? item) where T : class? => item == null ? default : new ItemOrArray<T>(item, null, 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrArray<T> FromItem<T>(T? item, bool hasItem) => hasItem ? new ItemOrArray<T>(item, null, 1) : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrArray<T> FromList<T>(T[]? array)
        {
            if (array == null || array.Length == 0)
                return default;
            if (array.Length == 1)
                return new ItemOrArray<T>(array[0], null, 1);
            return new ItemOrArray<T>(default, array, array.Length);
        }

        [Preserve(Conditional = true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrArray<T> FromRawValue<T>(object? value)
        {
            if (value == null)
                return default;
            if (value is T[] list)
                return FromList(list);
            return new ItemOrArray<T>((T)value, null, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ItemOrArray<T> FromRawValueFixedArray<T>(object? value)
        {
            if (value == null)
                return default;
            if (value is T[] t)
                return new ItemOrArray<T>(default, t, t.Length);
            return new ItemOrArray<T>((T)value, null, 1);
        }
    }
}