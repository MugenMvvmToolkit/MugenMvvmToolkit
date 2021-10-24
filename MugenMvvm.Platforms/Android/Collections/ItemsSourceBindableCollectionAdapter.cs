using System;
using System.Collections.Generic;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Metadata;

namespace MugenMvvm.Android.Collections
{
    public class ItemsSourceBindableCollectionAdapter : BindableCollectionAdapter, DiffUtil.ICallback, DiffUtil.IListUpdateCallback
    {
        protected readonly List<IItemsSourceObserver> Observers;
        private IReadOnlyList<object?>? _resetItems;
        private Dictionary<(int, object?), object?>? _changedItems;
        private int _diffSupportedCount;
        private int _diffVersion;
        private bool _isAlive;
        private int? _diffUtilAsyncThreshold;
        private int? _diffUtilMaxThreshold;

        public ItemsSourceBindableCollectionAdapter(IDiffableEqualityComparer? diffableComparer, IList<object?>? source = null, IThreadDispatcher? threadDispatcher = null)
            : base(source, threadDispatcher)
        {
            DiffableComparer = diffableComparer;
            Observers = new List<IItemsSourceObserver>();
            _isAlive = true;
        }

        [Preserve]
        public IDiffableEqualityComparer? DiffableComparer { get; set; }

        [Preserve]
        public bool DetectMoves { get; set; } = true;

        [Preserve]
        public int DiffUtilAsyncThreshold
        {
            get => _diffUtilAsyncThreshold.GetValueOrDefault(CollectionMetadata.DiffUtilAsyncThreshold);
            set => _diffUtilAsyncThreshold = value;
        }

        [Preserve]
        public int DiffUtilMaxThreshold
        {
            get => _diffUtilMaxThreshold.GetValueOrDefault(CollectionMetadata.DiffUtilMaxThreshold);
            set => _diffUtilMaxThreshold = value;
        }

        protected override bool IsAlive => _isAlive;

        public void AddObserver(IItemsSourceObserver observer)
        {
            Should.NotBeNull(observer, nameof(observer));
            Observers.Add(observer);
            if (observer.IsDiffUtilSupported)
                ++_diffSupportedCount;
        }

        public void RemoveObserver(IItemsSourceObserver observer)
        {
            Should.NotBeNull(observer, nameof(observer));
            Observers.Remove(observer);
            if (observer.IsDiffUtilSupported)
                --_diffSupportedCount;
        }

        protected override void OnAdded(object? item, int index, bool batchUpdate, int version)
        {
            base.OnAdded(item, index, batchUpdate, version);
            for (var i = 0; i < Observers.Count; i++)
                GetObserver(i)?.OnItemInserted(index);
        }

        protected override void OnMoved(object? item, int oldIndex, int newIndex, bool batchUpdate, int version)
        {
            base.OnMoved(item, oldIndex, newIndex, batchUpdate, version);
            for (var i = 0; i < Observers.Count; i++)
                GetObserver(i)?.OnItemMoved(oldIndex, newIndex);
        }

        protected override void OnRemoved(object? item, int index, bool batchUpdate, int version)
        {
            base.OnRemoved(item, index, batchUpdate, version);
            for (var i = 0; i < Observers.Count; i++)
                GetObserver(i)?.OnItemRemoved(index);
        }

        protected override void OnReplaced(object? oldItem, object? newItem, int index, bool batchUpdate, int version)
        {
            base.OnReplaced(oldItem, newItem, index, batchUpdate, version);
            for (var i = 0; i < Observers.Count; i++)
                GetObserver(i)?.OnItemChanged(index);
        }

        protected override void OnChanged(object? item, int index, object? args, bool batchUpdate, int version)
        {
            if (CollectionMetadata.ReloadItem == args)
            {
                for (var i = 0; i < Observers.Count; i++)
                    GetObserver(i)?.OnItemChanged(index);
            }
        }

