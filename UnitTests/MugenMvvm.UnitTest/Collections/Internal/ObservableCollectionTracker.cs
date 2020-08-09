using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using Should;

namespace MugenMvvm.UnitTest.Collections.Internal
{
    public class ObservableCollectionTracker<T> : ICollectionChangedListener<T>
    {
        #region Constructors

        public ObservableCollectionTracker()
        {
            ChangedItems = new List<T>();
        }

        #endregion

        #region Properties

        public List<T> ChangedItems { get; }

        #endregion

        #region Implementation of interfaces

        public void OnItemChanged(IObservableCollection<T> collection, T item, int index, object? args)
        {
        }

        public void OnAdded(IObservableCollection<T> collection, T item, int index)
        {
            OnAddEvent(ChangedItems, new[] {item}, index);
        }

        public void OnReplaced(IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            OnReplaceEvent(ChangedItems, new[] {oldItem}, new[] {newItem}, index);
        }

        public void OnMoved(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            OnMoveEvent(ChangedItems, new[] {item}, oldIndex, newIndex);
        }

        public void OnRemoved(IObservableCollection<T> collection, T item, int index)
        {
            OnRemoveEvent(ChangedItems, new[] {item}, index);
        }

        public void OnReset(IObservableCollection<T> collection, IEnumerable<T> items)
        {
            OnReset(ChangedItems, items);
        }

        public void OnCleared(IObservableCollection<T> collection)
        {
            OnReset(ChangedItems, Enumerable.Empty<T>());
        }

        #endregion

        #region Methods

        private static void OnAddEvent(List<T> items, IList newItems, int index)
        {
            foreach (var newItem in newItems.Cast<T>())
            {
                items.Insert(index, newItem);
                index++;
            }
        }

        private static void OnRemoveEvent(List<T> items, IList oldItems, int index)
        {
            if (oldItems.Count > 1)
                throw new NotSupportedException();
            items[index]!.ShouldEqual(oldItems[0]);
            items.RemoveAt(index);
        }

        private static void OnMoveEvent(List<T> items, IList oldItems, int oldIndex, int newIndex)
        {
            if (oldItems.Count > 1)
                throw new NotSupportedException();

            items[oldIndex]!.ShouldEqual(oldItems[0]);
            items.RemoveAt(oldIndex);
            items.Insert(newIndex, (T) oldItems[0]!);
        }

        private static void OnReplaceEvent(List<T> items, IList oldItems, IList newItems, int index)
        {
            if (oldItems.Count > 1 || newItems.Count > 1)
                throw new NotSupportedException();
            items[index]!.ShouldEqual(oldItems[0]);
            items[index] = (T) newItems[0]!;
        }

        private void OnReset(List<T> items, IEnumerable<T> resetItems)
        {
            items.Clear();
            items.AddRange(resetItems);
        }

        public void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
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
                    OnReset(items, (IEnumerable<T>) sender);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}