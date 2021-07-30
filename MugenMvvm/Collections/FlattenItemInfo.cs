using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct FlattenItemInfo
    {
        internal readonly IEnumerable? Items;
        internal readonly bool DecoratedItems;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FlattenItemInfo(IEnumerable? items, bool decoratedItems = true)
        {
            Items = items;
            DecoratedItems = decoratedItems;
        }

        [MemberNotNullWhen(false, nameof(Items))]
        internal bool IsEmpty => Items == null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IEnumerable<object?> GetItems() => DecoratedItems ? Items!.DecoratedItems() : Items!.AsEnumerable();

        internal FlattenCollectionItemBase GetCollectionItem(FlattenCollectionDecorator decorator)
        {
            var isWeak = Items is IReadOnlyObservableCollection;
            if (DecoratedItems)
                return new FlattenDecoratedCollectionItem(Items!, decorator, isWeak);

            var itemType = MugenExtensions.GetCollectionItemType(Items!);
            if (!itemType.IsValueType)
                return new FlattenCollectionItem<object?>().Initialize(Items!, decorator, isWeak);

            return ((FlattenCollectionItemBase) Activator.CreateInstance(typeof(FlattenCollectionItem<>).MakeGenericType(itemType))!).Initialize(Items!, decorator, isWeak);
        }
    }
}