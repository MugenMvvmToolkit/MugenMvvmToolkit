using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using Should;

namespace MugenMvvm.UnitTest.Collections.Internal
{
    public class ObservableCollectionTracker<TItem> : ICollectionChangedListener
    {
        #region Fields

        private bool _countRaised;
        private bool _indexerRaised;

        #endregion

        #region Constructors

        public ObservableCollectionTracker()
        {
            ChangedItems = new List<TItem>();
        }

        #endregion

        #region Properties

        public List<TItem> ChangedItems { get; }

        #endregion

        #region Implementation of interfaces

        public void OnItemChanged<T>(IObservableCollection<T> collection, T item, int index, object? args)
        {
        }

        public void OnAdded<T>(IObservableCollection<T> collection, T item, int index)
        {
            OnAddEvent(ChangedItems, new[] {item}, index);
        }

        public void OnReplaced<T>(IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            OnReplaceEvent(ChangedItems, new[] {oldItem}, new[] {newItem}, index);
        }

        public void OnMoved<T>(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            OnMoveEvent(ChangedItems, new[] {item}, oldIndex, newIndex);
        }

        public void OnRemoved<T>(IObservableCollection<T> collection, T item, int index)
        {
            OnRemoveEvent(ChangedItems, new[] {item}, index);
        }

        public void OnReset<T>(IObservableCollection<T> collection, IEnumerable<T> items)
        {
            OnReset(ChangedItems, items);
        }

        public void OnCleared<T>(IObservableCollection<T> collection)
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

        private static void OnAddEvent(List<TItem> items, IList newItems, int index)
        {
            foreach (TItem newItem in newItems.Cast<TItem>())
            {
                items.Insert(index, newItem);
                index++;
            }
        }

        private static void OnRemoveEvent(List<TItem> items, IList oldItems, int index)
        {
            if (oldItems.Count > 1)
                throw new NotSupportedException();
            items[index]!.ShouldEqual(oldItems[0]);
            items.RemoveAt(index);
        }

        private static void OnMoveEvent(List<TItem> items, IList oldItems, int oldIndex, int newIndex)
        {
            if (oldItems.Count > 1)
                throw new NotSupportedException();

            items[oldIndex]!.ShouldEqual(oldItems[0]);
            items.RemoveAt(oldIndex);
            items.Insert(newIndex, (TItem) oldItems[0]!);
        }

        private static void OnReplaceEvent(List<TItem> items, IList oldItems, IList newItems, int index)
        {
            if (oldItems.Count > 1 || newItems.Count > 1)
                throw new NotSupportedException();
            items[index]!.ShouldEqual(oldItems[0]);
            items[index] = (TItem) newItems[0]!;
        }

        private void OnReset<T>(List<TItem> items, IEnumerable<T> resetItems)
        {
            items.Clear();
            items.AddRange(resetItems.Cast<TItem>());
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
                    OnReset(items, (IEnumerable<TItem>) sender);
                    CheckPropertyChanged(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}