using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct FlattenItemInfo
    {
        internal readonly IEnumerable? Items;
        internal readonly bool DecoratorListener;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FlattenItemInfo(IEnumerable? items, bool decoratorListener = true)
        {
            Items = items;
            DecoratorListener = decoratorListener;
        }

        [MemberNotNullWhen(false, nameof(Items))]
        internal bool IsEmpty => Items == null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IEnumerable<object?> GetItems() => DecoratorListener ? Items!.DecoratedItems() : Items!.AsEnumerable();

        internal FlattenCollectionItemBase GetCollectionItem(FlattenCollectionDecorator decorator)
        {
            if (DecoratorListener)
                return new DecoratorFlattenCollectionItem(Items!, decorator);

            var itemType = MugenExtensions.GetCollectionItemType(Items!);
            if (!itemType.IsValueType)
                return new SourceFlattenCollectionItem<object?>().Initialize(Items!, decorator);

            return ((FlattenCollectionItemBase) Activator.CreateInstance(typeof(SourceFlattenCollectionItem<>).MakeGenericType(itemType))!).Initialize(Items!, decorator);
        }
    }
}