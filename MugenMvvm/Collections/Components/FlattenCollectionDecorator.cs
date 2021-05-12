using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class FlattenCollectionDecorator : AttachableComponentBase<ICollection>, ICollectionDecorator, IHasPriority
    {
        private readonly Listener _listener;
        private ICollectionDecoratorManagerComponent? _decoratorManager;
        private int _offset;

        public FlattenCollectionDecorator(IEnumerable collection, bool isHeader = false, int priority = CollectionComponentPriority.FlattenCollectionDecorator)
        {
            Should.NotBeNull(collection, nameof(collection));
            Collection = collection;
            IsHeader = isHeader;
            Priority = priority;
            _listener = new Listener(this);
        }

        public bool IsHeader { get; }

        public IEnumerable Collection { get; }

        public int Priority { get; set; }

        protected override void OnAttached(ICollection owner, IReadOnlyMetadataContext? metadata)
        {
            _decoratorManager = CollectionDecoratorManager.GetOrAdd(owner);
            if (Collection is IComponentOwner<ICollection> componentOwner)
                componentOwner.AddComponent(_listener);
        }

        protected override void OnDetached(ICollection owner, IReadOnlyMetadataContext? metadata)
        {
            _decoratorManager = null;
            if (Collection is IComponentOwner<ICollection> componentOwner)
                componentOwner.RemoveComponent(_listener);
        }

        IEnumerable<object?> ICollectionDecorator.Decorate(ICollection collection, IEnumerable<object?> items) =>
            Decorate(Collection.Decorate(), items) ?? Enumerable.Empty<object?>();

        bool ICollectionDecorator.OnChanged(ICollection collection, ref object? item, ref int index, ref object? args)
        {
            if (IsHeader)
                index += _offset;
            return true;
        }

        bool ICollectionDecorator.OnAdded(ICollection collection, ref object? item, ref int index)
        {
            if (IsHeader)
                index += _offset;
            else
            {
                if (index > _offset)
                    --index;
                _offset++;
            }

            return true;
        }

        bool ICollectionDecorator.OnReplaced(ICollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            if (IsHeader)
                index += _offset;
            return true;
        }

        bool ICollectionDecorator.OnMoved(ICollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            if (IsHeader)
            {
                oldIndex += _offset;
                newIndex += _offset;
            }

            return true;
        }

        bool ICollectionDecorator.OnRemoved(ICollection collection, ref object? item, ref int index)
        {
            if (IsHeader)
                index += _offset;
            else
                --_offset;
            return true;
        }

        bool ICollectionDecorator.OnReset(ICollection collection, ref IEnumerable<object?>? items)
        {
            _offset = IsHeader ? Collection.Decorate().CountEx() : items.CountEx();
            items = Decorate(Collection.Decorate(), items);
            return true;
        }

        private IEnumerable<object?>? Decorate(IEnumerable<object?>? collection, IEnumerable<object?>? items)
        {
            if (items == null)
                return collection;
            if (collection == null)
                return items;
            return IsHeader ? collection.Concat(items) : items.Concat(collection);
        }

        private sealed class Listener : ICollectionDecoratorListener
        {
            private readonly FlattenCollectionDecorator _decorator;

            public Listener(FlattenCollectionDecorator decorator)
            {
                _decorator = decorator;
            }

            public void OnChanged(ICollection collection, object? item, int index, object? args)
            {
                using var l = TryLock(out var manager, out var owner);
                manager?.OnChanged(owner!, _decorator, GetNestedCollectionIndex(index), index, args);
            }

            public void OnAdded(ICollection collection, object? item, int index)
            {
                using var l = TryLock(out var manager, out var owner);
                if (manager == null)
                    return;
                if (_decorator.IsHeader)
                    ++_decorator._offset;
                manager.OnAdded(owner!, _decorator, item, GetNestedCollectionIndex(index));
            }

            public void OnReplaced(ICollection collection, object? oldItem, object? newItem, int index)
            {
                using var l = TryLock(out var manager, out var owner);
                manager?.OnReplaced(owner!, _decorator, oldItem, newItem, GetNestedCollectionIndex(index));
            }

            public void OnMoved(ICollection collection, object? item, int oldIndex, int newIndex)
            {
                using var l = TryLock(out var manager, out var owner);
                manager?.OnMoved(owner!, _decorator, item, GetNestedCollectionIndex(oldIndex), GetNestedCollectionIndex(newIndex));
            }

            public void OnRemoved(ICollection collection, object? item, int index)
            {
                using var l = TryLock(out var manager, out var owner);
                if (manager == null)
                    return;

                if (_decorator.IsHeader)
                    --_decorator._offset;
                manager.OnRemoved(owner!, _decorator, item, GetNestedCollectionIndex(index));
            }

            public void OnReset(ICollection collection, IEnumerable<object?>? items)
            {
                using var l = TryLock(out var manager, out var owner);
                if (manager == null)
                    return;

                if (_decorator.IsHeader)
                    _decorator._offset = items.CountEx();
                manager.OnReset(owner!, _decorator, _decorator.Decorate(items, manager.Decorate(owner!, _decorator)));
            }

            private int GetNestedCollectionIndex(int index) => _decorator.IsHeader ? index : index + _decorator._offset;

            private MonitorLocker TryLock(out ICollectionDecoratorManagerComponent? manager, out ICollection? owner)
            {
                var locker = _decorator.OwnerOptional.TryLock();
                owner = _decorator.OwnerOptional;
                manager = _decorator._decoratorManager;
                if (owner == null || manager == null)
                {
                    owner = null;
                    manager = null;
                }

                return locker;
            }
        }
    }
}