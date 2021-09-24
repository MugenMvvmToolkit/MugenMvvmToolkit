using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Collections.Components
{
    public sealed class FirstLastTrackerCollectionDecorator<T> : CollectionDecoratorBase, IListenerCollectionDecorator
    {
        private static readonly bool IsObject = typeof(T) == typeof(object);

        private readonly Action<T?> _setter;
        private readonly Func<T, bool>? _condition;
        private readonly bool _isFirstTracker;
        private T? _lastItem;
        private int _index;

        public FirstLastTrackerCollectionDecorator(int priority, bool isFirstTracker, Action<T?> setter, Func<T, bool>? condition) : base(priority)
        {
            Should.NotBeNull(setter, nameof(setter));
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
            UpdateIfNeed(item, index);
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            if (index == _index)
            {
                if (IsValid(newItem, out var newItemT))
                    Set(newItemT, index);
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
                    Set(itemT, newIndex);
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
            if (items == null)
            {
                Set(default, _isFirstTracker ? int.MaxValue : int.MinValue);
                return true;
            }

            Invalidate(items);
            return true;
        }

        private void UpdateIfNeed(object? item, int index)
        {
            if (_isFirstTracker)
            {
                if (index <= _index && IsValid(item, out var itemT))
                    Set(itemT, index);
            }
            else
            {
                if (index > _index && IsValid(item, out var itemT))
                    Set(itemT, index);
            }
        }

        private void Set(T? item, int index)
        {
            _index = index;
            if (EqualityComparer<T?>.Default.Equals(item, _lastItem))
                return;
            _lastItem = item;
            _setter(item);
        }

        private void Invalidate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection) =>
            Invalidate(decoratorManager.Decorate(collection, this));

        private void Invalidate(IEnumerable<object?> items)
        {
            if (_isFirstTracker)
                Set(FirstOrDefault(items, out var i), i);
            else
                Set(LastOrDefault(items, out var i), i);
        }

        private T? FirstOrDefault(IEnumerable<object?> items, out int index)
        {
            if (IsObject && _condition == null)
            {
                if (items is IReadOnlyList<object?> readOnlyList)
                {
                    if (readOnlyList.Count == 0)
                    {
                        index = int.MaxValue;
                        return default;
                    }

                    index = 0;
                    return (T?) readOnlyList[0];
                }

                if (items is IList<object?> list)
                {
                    if (list.Count == 0)
                    {
                        index = int.MaxValue;
                        return default;
                    }

                    index = 0;
                    return (T?) list[0];
                }
            }

            index = 0;
            foreach (var item in items)
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
            foreach (var item in items)
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

        private bool IsValid(object? item, out T? result)
        {
            if (IsObject)
            {
                result = (T?) item!;
                return IsValid(result);
            }

            if (item is T itemT && IsValid(itemT))
            {
                result = itemT;
                return true;
            }

            result = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValid(T item) => _condition == null || _condition(item);
    }
}