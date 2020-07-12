using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static Task WhenAll<TList>(this ItemOrList<Task, TList> itemOrList) where TList : class, IEnumerable<Task>
        {
            if (itemOrList.Item != null)
                return itemOrList.Item;
            if (itemOrList.List != null)
                return Task.WhenAll(itemOrList.List);
            return Default.CompletedTask;
        }

        [return: MaybeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TItem FirstOrDefault<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TList : class, IEnumerable<TItem>
        {
            if (itemOrList.List != null)
                return itemOrList.List.FirstOrDefault();
            return itemOrList.Item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class?
            where TList : class, ICollection<TItem>
        {
            if (itemOrList.List != null)
                return itemOrList.List.Count;
            return itemOrList.Item == null ? 0 : 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count<TItem>(this ItemOrList<TItem, TItem[]> itemOrList)
            where TItem : class?
        {
            if (itemOrList.List != null)
                return itemOrList.List.Length;
            return itemOrList.Item == null ? 0 : 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count<TItem, TList>(this ItemOrList<TItem, TList> itemOrList, Func<TItem, bool> isNullOrEmpty)
            where TList : class, ICollection<TItem>
        {
            if (itemOrList.List != null)
                return itemOrList.List.Count;
            return isNullOrEmpty(itemOrList.Item!) ? 0 : 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TItem Get<TItem, TList>(this ItemOrList<TItem, TList> itemOrList, int index)
            where TList : class, IList<TItem>
        {
            if (itemOrList.List != null)
                return itemOrList.List[index];

            if (index == 0)
                return itemOrList.Item!;

            ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
            return default;
        }

        public static void Set<TItem, TList>(this ref ItemOrList<TItem, TList> itemOrList, TItem item, int index)
            where TList : class, IList<TItem>
        {
            if (itemOrList.List != null)
            {
                itemOrList.List[index] = item;
                return;
            }

            if (index == 0)
                itemOrList = item!;
            else
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
        }

        public static void Add<TItem>(this ref ItemOrList<TItem, IList<TItem>> itemOrList, TItem? item)
            where TItem : class
        {
            itemOrList!.Add(item, i => i == null);
        }

        public static void Add<TItem>(this ref ItemOrList<TItem, IList<TItem>> itemOrList, [AllowNull] TItem item, Func<TItem, bool> isNullOrEmpty)
        {
            itemOrList.Add(item, isNullOrEmpty, () => new List<TItem>());
        }

        public static void Add<TItem>(this ref ItemOrList<TItem, List<TItem>> itemOrList, TItem? item)
            where TItem : class
        {
            itemOrList!.Add(item, i => i == null);
        }

        public static void Add<TItem>(this ref ItemOrList<TItem, List<TItem>> itemOrList, [AllowNull] TItem item, Func<TItem, bool> isNullOrEmpty)
        {
            itemOrList.Add(item, isNullOrEmpty, () => new List<TItem>());
        }

        public static void Add<TItem, TList>(this ref ItemOrList<TItem, TList> itemOrList, [AllowNull] TItem item, Func<TItem, bool> isNullOrEmpty, Func<TList> getNewList)
            where TList : class, ICollection<TItem>
        {
            if (isNullOrEmpty(item!))
                return;
            if (itemOrList.List != null)
                itemOrList.List.Add(item!);
            else if (isNullOrEmpty(itemOrList.Item!))
                itemOrList = item!;
            else
            {
                var list = getNewList();
                list.Add(itemOrList.Item!);
                list.Add(item!);
                itemOrList = list;
            }
        }

        public static void AddRange<TItem>(this ref ItemOrList<TItem, IList<TItem>> itemOrList, ItemOrList<TItem, IReadOnlyList<TItem>> value)
            where TItem : class?
        {
            itemOrList.AddRange(value, item => item == null, () => new List<TItem>());
        }

        public static void AddRange<TItem>(this ref ItemOrList<TItem, IList<TItem>> itemOrList, ItemOrList<TItem, IReadOnlyList<TItem>> value, Func<TItem, bool> isNullOrEmpty)
        {
            itemOrList.AddRange(value, isNullOrEmpty, () => new List<TItem>());
        }

        public static void AddRange<TItem>(this ref ItemOrList<TItem, List<TItem>> itemOrList, ItemOrList<TItem, IReadOnlyList<TItem>> value)
            where TItem : class?
        {
            itemOrList.AddRange(value, item => item == null, () => new List<TItem>());
        }

        public static void AddRange<TItem>(this ref ItemOrList<TItem, List<TItem>> itemOrList, ItemOrList<TItem, IReadOnlyList<TItem>> value, Func<TItem, bool> isNullOrEmpty)
        {
            itemOrList.AddRange(value, isNullOrEmpty, () => new List<TItem>());
        }

        public static void AddRange<TItem, TList>(this ref ItemOrList<TItem, TList> itemOrList, ItemOrList<TItem, IReadOnlyList<TItem>> value, Func<TItem, bool> isNullOrEmpty, Func<TList> getNewList)
            where TList : class, ICollection<TItem>
        {
            if (isNullOrEmpty(value.Item!) && value.List == null)
                return;

            var items = itemOrList.List;
            if (items == null)
            {
                if (isNullOrEmpty(itemOrList.Item!))
                {
                    if (value.List == null)
                        itemOrList = value.Item!;
                    else
                    {
                        items = getNewList();
                        items.AddRange(value.List);
                        itemOrList = items;
                    }

                    return;
                }

                items = getNewList();
                items.Add(itemOrList.Item!);
            }

            if (value.List == null)
                items.Add(value.Item!);
            else
                items.AddRange(value.List);

            itemOrList = items;
        }

        public static bool Remove<TItem, TList>(this ref ItemOrList<TItem, TList> itemOrList, TItem item)
            where TList : class, ICollection<TItem>
        {
            if (itemOrList.List != null)
            {
                itemOrList.List.Remove(item);
                itemOrList = itemOrList.List;
                return true;
            }

            if (EqualityComparer<TItem>.Default.Equals(itemOrList.Item!, item))
            {
                itemOrList = default;
                return true;
            }

            return false;
        }

        public static void RemoveAt<TItem, TList>(this ref ItemOrList<TItem, TList> itemOrList, int index)
            where TList : class, IList<TItem>
        {
            if (itemOrList.List != null)
            {
                itemOrList.List.RemoveAt(index);
                itemOrList = itemOrList.List;
                return;
            }

            if (index == 0)
                itemOrList = default;
            else
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
        }

        public static TItem[] ToArray<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class?
            where TList : class, IEnumerable<TItem>
        {
            return itemOrList.ToArray(item => item == null);
        }

        public static TItem[] ToArray<TItem, TList>(this ItemOrList<TItem, TList> itemOrList, Func<TItem, bool> isNullOrEmpty)
            where TList : class, IEnumerable<TItem>
        {
            var list = itemOrList.List;
            if (list != null)
                return list.ToArray();

            if (isNullOrEmpty(itemOrList.Item!))
                return Default.Array<TItem>();
            return new[] { itemOrList.Item! };
        }

        public static List<TItem> ToList<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class?
            where TList : class, IEnumerable<TItem>
        {
            return itemOrList.ToList(item => item == null);
        }

        public static List<TItem> ToList<TItem, TList>(this ItemOrList<TItem, TList> itemOrList, Func<TItem, bool> isNullOrEmpty)
            where TList : class, IEnumerable<TItem>
        {
            var list = itemOrList.List;
            if (list != null)
                return list.ToList();

            if (isNullOrEmpty(itemOrList.Item!))
                return new List<TItem>();
            return new List<TItem> { itemOrList.Item! };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetRawValue<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class?
            where TList : class, IEnumerable<TItem>
        {
            return (object?)itemOrList.Item ?? itemOrList.List;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<TItem, TList>(this ItemOrList<TItem, TList> itemOrList)
            where TItem : class?
            where TList : class, IEnumerable<TItem>
        {
            return itemOrList.Item == null && itemOrList.List == null;
        }

        public static IList<T> AsList<T>(this ItemOrList<T, IList<T>> itemOrList) where T : class?
        {
            return itemOrList.AsList(arg => arg == null);
        }

        public static IList<T> AsList<T>(this ItemOrList<T, IList<T>> itemOrList, Func<T, bool> isNullOrEmpty)
        {
            return itemOrList.AsList(isNullOrEmpty, () => Default.Array<T>(), arg => new[] { arg });
        }

        public static IReadOnlyList<T> AsList<T>(this ItemOrList<T, IReadOnlyList<T>> itemOrList) where T : class?
        {
            return itemOrList.AsList(arg => arg == null);
        }

        public static IReadOnlyList<T> AsList<T>(this ItemOrList<T, IReadOnlyList<T>> itemOrList, Func<T, bool> isNullOrEmpty)
        {
            return itemOrList.AsList(isNullOrEmpty, () => Default.Array<T>(), arg => new[] { arg });
        }

        public static T[] AsList<T>(this ItemOrList<T, T[]> itemOrList, Func<T, bool> isNullOrEmpty)
        {
            return itemOrList.AsList(isNullOrEmpty, () => Default.Array<T>(), arg => new[] { arg });
        }

        public static T[] AsList<T>(this ItemOrList<T, T[]> itemOrList) where T : class?
        {
            return itemOrList.AsList(arg => arg == null);
        }

        public static TList AsList<T, TList>(this ItemOrList<T, TList> itemOrList, Func<T, bool> isNullOrEmpty, Func<TList> getDefaultList, Func<T, TList> getItemList)
            where TList : class, IEnumerable<T>
        {
            if (itemOrList.List != null)
                return itemOrList.List;
            if (isNullOrEmpty(itemOrList.Item!))
                return getDefaultList();
            return getItemList(itemOrList.Item!);
        }

        #endregion
    }
}