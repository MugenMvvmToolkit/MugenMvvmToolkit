using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Collections.Components
{
    public sealed class ImmutableItemConverterCollectionDecorator : ICollectionDecorator, IHasPriority //todo generic
    {
        public ImmutableItemConverterCollectionDecorator(Func<object?, object?> converter, int priority = CollectionComponentPriority.ConverterDecorator)
        {
            Should.NotBeNull(converter, nameof(converter));
            Converter = converter;
            Priority = priority;
        }

        public Func<object?, object?> Converter { get; }

        public bool HasAdditionalItems => true;

        public int Priority { get; set; }

        bool ICollectionDecorator.TryGetIndex(IReadOnlyObservableCollection collection, object item, out int index)
        {
            index = -1;
            return false;
        }

        IEnumerable<object?> ICollectionDecorator.Decorate(IReadOnlyObservableCollection collection, IEnumerable<object?> items) => items.Select(Converter);

        bool ICollectionDecorator.OnChanged(IReadOnlyObservableCollection collection, ref object? item, ref int index, ref object? args)
        {
            item = Converter(item);
            return true;
        }

        bool ICollectionDecorator.OnAdded(IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            item = Converter(item);
            return true;
        }

        bool ICollectionDecorator.OnReplaced(IReadOnlyObservableCollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            oldItem = Converter(oldItem);
            newItem = Converter(newItem);
            return true;
        }

        bool ICollectionDecorator.OnMoved(IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            item = Converter(item);
            return true;
        }

        bool ICollectionDecorator.OnRemoved(IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            item = Converter(item);
            return true;
        }

        bool ICollectionDecorator.OnReset(IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            items = items?.Select(Converter);
            return true;
        }
    }
}