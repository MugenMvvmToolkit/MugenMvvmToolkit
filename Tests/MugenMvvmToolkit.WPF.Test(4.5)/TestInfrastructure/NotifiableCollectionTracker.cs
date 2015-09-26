using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Models.EventArg;
using Should;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class NotifiableCollectionTracker<T>
    {
        #region Fields

        private readonly SynchronizedNotifiableCollection<T> _collection;
        private readonly List<T> _changingItems;
        private readonly List<T> _changedItems;

        private bool _countRaised;
        private bool _indexerRaised;

        #endregion

        #region Constructors

        public NotifiableCollectionTracker(SynchronizedNotifiableCollection<T> collection)
        {
            _collection = collection;
            collection.CollectionChanging += CollectionOnCollectionChanging;
            collection.CollectionChanged += CollectionOnCollectionChanged;
            collection.PropertyChanged += CollectionOnPropertyChanged;

            _changingItems = new List<T>(_collection);
            _changedItems = new List<T>(_collection);
        }

        private void CollectionOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName ==
                Empty.IndexerPropertyChangedArgs.PropertyName)
                _indexerRaised = true;
            if (propertyChangedEventArgs.PropertyName ==
                Empty.CountChangedArgs.PropertyName)
                _countRaised = true;
        }

        #endregion

        #region Properties

        public List<T> ChangingItems
        {
            get { return _changingItems; }
        }

        public List<T> ChangedItems
        {
            get { return _changedItems; }
        }

        public bool CountRaised
        {
            get { return _countRaised; }
        }

        public bool IndexerRaised
        {
            get { return _indexerRaised; }
        }

        #endregion

        #region Methods

        public void AssertChangedEquals()
        {
            _collection.Count.ShouldEqual(ChangedItems.Count, "Changed items not equals.");
            _collection.SequenceEqual(ChangedItems).ShouldBeTrue("Changed items not equals.");
        }

        public void AssertChangingEquals()
        {
            _collection.Count.ShouldEqual(ChangingItems.Count, "Changing items not equals.");
            _collection.SequenceEqual(ChangingItems).ShouldBeTrue("Changing items not equals.");
        }

        public void AssertEquals()
        {
            AssertChangingEquals();
            AssertChangedEquals();
        }

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

        private static void OnReplaceEvent(List<T> items, IList oldItems, IList newItems, int index)
        {
            if (oldItems.Count > 1 || newItems.Count > 1)
                throw new NotSupportedException();
            items[index].ShouldEqual(oldItems[0]);
            items[index] = (T)newItems[0];
        }

        private void OnReset(IList<T> items)
        {
            items.Clear();
            items.AddRange(_collection);
        }

        private void CollectionOnCollectionChanging(object sender, NotifyCollectionChangingEventArgs args)
        {
            List<T> items = ChangingItems;
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
                case NotifyCollectionChangedAction.Reset:
                    OnReset(items);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            List<T> items = ChangedItems;
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
                case NotifyCollectionChangedAction.Reset:
                    OnReset(items);
                    CheckPropertyChanged(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _changedItems.Count.ShouldEqual(_collection.NotificationCount);
        }

        #endregion
    }
}
