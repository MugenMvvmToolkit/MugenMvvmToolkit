using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections.Components
{
    public sealed class MaxMinImmutableCollectionDecorator<T, TResult> : CollectionDecoratorBase, IListenerCollectionDecorator, ICollectionBatchUpdateListener
    {
        private readonly bool _allowNull;
        private readonly TResult? _defaultValue;
        private readonly Func<T, TResult?> _selector;
        private readonly Action<T?, TResult?> _onChanged;
        private readonly IComparer<TResult?>? _comparer;
        private readonly bool _isMax;
        private readonly Func<T, bool>? _predicate;
        private bool _hasValue;
        private T? _item;
        private TResult? _value;
        private bool _isInBatch;
        private bool _pendingReset;
        private bool _isDirty;

        public MaxMinImmutableCollectionDecorator(int priority, bool allowNull, TResult? defaultValue, Func<T, TResult?> selector, Action<T?, TResult?> onChanged,
            IComparer<TResult?>? comparer, bool isMax, Func<T, bool>? predicate) : base(priority)
        {
            Should.NotBeNull(selector, nameof(selector));
            Should.NotBeNull(onChanged, nameof(onChanged));
            _allowNull = allowNull && TypeChecker.IsNullable<T>();
            _defaultValue = defaultValue;
            _selector = selector;
            _onChanged = onChanged;
            _comparer = comparer;
            _isMax = isMax;
            _predicate = predicate;
        }

        protected override bool HasAdditionalItems => false;

        protected override bool IsLazy => false;

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => items;

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args) => true;

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (IsSatisfied(item, out var itemT, out var value) && CheckValue(_value, value))
                Set(itemT, value, true);
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            if (IsSatisfied(newItem, out var itemT, out var value) && CheckValue(_value, value))
                Set(itemT, value, true);
            if (IsSatisfied(oldItem, out itemT, out value) && CheckValue(itemT, value))
                Invalidate(decoratorManager, collection);
            return true;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex) => true;

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (IsSatisfied(item, out var itemT, out var value) && CheckValue(itemT, value))
                Invalidate(decoratorManager, collection);
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            Invalidate(items);
            return true;
        }

        private void Set(T? item, TResult? value, bool hasValue, bool force = false)
        {
            if (!force && _hasValue == hasValue && EqualityComparer<TResult?>.Default.Equals(_value, value) && EqualityComparer<T?>.Default.Equals(item, _item))
                return;
            _hasValue = hasValue;
            _item = item;
            _value = value;
            if (_isInBatch)
                _isDirty = true;
            else
                _onChanged(item, value);
        }

        private bool CheckValue(T? item, TResult? value)
        {
            if (!_hasValue)
                return false;
            return _comparer.CompareOrDefault(_value, value) == 0 && EqualityComparer<T?>.Default.Equals(_item, item);
        }

        private bool CheckValue(TResult? current, TResult? value)
        {
            if (!_hasValue)
                return true;
            var compare = _comparer.CompareOrDefault(current, value);
            if (_isMax)
                return compare < 0;
            return compare > 0;
        }

        private void Invalidate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, bool force = false) =>
            Invalidate(decoratorManager.Decorate(collection, this), force);

        private void Invalidate(IEnumerable<object?>? items, bool force = false)
        {
            if (_isInBatch)
            {
                _pendingReset = true;
                return;
            }

            if (items.IsNullOrEmpty())
            {
                Set(default, _defaultValue, false, force);
                return;
            }

            var hasValue = false;
            T? item = default;
            var value = _defaultValue;
            foreach (var i in items)
            {
                if (!IsSatisfied(i, out var itemT, out var v))
                    continue;

                if (!hasValue)
                {
                    item = itemT;
                    value = v;
                    hasValue = true;
                    continue;
                }

                if (CheckValue(value, v))
                {
                    item = itemT;
                    value = v;
                }
            }

            Set(item, value, hasValue, force);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSatisfied(object? item, [NotNullWhen(true)] out T? itemT, [NotNullWhen(true)] out TResult? value)
        {
            if (!item.TryCast(_allowNull, out itemT!) || _predicate != null && !_predicate(itemT))
            {
                value = default;
                return false;
            }

            value = _selector(itemT)!;
            return true;
        }

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
                Set(_item, _value, _hasValue, true);
            }
        }
    }
}