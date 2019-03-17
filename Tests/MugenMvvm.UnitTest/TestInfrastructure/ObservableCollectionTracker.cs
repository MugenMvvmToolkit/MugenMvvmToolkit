using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Should;

namespace MugenMvvm.UnitTest.TestInfrastructure
{
    public class ObservableCollectionTracker<T>
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

        #region Methods

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
            items[index] = (T) newItems[0];
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