using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using Should;

namespace MugenMvvm.UnitTests.Collections.Internal
{
    public class DecoratedCollectionChangeTracker<T> : AttachableComponentBase<IReadOnlyObservableCollection>, IDecoratedCollectionChangedListener, ICollectionBatchUpdateListener
    {
        private bool _hasPendingEvents;
        private bool _countRaised;
        private bool _indexerRaised;

        private int _batchCount;

        public DecoratedCollectionChangeTracker()
        {
            ChangedItems = new List<T>();
        }

        public event Action? Changed;

        public event Action? PendingChanged;

        public int ItemChangedCount { get; set; }

        public List<T> ChangedItems { get; }

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
                    items.Reset((IEnumerable<T>) sender);
                    CheckPropertyChanged(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            RaiseChanged();
        }

        public void OnBeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType == BatchUpdateType.Decorators)
                Interlocked.Increment(ref _batchCount);
        }

        public void OnEndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType == BatchUpdateType.Decorators)
            {
                Interlocked.Decrement(ref _batchCount);
                RaiseChanged(true);
            }
        }

        public void OnChanged(IReadOnlyObservableCollection collection, object? item, int index, object? args)
        {
            ChangedItems[index].ShouldEqual(item);
            ++ItemChangedCount;
            RaiseChanged();
        }

        public void OnAdded(IReadOnlyObservableCollection collection, object? item, int index)
        {
            OnAddEvent(ChangedItems, new[] {item}, index);
            RaiseChanged();
        }

        public void OnReplaced(IReadOnlyObservableCollection collection, object? oldItem, object? newItem, int index)
        {
            OnReplaceEvent(ChangedItems, new[] {oldItem}, new[] {newItem}, index);
            RaiseChanged();
        }

        public void OnMoved(IReadOnlyObservableCollection collection, object? item, int oldIndex, int newIndex)
        {
            OnMoveEvent(ChangedItems, new[] {item}, oldIndex, newIndex);
            RaiseChanged();
        }

        public void OnRemoved(IReadOnlyObservableCollection collection, object? item, int index)
        {
            OnRemoveEvent(ChangedItems, new[] {item}, index);
            RaiseChanged();
        }

        public void OnReset(IReadOnlyObservableCollection collection, IEnumerable<object?>? items)
        {
            ChangedItems.Reset(items?.Cast<T>());
            RaiseChanged();
        }

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

        private void RaiseChanged(bool fromBatch = false)
        {
            if (_batchCount == 0)
            {
                if (!fromBatch || _hasPendingEvents)
                    Changed?.Invoke();
                _hasPendingEvents = false;
            }
            else
            {
                PendingChanged?.Invoke();
                OwnerOptional?.DecoratedItems().ShouldEqual(ChangedItems.Cast<object?>());
                _hasPendingEvents = true;
            }
        }

        private void CheckPropertyChanged(bool countChanged)
        {
            _indexerRaised.ShouldBeTrue();
            if (countChanged)
                _countRaised.ShouldBeTrue();
            _indexerRaised = false;
            _countRaised = false;
        }
    }
}