using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections.Components
{
    public sealed class FirstLastTrackerCollectionDecorator<T> : CollectionDecoratorBase, IListenerCollectionDecorator, ICollectionBatchUpdateListener
    {
        private readonly bool _allowNull;
        private readonly Action<Optional<T?>> _setter;
        private readonly Func<T, bool>? _condition;
        private readonly bool _isFirstTracker;
        private T? _value;
        private int _index;
        private bool? _hasValue;
        private bool _isInBatch;
        private bool _pendingReset;
        private bool _isDirty;

        public FirstLastTrackerCollectionDecorator(int priority, bool allowNull, bool isFirstTracker, Action<Optional<T?>> setter, Func<T, bool>? condition) : base(priority)
        {
            Should.NotBeNull(setter, nameof(setter));
            _allowNull = allowNull && TypeChecker.IsNullable<T>();
            _setter = setter;
            _condition = condition;
            _isFirstTracker = isFirstTracker;
        }

        protected override bool IsLazy => false;

        protected override bool HasAdditionalItems => false;

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => items;

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args)
        {
            if (index == _index)
            {
                if (!IsValid((T) item!))
                    Invalidate(decoratorManager, collection);
            }
            else
                UpdateIfNeed(item, index);

            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (!UpdateIfNeed(item, index) && _hasValue.GetValueOrDefault() && index <= _index)
                ++_index;
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            if (index == _index)
            {
                if (IsValid(newItem, out var newItemT))
                    Set(newItemT, index, true);
                else
                    Invalidate(decoratorManager, collection);
            }
            else
                UpdateIfNeed(newItem, index);

            return true;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            var index = _index;
            if (oldIndex < newIndex)
            {
                if (oldIndex < index && newIndex >= index)
                    --_index;
            }
            else
            {
                if (oldIndex > index && newIndex <= index)
                    ++_index;
            }

            if (newIndex == index)
            {
                if (IsValid(item, out var itemT))
                    Set(itemT, newIndex, true);
            }
            else if (oldIndex == index)
            {
                _index = newIndex;
                if (_isFirstTracker)
                {
                    if (newIndex > oldIndex)
                        Invalidate(decoratorManager, collection);
                }
                else
                {
                    if (newIndex < oldIndex)
                        Invalidate(decoratorManager, collection);
                }
            }
            else
                UpdateIfNeed(item, newIndex);

            return true;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (index == _index)
                Invalidate(decoratorManager, collection);
            else if (index < _index)
                --_index;
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            Invalidate(items);
            return true;
        }

        private bool UpdateIfNeed(object? item, int index)
        {
            if (_isFirstTracker)
            {
                if (index <= _index && IsValid(item, out var itemT))
                {
                    Set(itemT, index, true);
                    return true;
                }
            }
            else
            {
                if (index > _index && IsValid(item, out var itemT))
                {
                    Set(itemT, index, true);
                    return true;
                }
            }

            return false;
        }

        private void Set(T? item, int index, bool hasValue, bool force = false)
        {
            _index = index;
            if (!force && _hasValue == hasValue && EqualityComparer<T?>.Default.Equals(item, _value))
                return;
            _value = item;
            _hasValue = hasValue;
            if (_isInBatch)
                _isDirty = true;
            else
                _setter(Optional.Get<T?>(item, hasValue));
        }

        private void Invalidate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, bool force = false) =>
            Invalidate(decoratorManager.Decorate(collection, this, false), force);

        private void Invalidate(IEnumerable<object?>? items, bool force = false)
        {
            if (_isInBatch)
            {
                _pendingReset = true;
                return;
            }

            if (items.IsNullOrEmpty())
            {
                Set(default, _isFirstTracker ? int.MaxValue : int.MinValue, false, force);
                return;
            }

            if (_isFirstTracker)
                Set(FirstOrDefault(items, out var i), i, i != int.MaxValue, force);
            else
                Set(LastOrDefault(items, out var i), i, i != int.MinValue, force);
        }

        private T? FirstOrDefault(IEnumerable<object?> items, out int index)
        {
            if (items is IReadOnlyList<object?> readOnlyList)
            {
                if (readOnlyList.Count == 0)
                {
                    index = int.MaxValue;
                    return default;
                }
            }
            else if (items is IList<object?> {Count: 0})
            {
                index = int.MaxValue;
                return default;
            }

            index = 0;
            foreach (var item in ItemOrIEnumerable.FromList(items))
            {
                if (IsValid(item, out var itemT))
                    return itemT;
                ++index;
            }

            index = int.MaxValue;
            return default;
        }

        private T? LastOrDefault(IEnumerable<object?> items, out int index)
        {
            if (items is IReadOnlyList<object> readOnlyList)
            {
                for (var i = readOnlyList.Count - 1; i >= 0; i--)
                {
                    if (IsValid(readOnlyList[i], out var itemT))
                    {
                        index = i;
                        return itemT;
                    }
                }

                index = int.MinValue;
                return default;
            }

            if (items is IList<object> list)
            {
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    if (IsValid(list[i], out var itemT))
                    {
                        index = i;
                        return itemT;
                    }
                }

                index = int.MinValue;
                return default;
            }

            index = int.MinValue;
            T? result = default;
            var currentIndex = 0;
            foreach (var item in ItemOrIEnumerable.FromList(items))
            {
                if (IsValid(item, out var itemT))
                {
                    result = itemT;
                    index = currentIndex;
                }

                ++currentIndex;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValid(object? item, out T? result) => item.TryCast(_allowNull, out result) && IsValid(result!);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValid(T item) => _condition == null || _condition(item);

        void ICollectionBatchUpdateListener.OnBeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType == BatchUpdateType.Decorators)
                _isInBatch = true;
        }

        void ICollectionBatchUpdateListener.OnEndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType != BatchUpdateType.Decorators || !_isInBatch)
                return;
            _isInBatch = false;
            if (_pendingReset)
            {
                var force = _isDirty;
                _pendingReset = false;
                _isDirty = false;
                var decoratorManager = DecoratorManager;
                if (decoratorManager != null)
                    Invalidate(decoratorManager, collection, force);
            }
            else if (_isDirty)
            {
                _isDirty = false;
                Set(_value, _index, _hasValue.GetValueOrDefault(), true);
            }
        }
    }
}