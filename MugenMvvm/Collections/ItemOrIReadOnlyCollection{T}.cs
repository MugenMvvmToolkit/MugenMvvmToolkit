﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ItemOrIReadOnlyCollection<T> : IReadOnlyCollection<T>, IEquatable<ItemOrIReadOnlyCollection<T>>
    {
        internal readonly int FixedCount;
        public readonly T? Item;
        public readonly IReadOnlyCollection<T>? List;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrIReadOnlyCollection(T? item)
        {
            Item = item;
            List = null;
            FixedCount = 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ItemOrIReadOnlyCollection(T? item, IReadOnlyCollection<T>? list, int fixedCount)
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

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (FixedCount != 0)
                    return FixedCount;
                if (List == null)
                    return 0;
                return List.Count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIReadOnlyCollection<T>(T? item) => ItemOrIReadOnlyCollection.FromItem(item, item != null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIReadOnlyCollection<T>(T[]? items) => ItemOrIReadOnlyCollection.FromList(items);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIReadOnlyCollection<T>(List<T>? items) => ItemOrIReadOnlyCollection.FromList(items);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIReadOnlyCollection<T>(HashSet<T>? items) => ItemOrIReadOnlyCollection.FromList(items);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ItemOrIEnumerable<T>(ItemOrIReadOnlyCollection<T> itemOrList) => new(itemOrList.Item!, itemOrList.List, itemOrList.FixedCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ItemOrIReadOnlyCollection<T> left, ItemOrIReadOnlyCollection<T> right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ItemOrIReadOnlyCollection<T> left, ItemOrIReadOnlyCollection<T> right) => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ItemOrIReadOnlyCollection<T> other)
        {
            if (List != null)
                return Equals(List, other.List);
            return FixedCount == other.FixedCount && EqualityComparer<T?>.Default.Equals(Item, other.Item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is ItemOrIReadOnlyCollection<T> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(FixedCount, Item, List);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> AsEnumerable() => AsList();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrIReadOnlyCollection<TTo> CastTo<TTo>() => new((TTo?) (object?) Item, (IReadOnlyCollection<TTo>?) List, FixedCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IReadOnlyCollection<T> AsList()
        {
            if (List != null)
                return List;
            if (FixedCount == 0)
                return Default.ReadOnlyCollection<T>();
            return new[] {Item!};
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