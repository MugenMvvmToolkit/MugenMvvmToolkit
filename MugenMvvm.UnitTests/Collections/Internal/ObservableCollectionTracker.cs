using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using Should;

namespace MugenMvvm.UnitTests.Collections.Internal
{
    public class ObservableCollectionTracker<T> : ICollectionChangedListener<T>
    {
        public ObservableCollectionTracker()
        {
            ChangedItems = new List<T>();
        }

        public int ItemChangedCount { get; set; }

        public List<T> ChangedItems { get; }

        public void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            Should.NotBeNull(sender, nameof(sender));
            Should.NotBeNull(args, nameof(args));
            var items = ChangedItems;
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnAddEvent(items, args.NewItems, args.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnRemoveEvent(items, args.OldItems, args.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    OnReplaceEvent(items, args.OldItems, args.NewItems, args.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Move:
                    OnMoveEvent(items, args.OldItems, args.OldStartingIndex, args.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    OnReset(items, (IEnumerable<T>)sender);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnChanged(IReadOnlyObservableCollection<T> collection, T item, int index, object? args)
        {
            ChangedItems[index].ShouldEqual(item);
            ++ItemChangedCount;
        }

        public void OnAdded(IReadOnlyObservableCollection<T> collection, T item, int index) => OnAddEvent(ChangedItems, new[] { item }, index);

        public void OnReplaced(IReadOnlyObservableCollection<T> collection, T oldItem, T newItem, int index) =>
            OnReplaceEvent(ChangedItems, new[] { oldItem }, new[] { newItem }, index);

        public void OnMoved(IReadOnlyObservableCollection<T> collection, T item, int oldIndex, int newIndex) => OnMoveEvent(ChangedItems, new[] { item }, oldIndex, newIndex);

        public void OnRemoved(IReadOnlyObservableCollection<T> collection, T item, int index) => OnRemoveEvent(ChangedItems, new[] { item }, index);

        public void OnReset(IReadOnlyObservableCollection<T> collection, IReadOnlyCollection<T>? items) => OnReset(ChangedItems, items);

        private static void OnAddEvent(List<T> items, IList? newItems, int index)
        {
            if (newItems == null)
                throw new NotSupportedException();
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
            items.Insert(newIndex, (T)oldItems[0]!);
        }

        private static void OnReplaceEvent(List<T> items, IList? oldItems, IList? newItems, int index)
        {
            if (oldItems == null || newItems == null || oldItems.Count > 1 || newItems.Count > 1)
                throw new NotSupportedException();
            items[index]!.ShouldEqual(oldItems[0]);
            items[index] = (T)newItems[0]!;
        }

        private void OnReset(List<T> items, IEnumerable<T>? resetItems)
        {
            items.Clear();
            if (resetItems != null)
                items.AddRange(resetItems);
        }
    }
}