using System;
using System.Collections.Generic;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;

namespace MugenMvvm.Collections
{
    internal sealed class MaxMinClosure<T, TResult>
    {
        private readonly TResult? _defaultValue;
        private readonly Func<T, TResult?> _selector;
        private readonly Action<T?, TResult?> _onChanged;
        private readonly IComparer<TResult?>? _comparer;
        private readonly bool _isMax;
        private readonly Func<T, bool>? _predicate;
        private bool _isDirty;
        private bool _isPendingReset;
        private bool _hasValue;
        private T? _item;
        private TResult? _value;

        public MaxMinClosure(TResult? defaultValue, Func<T, TResult?> selector, Action<T?, TResult?> onChanged, IComparer<TResult?>? comparer, bool isMax, Func<T, bool>? predicate)
        {
            _defaultValue = defaultValue;
            _selector = selector;
            _onChanged = onChanged;
            _comparer = comparer;
            _isMax = isMax;
            _predicate = predicate;
        }

        public (TResult?, bool) OnAdded(TrackerCollectionDecorator<T, (TResult? value, bool hasValue)> items, T item, (TResult? value, bool hasValue) state, int count)
        {
            if (count == 1)
                state = Get(item);
            else if (items.IsReset)
                return OnChanged(items, item, state, count, null);

            if (state.hasValue && Check(_value, state.value))
                Set(item, state.value!, items.IsBatchUpdate, true);
            return state;
        }

        public (TResult?, bool) OnRemoved(TrackerCollectionDecorator<T, (TResult? value, bool hasValue)> items, T item, (TResult? value, bool hasValue) state, int count)
        {
            if (count == 0)
            {
                if (state.hasValue && _comparer.CompareOrDefault(_value, state.value) == 0)
                    Invalidate(items);
            }
            else if (items.IsReset)
                return OnChanged(items, item, state, count, null);

            return state;
        }

        public (TResult?, bool) OnChanged(TrackerCollectionDecorator<T, (TResult? value, bool hasValue)> items, T item, (TResult? value, bool hasValue) state, int count,
            object? args)
        {
            var newValue = Get(item);
            if (newValue.hasValue == state.hasValue)
            {
                if (_comparer.CompareOrDefault(state.value, newValue.value) == 0)
                    return newValue;

                if (_comparer.CompareOrDefault(_value, state.value) == 0)
                {
                    if (Check(_value, newValue.value))
                        Set(item, newValue.value, false, true);
                    else
                        Invalidate(items, true, item, newValue);
                }
                else if (Check(_value, newValue.value))
                    Set(item, newValue.value, false, true);

                return newValue;
            }

            if (state.hasValue)
            {
                if (_comparer.CompareOrDefault(_value, state.value) == 0)
                    Invalidate(items, true, item, newValue);
            }

            if (newValue.hasValue)
            {
                if (Check(_value, newValue.value))
                    Set(item, newValue.value, false, true);
            }

            return newValue;
        }

        public void OnEndBatchUpdate(TrackerCollectionDecorator<T, (TResult? value, bool hasValue)> items)
        {
            if (_isDirty)
            {
                _isDirty = false;
                if (_isPendingReset)
                {
                    _isPendingReset = false;
                    Invalidate(items);
                }
                else
                    _onChanged(_item, _value);
            }
        }

        private bool Check(TResult? current, TResult? value)
        {
            if (!_hasValue)
                return true;
            var compare = _comparer.CompareOrDefault(current, value);
            if (_isMax)
                return compare < 0;
            return compare > 0;
        }

        private (TResult? value, bool hasValue) Get(T item)
        {
            if (_predicate == null || _predicate(item))
                return (_selector(item), true);
            return default;
        }
        
        private void Set(T? item, TResult? value, bool isBatch, bool hasValue, bool force = false)
        {
            if (!force && _hasValue == hasValue && EqualityComparer<TResult?>.Default.Equals(_value, value) && EqualityComparer<T?>.Default.Equals(item, _item))
                return;
            _hasValue = hasValue;
            _item = item;
            _value = value;
            if (isBatch)
                _isDirty = true;
            else
                _onChanged(item, value);
        }

        private void Invalidate(TrackerCollectionDecorator<T, (TResult? value, bool hasValue)> items, bool hasCurrentItem = false, T? currentItem = default,
            (TResult? value, bool hasValue) currentValue = default)
        {
            if (items.IsBatchUpdate)
            {
                _isDirty = true;
                _isPendingReset = true;
                return;
            }

            if (items.ItemsRaw.Count == 0)
                Set(default, _defaultValue, false, false, true);
            else
            {
                var hasValue = false;
                T? item = default;
                var value = _defaultValue;
                foreach (var pair in items.ItemsRaw)
                {
                    if (!pair.Value.state.hasValue)
                        continue;

                    TResult? itemValue;
                    if (hasCurrentItem && EqualityComparer<T?>.Default.Equals(pair.Key, currentItem))
                    {
                        if (!currentValue.hasValue)
                            continue;
                        itemValue = currentValue.value;
                    }
                    else
                        itemValue = pair.Value.state.value;

                    if (!hasValue)
                    {
                        item = pair.Key;
                        value = itemValue;
                        hasValue = true;
                        continue;
                    }

                    if (Check(value, itemValue))
                    {
                        item = pair.Key;
                        value = itemValue;
                    }
                }

                Set(item, value, false, hasValue, true);
            }
        }
    }
}