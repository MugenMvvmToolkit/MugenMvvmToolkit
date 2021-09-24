using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Collections.Components
{
    public sealed class ConvertImmutableCollectionDecorator<T, TTo> : ICollectionDecorator, IHasPriority where TTo : class?
    {
        private readonly Func<object?, object?> _converter;

        public ConvertImmutableCollectionDecorator(int priority, Func<T, TTo> converter)
        {
            Should.NotBeNull(converter, nameof(converter));
            Converter = converter;
            Priority = priority;
            _converter = converter as Func<object?, object?> ?? Convert;
        }

        public Func<T, TTo> Converter { get; }

        public int Priority { get; set; }

        public bool IsLazy(IReadOnlyObservableCollection collection) => true;

        public bool IsCacheRequired(IReadOnlyObservableCollection collection) => false;

        public bool HasAdditionalItems(IReadOnlyObservableCollection collection) => true;

        private object? Convert(object? arg)
        {
            if (arg is T t)
                return Converter(t);
            return arg;
        }

        bool ICollectionDecorator.TryGetIndexes(IReadOnlyObservableCollection collection, IEnumerable<object?> items, object? item, bool ignoreDuplicates,
            ref ItemOrListEditor<int> indexes)
        {
            if (typeof(TTo) == typeof(object))
                return false;

            if (item is TTo itemTo)
            {
                var index = 0;
                foreach (var v in items)
                {
                    if (v is T t && EqualityComparer<TTo>.Default.Equals(Converter(t), itemTo))
                    {
                        indexes.Add(index);
                        if (ignoreDuplicates)
                            return true;
                    }

                    ++index;
                }
            }

            return true;
        }

        IEnumerable<object?> ICollectionDecorator.Decorate(IReadOnlyObservableCollection collection, IEnumerable<object?> items) => items.Select(_converter);

        bool ICollectionDecorator.OnChanged(IReadOnlyObservableCollection collection, ref object? item, ref int index, ref object? args)
        {
            item = _converter(item);
            return true;
        }

        bool ICollectionDecorator.OnAdded(IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            item = _converter(item);
            return true;
        }

        bool ICollectionDecorator.OnReplaced(IReadOnlyObservableCollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            oldItem = _converter(oldItem);
            newItem = _converter(newItem);
            return true;
        }

        bool ICollectionDecorator.OnMoved(IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            item = _converter(item);
            return true;
        }

        bool ICollectionDecorator.OnRemoved(IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            item = _converter(item);
            return true;
        }

        bool ICollectionDecorator.OnReset(IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            items = items?.Select(_converter);
            return true;
        }
    }
}