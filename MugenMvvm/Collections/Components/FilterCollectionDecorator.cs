using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class FilterCollectionDecorator<T> : CollectionDecoratorBase, IHasCache
    {
        private readonly bool _allowNull;
        private Func<T, int, bool>? _filter;
        private IndexMapList<object?> _items;

        public FilterCollectionDecorator(int priority, bool allowNull, Func<T, int, bool>? filter = null) : base(priority)
        {
            _allowNull = allowNull && TypeChecker.IsNullable<T>();
            _filter = filter;
            _items = IndexMapList<object?>.Get();
            Priority = priority;
        }

        public Func<T, int, bool>? Filter
        {
            get => _filter;
            set
            {
                if (_filter != value)
                    UpdateFilterInternal(value);
            }
        }

        protected override bool HasAdditionalItems => false;

        protected virtual bool HasItemDecorator => false;

        [MemberNotNullWhen(true, nameof(_filter))]
        private bool HasFilter => _filter != null;

        public void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null) => UpdateFilterInternal(_filter);

        protected virtual object? Decorate(object? item) => item;

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            _items.Clear();
        }

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => Decorate(items);

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args) => !HasFilter || Replace(decoratorManager, collection, item, item, ref index, true);

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (!HasFilter)
                return true;

            var binarySearchIndex = _items.BinarySearch(index);
            _items.UpdateIndexesBinary(binarySearchIndex, 1);
            if (!FilterInternal(item, index))
            {
                _items.Add(index, item, binarySearchIndex);
                return false;
            }

            index -= GetIndexOffset(binarySearchIndex);
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            if (!HasFilter)
                return true;

            return Replace(decoratorManager, collection, oldItem, newItem, ref index, false);
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            if (!HasFilter)
                return true;
            int? binarySearchIndexOld = null;
            int? binarySearchIndexNew = null;
            if (_items.Move(oldIndex, newIndex, out _, ref binarySearchIndexOld, ref binarySearchIndexNew))
                return false;

            if (binarySearchIndexNew.Value >= 0 && newIndex > oldIndex)
                newIndex--;
            oldIndex -= GetIndexOffset(binarySearchIndexOld.Value);
            newIndex -= GetIndexOffset(binarySearchIndexNew.Value);
            return oldIndex != newIndex;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (!HasFilter)
                return true;

            var removeIndex = _items.BinarySearch(index);
            _items.UpdateIndexesBinary(removeIndex, -1);
            if (removeIndex < 0)
            {
                index -= GetIndexOffset(removeIndex);
                return true;
            }

            _items.RemoveAt(removeIndex);
            return false;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (!HasFilter)
                return true;

            _items.Clear();
            if (items != null)
            {
                UpdateItems(items);
                items = Decorate(items);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetIndexOffset(int binarySearchIndex)
        {
            if (binarySearchIndex < 0)
                return ~binarySearchIndex;
            return binarySearchIndex;
        }

        private bool Replace(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, object? oldItem, object? newItem, ref int index,
            bool isChange)
        {
            var oldIndex = _items.BinarySearch(index);
            if (oldIndex < 0)
            {
                if (FilterInternal(newItem, index))
                {
                    index -= GetIndexOffset(oldIndex);
                    return true;
                }

                _items.Add(index, newItem, oldIndex);
                decoratorManager.OnRemoved(collection, this, Decorate(oldItem), index - GetIndexOffset(oldIndex));
                return false;
            }

            if (FilterInternal(newItem, index))
            {
                _items.RemoveAt(oldIndex);
                decoratorManager.OnAdded(collection, this, Decorate(newItem), index - GetIndexOffset(oldIndex));
                return false;
            }

            if (!isChange)
                _items.Indexes[oldIndex].Value = newItem;
            return false;
        }

        private void UpdateFilterInternal(Func<T, int, bool>? filter)
        {
            var decoratorManager = DecoratorManager;
            var owner = OwnerOptional;
            if (decoratorManager == null || owner == null)
            {
                _filter = filter;
                return;
            }

            using var _ = owner.Lock();
            _filter = filter;
            if (DecoratorManager == null)
                return;

            _items.Clear();
            var items = decoratorManager.Decorate(owner, this, false);
            if (HasFilter)
            {
                UpdateItems(items);
                items = Decorate(items);
            }

            decoratorManager.OnReset(owner, this, items);
        }

        private void UpdateItems(IEnumerable<object?> items)
        {
            if (items.IsNullOrEmpty())
                return;

            var index = 0;
            foreach (var item in items)
            {
                if (!FilterInternal(item, index))
                    _items.AddRaw(index, item);
                ++index;
            }
        }

        private IEnumerable<object?> Decorate(IEnumerable<object?> items)
        {
            if ((_items.Size == 0 || !HasFilter) && !HasItemDecorator)
                return items;
            return DecorateImpl(items);
        }

        private IEnumerable<object?> DecorateImpl(IEnumerable<object?> items)
        {
            var index = 0;
            var itemIndex = 0;
            foreach (var item in items)
            {
                if (itemIndex < _items.Size)
                {
                    var entry = _items.Indexes[itemIndex];
                    if (entry.Index == index)
                    {
                        ++index;
                        ++itemIndex;
                        continue;
                    }
                }

                ++index;
                yield return Decorate(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool FilterInternal(object? value, int index) => !value.TryCast<T>(_allowNull, out var itemT) || _filter!(itemT!, index);
    }
}