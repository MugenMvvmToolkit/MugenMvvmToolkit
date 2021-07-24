using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Collections.Components
{
    public class FilterCollectionDecorator<T> : CollectionDecoratorBase, IReadOnlyCollection<object?>
    {
        private Func<T, bool>? _filter;
        private IndexMapList<object?> _list;

        public FilterCollectionDecorator(Func<T, bool>? filter = null, int priority = CollectionComponentPriority.FilterDecorator) : base(priority)
        {
            _filter = filter;
            _list = IndexMapList<object?>.Get();
            Priority = priority;
            NullItemResult = true;
        }

        public override bool HasAdditionalItems => false;

        public Func<T, bool>? Filter
        {
            get => _filter;
            set
            {
                if (_filter != value)
                    UpdateFilterInternal(value);
            }
        }

        public bool NullItemResult { get; set; }

        [MemberNotNullWhen(true, nameof(_filter))]
        private bool HasFilter => _filter != null;

        int IReadOnlyCollection<object?>.Count => _list.Size;

        public void Invalidate() => UpdateFilterInternal(_filter);

        public IEnumerator<object?> GetEnumerator()
        {
            for (var i = 0; i < _list.Size; i++)
                yield return _list.Values[i];
        }

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
            if (FilterInternal(item))
            {
                if (filterIndex < 0)
                {
                    index = _list.Add(index, item, filterIndex);
                    decoratorManager.OnAdded(collection, this, item, index);
                }
                else
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
            _list.UpdateIndexes(index, 1, binarySearchIndex);
            if (!FilterInternal(item))
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
                if (FilterInternal(newItem))
                    decoratorManager.OnAdded(collection, this, newItem, _list.Add(index, newItem, filterIndex));

                return false;
            }

            if (FilterInternal(newItem))
            {
                oldItem = _list.GetValue(filterIndex)!;
                _list.SetValue(filterIndex, newItem);
                index = filterIndex;
                return true;
            }

            var oldValue = _list.GetValue(filterIndex);
            _list.RemoveAt(filterIndex);
            decoratorManager.OnRemoved(collection, this, oldValue, filterIndex);
            return false;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            if (!HasFilter)
                return true;

            var filterIndex = _list.IndexOfKey(oldIndex);
            _list.UpdateIndexes(oldIndex + 1, -1);
            _list.UpdateIndexes(newIndex, 1);

            if (filterIndex == -1)
                return false;

            _list.RemoveAt(filterIndex);
            oldIndex = filterIndex;
            newIndex = _list.Add(newIndex, item);
            return true;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (!HasFilter)
                return true;

            var removeIndex = _list.BinarySearch(index);
            _list.UpdateIndexes(index, -1, removeIndex);
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

        private void UpdateFilterInternal(Func<T, bool>? filter)
        {
            if (DecoratorManager == null)
            {
                _filter = filter;
                return;
            }

            using var _ = Owner.TryLock();
            _filter = filter;
            _list.Clear();
            if (HasFilter)
                UpdateItems(DecoratorManager.Decorate(Owner, this));
            DecoratorManager.OnReset(Owner, this, this);
        }

        private void UpdateItems(IEnumerable<object?> items)
        {
            if (items is IReadOnlyCollection<object?> c)
                _list.EnsureCapacity(c.Count);

            var index = 0;
            foreach (var item in items)
            {
                if (FilterInternal(item))
                    _list.AddRaw(index, item);
                ++index;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool FilterInternal(object? value)
        {
            if (value == null)
                return NullItemResult;
            return value is not T v || _filter!(v);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}