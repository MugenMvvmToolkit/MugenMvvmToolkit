using System;
using System.Collections.Generic;
using MugenMvvm.Collections.Components;

namespace MugenMvvm.Collections
{
    internal sealed class AccumulateClosure<T, TResult>
    {
        private readonly Func<T, TResult> _selector;
        private readonly Func<TResult, TResult, TResult> _add;
        private readonly Func<TResult, TResult, TResult> _remove;
        private readonly Action<TResult> _onChanged;
        private readonly Func<T, bool>? _predicate;
        private bool _isDirty;
        private bool _hasValue;
        private TResult _seed;

        public AccumulateClosure(TResult seed, Func<T, TResult> selector, Func<TResult, TResult, TResult> add, Func<TResult, TResult, TResult> remove, Action<TResult> onChanged,
            Func<T, bool>? predicate)
        {
            _seed = seed;
            _selector = selector;
            _add = add;
            _remove = remove;
            _onChanged = onChanged;
            _predicate = predicate;
        }

        public (TResult, bool) OnAdded(TrackerCollectionDecorator<T, (TResult value, bool hasValue)> items, T item, (TResult? value, bool hasValue) state, int count)
        {
            if (count == 1)
                state = GetValue(item);
            else if (items.IsReset)
                state = OnChanged(items, item, state!, count - 1, null);

            if (state.hasValue)
                Set(_add(_seed, state.value!), items.IsBatchUpdate);
            return state!;
        }

        public (TResult, bool) OnRemoved(TrackerCollectionDecorator<T, (TResult value, bool hasValue)> items, T item, (TResult value, bool hasValue) state, int count)
        {
            if (count != 0 && items.IsReset)
                state = OnChanged(items, item, state, count + 1, null);

            if (state.hasValue)
                Set(_remove(_seed, state.value), items.IsBatchUpdate);
            return state;
        }

        public (TResult, bool) OnChanged(TrackerCollectionDecorator<T, (TResult value, bool hasValue)> items, T item, (TResult value, bool hasValue) state, int count,
            object? args)
        {
            var newValue = GetValue(item);
            if (newValue.hasValue != state.hasValue || !EqualityComparer<TResult>.Default.Equals(newValue.value, state.value))
            {
                var seed = _seed;
                for (var i = 0; i < count; i++)
                {
                    if (state.hasValue)
                        seed = _remove(seed, state.value);
                    if (newValue.hasValue)
                        seed = _add(seed, newValue.value);
                }

                Set(seed, items.IsBatchUpdate);
            }

            return newValue;
        }

        public void OnEndBatchUpdate(TrackerCollectionDecorator<T, (TResult value, bool hasValue)> items)
        {
            if (_isDirty)
            {
                _isDirty = false;
                _onChanged(_seed);
            }
        }

        private void Set(TResult value, bool isBatch)
        {
            if (_hasValue && EqualityComparer<TResult>.Default.Equals(_seed, value))
                return;
            _hasValue = true;
            _seed = value;
            if (isBatch)
                _isDirty = true;
            else
                _onChanged(value);
        }

        private (TResult value, bool hasValue) GetValue(T item)
        {
            if (_predicate == null || _predicate(item))
                return (_selector(item), true);
            return default;
        }
    }
}