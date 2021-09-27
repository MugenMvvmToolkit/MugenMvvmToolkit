using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Metadata;

namespace MugenMvvm.Collections.Components
{
    public sealed class FilterCollectionDecorator<T> : CollectionDecoratorBase, IReadOnlyList<object?>, IHasCache
    {
        private Func<T, int, bool>? _filter;
        private IndexMapList<object?> _list;

        public FilterCollectionDecorator(int priority, Func<T, int, bool>? filter = null) : base(priority)
        {
            _filter = filter;
            _list = IndexMapList<object?>.Get();
            Priority = priority;
            NullItemResult = true;
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

        public bool NullItemResult { get; set; }

        protected override bool HasAdditionalItems => false;

        [MemberNotNullWhen(true, nameof(_filter))]
        private bool HasFilter => _filter != null;

        int IReadOnlyCollection<object?>.Count => _list.Size;

        object? IReadOnlyList<object?>.this[int index] => _list.Indexes[index].Value;

        public IEnumerator<object?> GetEnumerator()
        {
            for (var i = 0; i < _list.Size; i++)
                yield return _list.Indexes[i].Value;
        }

        public void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null) => UpdateFilterInternal(_filter);

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            _list.Clear();
        }

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => HasFilter ? this : items;

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args)
        {
            if (!HasFilter)
                return true;

            var filterIndex = _list.BinarySearch(index);
            if (FilterInternal(item, index, args))
            {
                if (filterIndex < 0)
                {
                    decoratorManager.OnAdded(collection, this, item, _list.Add(index, item, filterIndex));
                    return false;
                }

                index = filterIndex;
                return true;
            }

            if (filterIndex >= 0)
            {
                _list.RemoveAt(filterIndex);
                decoratorManager.OnRemoved(collection, this, item, filterIndex);
            }

            return false;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (!HasFilter)
                return true;

            var binarySearchIndex = _list.BinarySearch(index);
            _list.UpdateIndexesBinary(binarySearchIndex, 1);
            if (!FilterInternal(item, index))
                return false;

            index = _list.Add(index, item, binarySearchIndex);
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            if (!HasFilter)
                return true;

            var filterIndex = _list.BinarySearch(index);
            if (filterIndex < 0)
            {
                if (FilterInternal(newItem, index))
                    decoratorManager.OnAdded(collection, this, newItem, _list.Add(index, newItem, filterIndex));

                return false;
            }

            if (FilterInternal(newItem, index))
            {
                oldItem = _list.Indexes[filterIndex].Value;
                _list.Indexes[filterIndex].Value = newItem;
                index = filterIndex;
                return true;
            }

            var oldValue = _list.Indexes[filterIndex].Value;
            _list.RemoveAt(filterIndex);
            decoratorManager.OnRemoved(collection, this, oldValue, filterIndex);
            return false;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex) => !HasFilter || _list.Move(ref oldIndex, ref newIndex, out _);

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (!HasFilter)
                return true;

            var removeIndex = _list.BinarySearch(index);
            _list.UpdateIndexesBinary(removeIndex, -1);
            if (removeIndex < 0)
                return false;

            _list.RemoveAt(removeIndex);
            index = removeIndex;
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (!HasFilter)
                return true;

            _list.Clear();
            if (items != null)
            {
                UpdateItems(items);
                items = this;
            }

            return true;
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

            _list.Clear();
            if (HasFilter)
                UpdateItems(decoratorManager.Decorate(owner, this));
            decoratorManager.OnReset(owner, this, this);
        }

        private void UpdateItems(IEnumerable<object?> items)
        {
            if (items.IsNullOrEmpty())
                return;

            var index = 0;
            foreach (var item in items)
            {
                if (FilterInternal(item, index))
                    _list.AddRaw(index, item);
                ++index;
            }
        }

        private bool FilterInternal(object? value, int index, object? args = null)
        {
            if (ReferenceEquals(args, CollectionMetadata.TrueFilterArgs))
                return true;
            if (ReferenceEquals(args, CollectionMetadata.FalseFilterArgs))
                return false;
            if (value == null)
                return NullItemResult;
            return value is not T v || _filter!(v, index);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}