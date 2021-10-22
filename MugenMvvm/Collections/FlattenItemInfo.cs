using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Collections.Components;
using MugenMvvm.Interfaces.Collections;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct FlattenItemInfo : IEquatable<FlattenItemInfo>
    {
        public readonly IEnumerable? Items;
        public readonly bool DecoratedItems;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FlattenItemInfo(IEnumerable? items, bool decoratedItems)
        {
            Items = items;
            DecoratedItems = decoratedItems;
        }

        [MemberNotNullWhen(false, nameof(Items))]
        public bool IsEmpty => Items == null;

        internal FlattenCollectionItemBase GetCollectionItem(object? item, FlattenCollectionDecorator decorator)
        {
            var isWeak = Items is IReadOnlyObservableCollection;
            if (DecoratedItems)
                return new FlattenDecoratedCollectionItem(item, Items!, decorator, isWeak);

            if (Items is not IReadOnlyObservableCollection observableCollection || !observableCollection.ItemType.IsValueType)
                return new FlattenCollectionItem<object?>().Initialize(item, Items!, decorator, isWeak);

            return ((FlattenCollectionItemBase) Activator
                .CreateInstance(typeof(FlattenCollectionItem<>).MakeGenericType(observableCollection.ItemType))!).Initialize(item, Items!, decorator, isWeak);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(FlattenItemInfo other) => ReferenceEquals(Items, other.Items) && DecoratedItems == other.DecoratedItems;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is FlattenItemInfo other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Items != null ? RuntimeHelpers.GetHashCode(Items) : 0) * 397) ^ DecoratedItems.GetHashCode();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(FlattenItemInfo left, FlattenItemInfo right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(FlattenItemInfo left, FlattenItemInfo right) => !left.Equals(right);
    }
}