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
    public sealed class AccumulateImmutableCollectionDecorator<T, TResult> : CollectionDecoratorBase, IListenerCollectionDecorator, ICollectionBatchUpdateListener
    {
        private readonly Func<T, TResult> _selector;
        private readonly Func<TResult, TResult, TResult> _add;
        private readonly Func<TResult, TResult, TResult> _remove;
        private readonly Action<TResult> _onChanged;
        private readonly Func<T, bool>? _predicate;
        private readonly bool _allowNull;
        private readonly TResult _defaultValue;
        private TResult _value;
        private bool _isInBatch;
        private bool _isDirty;

        public AccumulateImmutableCollectionDecorator(int priority, bool allowNull, TResult seed, Func<T, TResult> selector, Func<TResult, TResult, TResult> add,
            Func<TResult, TResult, TResult> remove, Action<TResult> onChanged, Func<T, bool>? predicate) : base(priority)
        {
            Should.NotBeNull(selector, nameof(selector));
            Should.NotBeNull(add, nameof(add));
            Should.NotBeNull(remove, nameof(remove));
            Should.NotBeNull(onChanged, nameof(onChanged));
            _allowNull = allowNull && TypeChecker.IsNullable<T>();
            _defaultValue = seed;
            _value = seed;
            _selector = selector;
            _add = add;
            _remove = remove;
            _onChanged = onChanged;
            _predicate = predicate;
        }

        protected override bool IsLazy => false;

        protected override bool HasAdditionalItems => false;

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => items;

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args) => true;

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (IsSatisfied(item, out var result))
                Set(_add(_value, result));
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            var value = _value;
            var hasValue = false;
            if (IsSatisfied(oldItem, out var oldResult))
            {
                value = _remove(value, oldResult);
                hasValue = true;
            }

            if (IsSatisfied(newItem, out var newResult))
            {
                value = _add(value, newResult);
                hasValue = true;
            }

            if (hasValue)
                Set(value);
            return true;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex) => true;

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (IsSatisfied(item, out var result))
                Set(_remove(_value, result));
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (items.IsNullOrEmpty())
                Set(_defaultValue);
            else
            {
                var value = _defaultValue;
                foreach (var item in items)
                {
                    if (IsSatisfied(item, out var result))
                        value = _add(value, result);
                }

                Set(value);
            }

            return true;
        }

        private void Set(TResult value, bool force = false)
        {
            if (!force && EqualityComparer<TResult>.Default.Equals(_value, value))
                return;
            _value = value;
            if (_isInBatch)
                _isDirty = true;
            else
                _onChanged(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSatisfied(object? item, [NotNullWhen(true)] out TResult? result)
        {
            if (!item.TryCast<T>(_allowNull, out var itemT) || _predicate != null && !_predicate(itemT!))
            {
                result = default;
                return false;
            }

            result = _selector(itemT!)!;
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
            if (_isDirty)
            {
                _isDirty = false;
                Set(_value, true);
            }
        }
    }
}