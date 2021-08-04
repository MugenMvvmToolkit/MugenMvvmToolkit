using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct CollectionChangedEventInfo<T> : IEquatable<CollectionChangedEventInfo<T>> where T : class
    {
        public readonly IReadOnlyObservableCollection Collection;
        public readonly CollectionChangedAction Action;
        public readonly T? Item;
        public readonly object? Parameter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal CollectionChangedEventInfo(IReadOnlyObservableCollection collection, T? item, object? parameter, CollectionChangedAction action)
        {
            Action = action;
            Collection = collection;
            Item = item;
            Parameter = parameter;
        }

        public bool IsCollectionEvent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Action != CollectionChangedAction.Changed;
        }

        public object? OldItem
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (Action == CollectionChangedAction.Replace)
                    return Parameter;
                return null;
            }
        }

        public string? Member
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Parameter as string;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMemberChanged(string member, bool emptyMemberResult = true) =>
            Action == CollectionChangedAction.Changed && Item != null && (member == Member || Member == "" && emptyMemberResult);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMemberOrCollectionChanged(string member, bool emptyMemberResult = true) => IsCollectionEvent || member == Member || Member == "" && emptyMemberResult;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(CollectionChangedEventInfo<T> other) => Action == other.Action &&
                                                                   EqualityComparer<T?>.Default.Equals(Item, other.Item) && Equals(Parameter, other.Parameter);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is CollectionChangedEventInfo<T> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine((int)Action, Item, Parameter);
    }
}