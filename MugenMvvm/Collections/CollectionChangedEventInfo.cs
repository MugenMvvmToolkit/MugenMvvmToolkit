using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Collections.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct CollectionChangedEventInfo<T> : IEquatable<CollectionChangedEventInfo<T>> where T : class
    {
        private readonly CollectionObserverBase _observer;
        public readonly CollectionChangedAction Action;
        public readonly T? Item;
        public readonly object? Parameter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal CollectionChangedEventInfo(CollectionObserverBase observer, T? item, object? parameter, CollectionChangedAction action)
        {
            _observer = observer;
            Action = action;
            Item = item;
            Parameter = parameter;
        }

        public IReadOnlyObservableCollection? Collection
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _observer.OwnerOptional;
        }

        public IEnumerable<object?> Items
        {
            get
            {
                var owner = _observer.OwnerOptional;
                if (owner == null)
                    return Default.EmptyEnumerable<object?>();
                return _observer.GetItems(owner);
            }
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