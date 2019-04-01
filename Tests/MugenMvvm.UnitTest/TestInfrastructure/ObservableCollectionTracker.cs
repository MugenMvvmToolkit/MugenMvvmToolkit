using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using MugenMvvm.Interfaces.Collections;
using Should;

namespace MugenMvvm.UnitTest.TestInfrastructure
{
    public class ObservableCollectionTracker<T> : IObservableCollectionChangedListener<T>
    {
        #region Fields

        private bool _countRaised;
        private bool _indexerRaised;

        #endregion

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

        public int GetPriority(object source)
        {
            return 0;
        }

        public void OnBeginBatchUpdate(IObservableCollection<T> collection)
        {
        }

        public void OnEndBatchUpdate(IObservableCollection<T> collection)
        {
        }

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

        private void CheckPropertyChanged(bool countChanged)
        {
            _indexerRaised.ShouldBeTrue();
            if (countChanged)
                _countRaised.ShouldBeTrue();
            _indexerRaised = false;
            _countRaised = false;
        }

        private static void OnAddEvent(List<T> items, IList newItems, int index)
        {
            foreach (T newItem in newItems)
            {
                items.Insert(index, newItem);
                index++;
            }
        }

        private static void OnRemoveEvent(List<T> items, IList oldItems, int index)
        {
            if (oldItems.Count > 1)
                throw new NotSupportedException();
            items[index].ShouldEqual(oldItems[0]);
            items.RemoveAt(index);
        }

        private static void OnMoveEvent(List<T> items, IList oldItems, int oldIndex, int newIndex)
        {
            if (oldItems.Count > 1)
                throw new NotSupportedException();

            items[oldIndex].ShouldEqual(oldItems[0]);
            items.RemoveAt(oldIndex);
            items.Insert(newIndex, (T) oldItems[0]);
        }

        private static void OnReplaceEvent(List<T> items, IList oldItems, IList newItems, int index)
        {
            if (oldItems.Count > 1 || newItems.Count > 1)
                throw new NotSupportedException();
            items[index].ShouldEqual(oldItems[0]);
            items[index] = (T) newItems[0];
        }

        private void OnReset(List<T> items, IEnumerable<T> resetItems)
        {
            items.Clear();
            items.AddRange(resetItems);
        }

        public void CollectionOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Item[]")
                _indexerRaised = true;
            if (propertyChangedEventArgs.PropertyName == "Count")
                _countRaised = true;
        }

        public void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
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

        #endregion
    }
}