        protected override async void OnReset(IEnumerable<object?>? items, Dictionary<(int index, object? args), object?>? changedItems, bool batchUpdate, int version)
        {
            if (items != null && Items.Count != 0 && _diffSupportedCount > 0 && Items.Count <= DiffUtilMaxThreshold)
            {
                if (items is IReadOnlyList<object?> list)
                    _resetItems = list;
                else
                {
                    MugenExtensions.Reset(ref ResetCache, items);
                    _resetItems = ResetCache;
                }

                if (Items.Count + _resetItems.Count <= DiffUtilMaxThreshold)
                {
                    _changedItems = changedItems;
                    var isAsync = Items.Count + _resetItems.Count > DiffUtilAsyncThreshold;
                    if (!batchUpdate && isAsync && !ReferenceEquals(_resetItems, ResetCache))
                    {
                        MugenExtensions.Reset(ref ResetCache, _resetItems);
                        _resetItems = ResetCache;
                    }

                    if (isAsync)
                        BeginBatchUpdate(version);

                    try
                    {
                        _diffVersion = version;
                        var diffResult = await DiffUtil.CalculateDiffAsync(this, isAsync, DetectMoves);
                        if (Version == version)
                        {
                            base.OnReset(_resetItems, changedItems, batchUpdate, version);
                            diffResult.DispatchUpdatesTo(this, true);
                            ClearResetCache();
                            if (isAsync)
                                EndBatchUpdate(version);
                        }
                    }
                    catch
                    {
                        if (Version == version)
                            throw;
                    }

                    return;
                }

                base.OnReset(_resetItems, changedItems, batchUpdate, version);
                ClearResetCache();
            }
            else
            {
                base.OnReset(items, changedItems, batchUpdate, version);
                ClearResetCache();
            }

            for (var i = 0; i < Observers.Count; i++)
                GetObserver(i)?.OnReset();
        }

        protected override void ClearResetCache()
        {
            base.ClearResetCache();
            _resetItems = null;
            _changedItems = null;
        }

        protected override void RaiseBatchUpdate(List<CollectionChangedEvent> events, int version)
        {
            var callback = new DiffUtil.BatchingListUpdateCallback(this, true);
            for (var i = 0; i < events.Count; i++)
            {
                var e = events[i];
                if (e.Action != CollectionChangedAction.Changed || e.ChangedArgs == CollectionMetadata.ReloadItem)
                    e.Raise(Items, ref callback);
            }

            callback.DispatchLastEvent();
        }

        protected IItemsSourceObserver? GetObserver(int index)
        {
            var observer = Observers[index];
            if (observer.Handle == IntPtr.Zero)
            {
                _isAlive = false;
                return null;
            }

            return observer;
        }

        int DiffUtil.ICallback.GetOldListSize()
        {
            if (_diffVersion != Version)
                return 0;
            return Items.Count;
        }

        int DiffUtil.ICallback.GetNewListSize()
        {
            if (_diffVersion != Version)
                return 0;
            return _resetItems!.Count;
        }

        bool DiffUtil.ICallback.AreItemsTheSame(int oldItemPosition, int newItemPosition)
        {
            if (_diffVersion != Version)
                return true;
            if (DiffableComparer == null)
                return Equals(Items[oldItemPosition], _resetItems![newItemPosition]);
            return DiffableComparer.AreItemsTheSame(Items[oldItemPosition], _resetItems![newItemPosition]);
        }

        bool DiffUtil.ICallback.AreContentsTheSame(int oldItemPosition, int newItemPosition)
        {
            if (_diffVersion != Version)
                return true;
            if (!ReferenceEquals(Items[oldItemPosition], _resetItems![newItemPosition]))
                return false;
            
            var changedItems = _changedItems;
            return changedItems == null || !changedItems.ContainsKey((oldItemPosition, CollectionMetadata.ReloadItem));
        }

        void DiffUtil.IListUpdateCallback.OnInserted(int position, int finalPosition, int count)
        {
            for (var i = 0; i < Observers.Count; i++)
                GetObserver(i)?.OnItemRangeInserted(position, count);
        }

        void DiffUtil.IListUpdateCallback.OnRemoved(int position, int count)
        {
            for (var i = 0; i < Observers.Count; i++)
                GetObserver(i)?.OnItemRangeRemoved(position, count);
        }

        void DiffUtil.IListUpdateCallback.OnMoved(int fromPosition, int toPosition, int fromOriginalPosition, int toFinalPosition)
        {
            for (var i = 0; i < Observers.Count; i++)
                GetObserver(i)?.OnItemMoved(fromPosition, toPosition);
        }

        void DiffUtil.IListUpdateCallback.OnChanged(int position, int finalPosition, int count, bool isMove)
        {
            for (var i = 0; i < Observers.Count; i++)
                GetObserver(i)?.OnItemRangeChanged(position, count);
        }
    }
}