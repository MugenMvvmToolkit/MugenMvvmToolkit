using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Collections.Components
{
    public sealed class ItemConverterCollectionDecorator : IAttachableComponent, ICollectionDecorator, IHasPriority
    {
        public ItemConverterCollectionDecorator(Func<object?, object?> converter, int priority = CollectionComponentPriority.ConverterDecorator)
        {
            Should.NotBeNull(converter, nameof(converter));
            Converter = converter;
            Priority = priority;
        }

        public Func<object?, object?> Converter { get; }

        public int Priority { get; set; }

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata) => true;

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata) => CollectionDecoratorManager.GetOrAdd((IEnumerable) owner);

        IEnumerable<object?> ICollectionDecorator.Decorate(ICollection collection, IEnumerable<object?> items) => items.Select(Converter);

        bool ICollectionDecorator.OnChanged(ICollection collection, ref object? item, ref int index, ref object? args)
        {
            item = Converter(item);
            return true;
        }

        bool ICollectionDecorator.OnAdded(ICollection collection, ref object? item, ref int index)
        {
            item = Converter(item);
            return true;
        }

        bool ICollectionDecorator.OnReplaced(ICollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            oldItem = Converter(oldItem);
            newItem = Converter(newItem);
            return true;
        }

        bool ICollectionDecorator.OnMoved(ICollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            item = Converter(item);
            return true;
        }

        bool ICollectionDecorator.OnRemoved(ICollection collection, ref object? item, ref int index)
        {
            item = Converter(item);
            return true;
        }

        bool ICollectionDecorator.OnReset(ICollection collection, ref IEnumerable<object?>? items)
        {
            items = items?.Select(Converter);
            return true;
        }
    }
}