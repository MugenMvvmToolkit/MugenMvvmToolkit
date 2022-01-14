using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ItemOrIEnumerable<T> : IEnumerable<T>, IEquatable<ItemOrIEnumerable<T>>
    {
        internal readonly int FixedCount;
        public readonly T? Item;
        public readonly IEnumerable<T>? List;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrIEnumerable(T? item)
        {
            Item = item;
            List = null;
            FixedCount = 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ItemOrIEnumerable(T? item, IEnumerable<T>? list, int fixedCount)
        {
            Item = item!;
            List = list;
            FixedCount = fixedCount;
        }

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => FixedCount == 0 && List == null;
        }

        public bool HasItem
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => FixedCount == 1 && List == null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIEnumerable<T>(T? item) => ItemOrIEnumerable.FromItem(item, item != null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIEnumerable<T>(T[]? items) => ItemOrIEnumerable.FromList(items);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIEnumerable<T>(List<T>? items) => ItemOrIEnumerable.FromList(items);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ItemOrIEnumerable<T> left, ItemOrIEnumerable<T> right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ItemOrIEnumerable<T> left, ItemOrIEnumerable<T> right) => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ItemOrIEnumerable<T> other)
        {
            if (List != null)
                return Equals(List, other.List);
            return FixedCount == other.FixedCount && EqualityComparer<T?>.Default.Equals(Item, other.Item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is ItemOrIEnumerable<T> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(FixedCount, Item, List);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrIEnumerable<TTo> CastTo<TTo>() => new((TTo?) (object?) Item, (IEnumerable<TTo>?) List, FixedCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count() => FixedCount == 0 ? List.CountEx() : FixedCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> AsEnumerable() => AsList();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> AsList()
        {
            if (List != null)
                return List;
            if (FixedCount == 0)
                return Default.Enumerable<T>();
            return Default.SingleItemEnumerable(Item!);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T> ToList()
        {
            if (List != null)
                return new List<T>(List);
            if (FixedCount == 0)
                return new List<T>();
            return new List<T> {Item!};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            if (List != null)
                return List.ToArray();
            if (FixedCount == 0)
                return Array.Empty<T>();
            return new[] {Item!};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrListEnumerator<T> GetEnumerator() => new(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerator<T> GetEnumeratorRef()
        {
            if (List != null)
                return List.GetEnumerator();
            if (FixedCount == 0)
                return Default.Enumerator<T>();
            return Default.SingleItemEnumerator(Item!);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumeratorRef();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorRef();
    }
}