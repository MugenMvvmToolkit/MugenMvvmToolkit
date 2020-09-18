using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MugenMvvm.Internal
{
    public static class ItemOrList
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, IReadOnlyList<TItem>> FromItem<TItem>([AllowNull] TItem item) => new ItemOrList<TItem, IReadOnlyList<TItem>>(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, TList> FromItem<TItem, TList>([AllowNull] TItem item) where TList : class, IEnumerable<TItem> => new ItemOrList<TItem, TList>(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, IReadOnlyList<TItem>> FromListToReadOnly<TItem>(TItem[]? array)
        {
            if (array == null)
                return default;
            var count = array.Length;
            if (count > 1)
                return new ItemOrList<TItem, IReadOnlyList<TItem>>(array);
            return count == 1 ? new ItemOrList<TItem, IReadOnlyList<TItem>>(array[0]) : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, IReadOnlyList<TItem>> FromListToReadOnly<TItem>(List<TItem>? list)
        {
            if (list == null)
                return default;
            var count = list.Count;
            if (count > 1)
                return new ItemOrList<TItem, IReadOnlyList<TItem>>(list);
            return count == 1 ? new ItemOrList<TItem, IReadOnlyList<TItem>>(list[0]) : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, IReadOnlyList<TItem>> FromList<TItem>(IReadOnlyList<TItem>? readOnlyList)
        {
            if (readOnlyList == null)
                return default;
            var count = readOnlyList.Count;
            if (count > 1)
                return new ItemOrList<TItem, IReadOnlyList<TItem>>(readOnlyList);
            return count == 1 ? new ItemOrList<TItem, IReadOnlyList<TItem>>(readOnlyList[0]) : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, TItem[]> FromList<TItem>(TItem[]? array)
        {
            if (array == null)
                return default;
            var count = array.Length;
            if (count > 1)
                return new ItemOrList<TItem, TItem[]>(array);
            return count == 1 ? new ItemOrList<TItem, TItem[]>(array[0]) : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, IList<TItem>> FromList<TItem>(IList<TItem>? iList)
        {
            if (iList == null)
                return default;
            var count = iList.Count;
            if (count > 1)
                return new ItemOrList<TItem, IList<TItem>>(iList);
            return count == 1 ? new ItemOrList<TItem, IList<TItem>>(iList[0]) : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, List<TItem>> FromList<TItem>(List<TItem>? list)
        {
            if (list == null)
                return default;
            var count = list.Count;
            if (count > 1)
                return new ItemOrList<TItem, List<TItem>>(list);
            return count == 1 ? new ItemOrList<TItem, List<TItem>>(list[0]) : default;
        }

        public static ItemOrList<TItem, TList> FromList<TItem, TList>(TList? collection)
            where TList : class, IEnumerable<TItem>
        {
            if (collection == null)
                return default;

            int count;
            if (collection is TItem[] array)
            {
                count = array.Length;
                if (count > 1)
                    return new ItemOrList<TItem, TList>(collection);
                return count == 1 ? new ItemOrList<TItem, TList>(array[0]) : default;
            }

            if (collection is IReadOnlyList<TItem> l)
            {
                count = l.Count;
                if (count > 1)
                    return new ItemOrList<TItem, TList>(collection);
                return count == 1 ? new ItemOrList<TItem, TList>(l[0]) : default;
            }

            count = collection.Count();
            if (count > 1)
                return new ItemOrList<TItem, TList>(collection);
            return count == 1 ? new ItemOrList<TItem, TList>(collection.ElementAtOrDefault(0)) : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, TList> FromRawValue<TItem, TList>(object? value, bool @unchecked = false)
            where TList : class, IEnumerable<TItem>
        {
            if (value is TList list)
            {
                if (@unchecked)
                    return new ItemOrList<TItem, TList>(list);
                return FromList<TItem, TList>(list);
            }

            return new ItemOrList<TItem, TList>((TItem) value!);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, TList> FromListRaw<TItem, TList>(TList? list) where TList : class, IEnumerable<TItem> => new ItemOrList<TItem, TList>(list);

        //note because of slow cast for covariant\contravariant types (value is IReadonlyList<T> list), we should use this method, fixed in net5.0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ItemOrList<TItem, IReadOnlyList<TItem>> FromRawValueReadonly<TItem>(object? value, bool @unchecked = false)
        {
            if (value is TItem item)
                return new ItemOrList<TItem, IReadOnlyList<TItem>>(item);
            if (@unchecked)
                return new ItemOrList<TItem, IReadOnlyList<TItem>>((IReadOnlyList<TItem>?) value);
            return FromList((IReadOnlyList<TItem>?) value);
        }

        #endregion
    }
}