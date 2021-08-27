using System;
using System.Collections.Generic;

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
        private TResult _seed;

        public AccumulateClosure(TResult seed, Func<T, TResult> selector, Func<TResult, TResult, TResult> add, Func<TResult, TResult, TResult> remove, Action<TResult> onChanged,
            Func<T, bool>? predicate = null)
        {
            _seed = seed;
            _selector = selector;
            _add = add;
            _remove = remove;
            _onChanged = onChanged;
            _predicate = predicate;
        }

        public (TResult, bool) OnAdded(IReadOnlyDictionary<T, ((TResult value, bool hasValue) state, int count)> items, T item, (TResult? value, bool hasValue) state,
            int count, bool isReset)
        {
            if (count == 1)
                state = GetValue(item);
            else if (isReset)
                state = OnChanged(items, item, state!, count - 1, true, null);

            if (state.hasValue)
                SetValue(_add(_seed, state.value!), isReset);
            return state!;
        }

        public (TResult, bool) OnRemoved(IReadOnlyDictionary<T, ((TResult value, bool hasValue) state, int count)> items, T item, (TResult value, bool hasValue) state, int count,
            bool isReset)
        {
            if (count != 0 && isReset)
                state = OnChanged(items, item, state, count + 1, isReset, null);

            if (state.hasValue)
                SetValue(_remove(_seed, state.value), isReset);
            return state;
        }

        public (TResult, bool) OnChanged(IReadOnlyDictionary<T, ((TResult value, bool hasValue) state, int count)> items, T item, (TResult value, bool hasValue) state,
            int count, bool isReset, object? args)
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

                SetValue(seed, isReset);
            }

            return newValue;
        }

        public void OnReset(IReadOnlyDictionary<T, ((TResult value, bool hasValue) state, int count)> items)
        {
            if (_isDirty)
            {
                _isDirty = false;
                _onChanged(_seed);
            }
        }

        private void SetValue(TResult value, bool isReset)
        {
            if (EqualityComparer<TResult>.Default.Equals(_seed, value))
                return;
            _seed = value;
            if (isReset)
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