﻿using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionDecorator : IComponent<IReadOnlyObservableCollection>
    {
        bool IsLazy(IReadOnlyObservableCollection collection);

        bool IsCacheRequired(IReadOnlyObservableCollection collection);
        
        bool HasAdditionalItems(IReadOnlyObservableCollection collection);

        bool TryGetIndexes(IReadOnlyObservableCollection collection, IEnumerable<object?> items, object? item, bool ignoreDuplicates, ref ItemOrListEditor<int> indexes);

        IEnumerable<object?> Decorate(IReadOnlyObservableCollection collection, IEnumerable<object?> items);

        bool OnChanged(IReadOnlyObservableCollection collection, ref object? item, ref int index, ref object? args);

        bool OnAdded(IReadOnlyObservableCollection collection, ref object? item, ref int index);

        bool OnReplaced(IReadOnlyObservableCollection collection, ref object? oldItem, ref object? newItem, ref int index);

        bool OnMoved(IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex, ref int newIndex);

        bool OnRemoved(IReadOnlyObservableCollection collection, ref object? item, ref int index);

        bool OnReset(IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items);
    }
}