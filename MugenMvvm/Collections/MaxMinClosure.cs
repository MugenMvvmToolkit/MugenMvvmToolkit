using System;
using System.Collections.Generic;

namespace MugenMvvm.Collections
{
    internal sealed class MaxMinClosure<T, TResult>
    {
        private readonly TResult _defaultValue;
        private readonly Func<T, TResult?> _selector;
        private readonly Action<TResult?> _onChanged;
        private readonly IComparer<TResult>? _comparer;
        private readonly bool _isMax;
        private readonly Func<T, bool>? _predicate;
        private bool _isDirty;
        private bool _isPendingReset;
        private bool _hasValue;
        private TResult? _value;

        public MaxMinClosure(TResult defaultValue, Func<T, TResult?> selector, Action<TResult?> onChanged, IComparer<TResult>? comparer, bool isMax,
            Func<T, bool>? predicate = null)
        {
            _defaultValue = defaultValue;
            _selector = selector;
            _onChanged = onChanged;
            _comparer = comparer;
            _isMax = isMax;
            _predicate = predicate;
        }

        public (TResult?, bool) OnAdded(IReadOnlyDictionary<T, ((TResult? value, bool hasValue) state, int count)> items, T item, (TResult? value, bool hasValue) state,
            int count, bool isReset)
        {
            if (count == 1)
                state = GetValue(item);
            else if (isReset)
                return OnChanged(items, item, state, count, true, null);

            if (state.hasValue && CheckValue(_value, state.value))
                SetValue(state.value!, isReset, true);
            return state;
        }

        public (TResult?, bool) OnRemoved(IReadOnlyDictionary<T, ((TResult? value, bool hasValue) state, int count)> items, T item, (TResult? value, bool hasValue) state,
            int count, bool isReset)
        {
            if (count == 0)
            {
                if (state.hasValue && Compare(_value, state.value) == 0)
                    Reset(items, isReset);
            }
            else if (isReset)
                return OnChanged(items, item, state, count, true, null);

            return state;
        }

        public (TResult?, bool) OnChanged(IReadOnlyDictionary<T, ((TResult? value, bool hasValue) state, int count)> items, T item, (TResult? value, bool hasValue) state,
            int count, bool isReset, object? args)
        {
            var newValue = GetValue(item);
            if (newValue.hasValue == state.hasValue)
            {
                if (Compare(state.value, newValue.value) == 0)
                    return newValue;

                if (Compare(_value, state.value) == 0)
                {
                    if (CheckValue(_value, newValue.value))
                        SetValue(newValue.value, false, true);
                    else
                        Reset(items, isReset, true, item, newValue);
                }
                else if (CheckValue(_value, newValue.value))
                    SetValue(newValue.value, false, true);

                return newValue;
            }

            if (state.hasValue)
            {
                if (Compare(_value, state.value) == 0)
                    Reset(items, isReset, true, item, newValue);
            }

            if (newValue.hasValue)
            {
                if (CheckValue(_value, newValue.value))
                    SetValue(newValue.value, false, true);
            }

            return newValue;
        }

        public void OnReset(IReadOnlyDictionary<T, ((TResult? value, bool hasValue) state, int count)> items)
        {
            if (_isDirty)
            {
                _isDirty = false;
                if (_isPendingReset)
                {
                    _isPendingReset = false;
                    Reset(items, false);
                }
                else
                    _onChanged(_value);
            }
        }

        private void SetValue(TResult? value, bool isReset, bool hasValue, bool force = false)
        {
            if (!force && _hasValue == hasValue && EqualityComparer<TResult?>.Default.Equals(_value, value))
                return;
            _hasValue = hasValue;
            _value = value;
            if (isReset)
                _isDirty = true;
            else
                _onChanged(value);
        }

        private int Compare(TResult? current, TResult? value) => _comparer == null ? Comparer<TResult?>.Default.Compare(current, value) : _comparer.Compare(current, value);

        private bool CheckValue(TResult? current, TResult? value)
        {
            if (!_hasValue)
                return true;
            var compare = Compare(current, value);
            if (_isMax)
                return compare < 0;
            return compare > 0;
        }

        private (TResult? value, bool hasValue) GetValue(T item)
        {
            if (_predicate == null || _predicate(item))
                return (_selector(item), true);
            return default;
        }

        private void Reset(IReadOnlyDictionary<T, ((TResult? value, bool hasValue) state, int count)> items, bool isReset, bool hasCurrentItem = false, T? currentItem = default,
            (TResult? value, bool hasValue) currentValue = default)
        {
            if (isReset)
            {
                _isDirty = true;
                _isPendingReset = true;
                return;
            }

            if (items.Count == 0)
                SetValue(_defaultValue, false, false, true);
            else
            {
                var hasValue = false;
                var value = _defaultValue;
                foreach (var pair in items)
                {
                    if (!pair.Value.state.hasValue)
                        continue;

                    TResult? itemValue;
                    if (hasCurrentItem && EqualityComparer<T>.Default.Equals(pair.Key, currentItem))
                    {
                        if (!currentValue.hasValue)
                            continue;
                        itemValue = currentValue.value;
                    }
                    else
                        itemValue = pair.Value.state.value;

                    if (!hasValue)
                    {
                        value = itemValue;
                        hasValue = true;
                        continue;
                    }

                    if (CheckValue(value, itemValue))
                        value = itemValue;
                }

                SetValue(value, false, hasValue, true);
            }
        }
    }
}