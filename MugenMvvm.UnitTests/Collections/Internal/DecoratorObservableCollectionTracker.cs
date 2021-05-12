using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MugenMvvm.Interfaces.Collections.Components;
using Should;

namespace MugenMvvm.UnitTests.Collections.Internal
{
    public class DecoratorObservableCollectionTracker<T> : ICollectionDecoratorListener
    {
        private bool _countRaised;
        private bool _indexerRaised;

        public DecoratorObservableCollectionTracker()
        {
            ChangedItems = new List<T>();
        }

        public List<T> ChangedItems { get; }

        private static void OnAddEvent(List<T> items, IList? newItems, int index)
        {
            Should.NotBeNull(newItems, nameof(newItems));
            foreach (var newItem in newItems.Cast<T>())
            {
                items.Insert(index, newItem);
                index++;
            }
        }

        private static void OnRemoveEvent(List<T> items, IList? oldItems, int index)
        {
            if (oldItems == null || oldItems.Count > 1)
                throw new NotSupportedException();
            items[index]!.ShouldEqual(oldItems[0]);
            items.RemoveAt(index);
        }

        private static void OnMoveEvent(List<T> items, IList? oldItems, int oldIndex, int newIndex)
        {
            if (oldItems == null || oldItems.Count > 1)
                throw new NotSupportedException();

            items[oldIndex]!.ShouldEqual(oldItems[0]);
            items.RemoveAt(oldIndex);
            items.Insert(newIndex, (T) oldItems[0]!);
        }

        private static void OnReplaceEvent(List<T> items, IList? oldItems, IList? newItems, int index)
        {
            if (oldItems == null || newItems == null || oldItems.Count > 1 || newItems.Count > 1)
                throw new NotSupportedException();
            items[index]!.ShouldEqual(oldItems[0]);
            items[index] = (T) newItems[0]!;
        }

        public void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            Should.NotBeNull(sender, nameof(sender));
            var items = ChangedItems;
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnAddEvent(items, args.NewItems, args.NewStartingIndex);
                    CheckPropertyChanged(true);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnRemoveEvent(items, args.OldItems, args.OldStartingIndex);
                    CheckPropertyChanged(true);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    OnReplaceEvent(items, args.OldItems, args.NewItems, args.NewStartingIndex);
                    CheckPropertyChanged(false);
                    break;
                case NotifyCollectionChangedAction.Move:
                    OnMoveEvent(items, args.OldItems, args.OldStartingIndex, args.NewStartingIndex);
                    CheckPropertyChanged(false);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    OnReset(items, (IEnumerable<T>) sender);
                    CheckPropertyChanged(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnChanged(ICollection collection, object? item, int index, object? args)
        {
        }

        public void OnAdded(ICollection collection, object? item, int index) => OnAddEvent(ChangedItems, new[] {item}, index);

        public void OnReplaced(ICollection collection, object? oldItem, object? newItem, int index) => OnReplaceEvent(ChangedItems, new[] {oldItem}, new[] {newItem}, index);

        public void OnMoved(ICollection collection, object? item, int oldIndex, int newIndex) => OnMoveEvent(ChangedItems, new[] {item}, oldIndex, newIndex);

        public void OnRemoved(ICollection collection, object? item, int index) => OnRemoveEvent(ChangedItems, new[] {item}, index);

        public void OnReset(ICollection collection, IEnumerable<object?>? items) => OnReset(ChangedItems, items?.Cast<T>());

        private void CheckPropertyChanged(bool countChanged)
        {
            _indexerRaised.ShouldBeTrue();
            if (countChanged)
                _countRaised.ShouldBeTrue();
            _indexerRaised = false;
            _countRaised = false;
        }

        private void OnReset(List<T> items, IEnumerable<T>? resetItems)
        {
            items.Clear();
            if (resetItems != null)
                items.AddRange(resetItems);
        }
    }
}