using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MugenMvvm.Collections
{
    public static class ItemOrArray
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrArray<T> FromItem<T>(T item) where T : class? => new(item, item != null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrArray<T> FromItem<T>([AllowNull] T item, bool hasItem) => new(item, hasItem);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrArray<T> FromList<T>(T[]? array) => new(array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrArray<T> FromRawValue<T>(object? value)
        {
            if (value is T[] list)
                return new ItemOrArray<T>(list);
            if (value == null)
                return default;
            return new ItemOrArray<T>((T) value, true);
        }

        #endregion
    }
}