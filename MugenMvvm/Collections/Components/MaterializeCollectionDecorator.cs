using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Collections.Components
{
    internal sealed class MaterializeCollectionDecorator : ICollectionDecorator, IHasPriority
    {
        public MaterializeCollectionDecorator(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; }

        public bool IsLazy(IReadOnlyObservableCollection collection) => false;

        public bool IsCacheRequired(IReadOnlyObservableCollection collection) => true;

        public bool HasAdditionalItems(IReadOnlyObservableCollection collection) => false;

        public bool TryGetIndexes(IReadOnlyObservableCollection collection, IEnumerable<object?> items, object? item, bool ignoreDuplicates, ref ItemOrListEditor<int> indexes) =>
            false;

        public IEnumerable<object?> Decorate(IReadOnlyObservableCollection collection, IEnumerable<object?> items) => items;

        public bool OnChanged(IReadOnlyObservableCollection collection, ref object? item, ref int index, ref object? args) => true;

        public bool OnAdded(IReadOnlyObservableCollection collection, ref object? item, ref int index) => true;

        public bool OnReplaced(IReadOnlyObservableCollection collection, ref object? oldItem, ref object? newItem, ref int index) => true;

        public bool OnMoved(IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex, ref int newIndex) => true;

        public bool OnRemoved(IReadOnlyObservableCollection collection, ref object? item, ref int index) => true;

        public bool OnReset(IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items) => true;
    }
}