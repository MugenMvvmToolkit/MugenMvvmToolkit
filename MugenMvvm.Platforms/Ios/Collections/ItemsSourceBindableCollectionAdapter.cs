using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Foundation;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Ios.Interfaces;
using MugenMvvm.Metadata;

namespace MugenMvvm.Ios.Collections
{
    public class ItemsSourceBindableCollectionAdapter : BindableCollectionAdapter, DiffUtil.ICallback, DiffUtil.IListUpdateCallback
    {
        private IReadOnlyList<object?>? _resetItems;
        private Dictionary<(int, object?), object?>? _changedItems;
        private Closure? _closure;
        private DiffUtil.DiffResult _diffResult;
        private bool _isInitialized;
        private int _pendingReloadCount;
        private List<(int position, int count)>? _pendingReloads;
        private int? _diffUtilAsyncLimit;
        private int? _diffUtilMaxLimit;

        public ItemsSourceBindableCollectionAdapter(ICollectionViewAdapter collectionViewAdapter,
            IDiffableEqualityComparer? diffableComparer, IList<object?>? source = null, IThreadDispatcher? threadDispatcher = null)
            : base(source, threadDispatcher)
        {
            Should.NotBeNull(collectionViewAdapter, nameof(collectionViewAdapter));
            DiffableComparer = diffableComparer;
            CollectionViewAdapter = collectionViewAdapter;
        }

        public ICollectionViewAdapter CollectionViewAdapter { get; }

        [Preserve]
        public IDiffableEqualityComparer? DiffableComparer { get; set; }

        [Preserve]
        public bool DetectMoves { get; set; } = true;

        [Preserve]
        public int DiffUtilAsyncLimit
        {
            get => _diffUtilAsyncLimit.GetValueOrDefault(CollectionMetadata.DiffUtilAsyncLimit);
            set => _diffUtilAsyncLimit = value;
        }

        [Preserve]
        public int DiffUtilMaxLimit
        {
            get => _diffUtilMaxLimit.GetValueOrDefault(CollectionMetadata.DiffUtilMaxLimit);
            set => _diffUtilMaxLimit = value;
        }

        protected override bool IsAlive => CollectionViewAdapter.IsAlive;

        public void Reload(object? item)
        {
            var index = IndexOf(item);
            if (index >= 0)
                AddEvent(CollectionChangedEvent.Changed(item, index, CollectionMetadata.ReloadItem), Version);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void DisposeIndexPaths(NSIndexPath[] paths)
        {
            foreach (var t in paths)
                t.Dispose();
        }

        protected virtual NSIndexPath GetIndexPath(int index) => NSIndexPath.FromRowSection(index, 0);

        protected override void ClearResetCache()
        {
            base.ClearResetCache();
            _pendingReloads?.Clear();
            _changedItems = null;
            _diffResult = default;
            _resetItems = null;
            _pendingReloadCount = 0;
        }

        protected override void OnAdded(object? item, int index, bool batchUpdate, int version)
        {
            base.OnAdded(item, index, batchUpdate, version);
            NotifyInserted(index, 1);
        }

        protected override void OnMoved(object? item, int oldIndex, int newIndex, bool batchUpdate, int version)
        {
            base.OnMoved(item, oldIndex, newIndex, batchUpdate, version);
            NotifyMoved(oldIndex, newIndex);
        }

        protected override void OnRemoved(object? item, int index, bool batchUpdate, int version)
        {
            base.OnRemoved(item, index, batchUpdate, version);
            NotifyDeleted(index, 1);
        }

        protected override void OnReplaced(object? oldItem, object? newItem, int index, bool batchUpdate, int version)
        {
            base.OnReplaced(oldItem, newItem, index, batchUpdate, version);
            NotifyReload(index, 1);
        }

        protected override void OnChanged(object? item, int index, object? args, bool batchUpdate, int version)
        {
            if (CollectionMetadata.ReloadItem == args)
                NotifyReload(index, 1);
        }

        protected override async void OnReset(IEnumerable<object?>? items, Dictionary<(int, object?), object?>? changedItems, bool batchUpdate, int version)
        {
            BeginBatchUpdate(version);
            var closure = Closure.GetClosure(this, version);
            if (items == null || !_isInitialized || Items.Count == 0 || Items.Count > DiffUtilMaxLimit)
            {
                _isInitialized = true;
                Reload(closure, items, changedItems, batchUpdate, version);
                return;
            }

            if (items is IReadOnlyList<object?> list)
                _resetItems = list;
            else
            {
                MugenExtensions.Reset(ref ResetCache, items);
                _resetItems = ResetCache;
            }

            if (Items.Count + _resetItems.Count > DiffUtilMaxLimit)
            {
                Reload(closure, _resetItems, changedItems, batchUpdate, version);
                return;
            }

            _changedItems = changedItems;
            var isAsync = Items.Count + _resetItems.Count > DiffUtilAsyncLimit;
            if (!batchUpdate && isAsync && !ReferenceEquals(_resetItems, ResetCache))
            {
                MugenExtensions.Reset(ref ResetCache, _resetItems);
                _resetItems = ResetCache;
            }

            try
            {
                var diffResult = await DiffUtil.CalculateDiffAsync(this, isAsync, DetectMoves);
                if (Version == version)
                {
                    _diffResult = diffResult;
                    CollectionViewAdapter.PerformUpdates(closure.PerformUpdates, closure.EndPerformUpdates);
                }
            }
            catch
            {
                if (Version == version)
                    throw;
            }
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

        protected NSIndexPath[] GetIndexPaths(int startingPosition, int count)
        {
            var indexPaths = new NSIndexPath[count];
            for (var i = 0; i < count; i++)
                indexPaths[i] = GetIndexPath(i + startingPosition);
            return indexPaths;
        }

        protected void NotifyInserted(int index, int count)
        {
            var indexPaths = GetIndexPaths(index, count);
            CollectionViewAdapter.InsertItems(indexPaths);
            DisposeIndexPaths(indexPaths);
        }

        protected void NotifyDeleted(int index, int count)
        {
            var indexPaths = GetIndexPaths(index, count);
            CollectionViewAdapter.DeleteItems(indexPaths);
            DisposeIndexPaths(indexPaths);
        }

        protected void NotifyMoved(int oldIndex, int newIndex)
        {
            var oldIndexPath = GetIndexPath(oldIndex);
            var newIndexPath = GetIndexPath(newIndex);
            CollectionViewAdapter.MoveItem(oldIndexPath, newIndexPath);
            oldIndexPath.Dispose();
            newIndexPath.Dispose();
        }

        protected void NotifyReload(int index, int count)
        {
            var indexPaths = GetIndexPaths(index, count);
            CollectionViewAdapter.ReloadItems(indexPaths);
            DisposeIndexPaths(indexPaths);
        }

        private void ResetBase(IEnumerable<object?>? items, Dictionary<(int, object?), object?>? changedItems, bool batchUpdate, int version) =>
            base.OnReset(items, changedItems, batchUpdate, version);

        private void Reload(Closure closure, IEnumerable<object?>? items, Dictionary<(int, object?), object?>? changedItems, bool batchUpdate, int version)
        {
            base.OnReset(items, changedItems, batchUpdate, version);
            ClearResetCache();
            CollectionViewAdapter.ReloadData(closure.EndBatchUpdate);
        }

        int DiffUtil.ICallback.GetOldListSize()
        {
            if (_closure == null || _closure.Version != Version)
                return 0;
            return Items.Count;
        }

        int DiffUtil.ICallback.GetNewListSize()
        {
            if (_closure == null || _closure.Version != Version)
                return 0;
            return _resetItems!.Count;
        }

        bool DiffUtil.ICallback.AreItemsTheSame(int oldItemPosition, int newItemPosition)
        {
            if (_closure == null || _closure.Version != Version)
                return true;

            if (DiffableComparer == null)
                return Equals(Items[oldItemPosition], _resetItems![newItemPosition]);
            return DiffableComparer.AreItemsTheSame(Items[oldItemPosition], _resetItems![newItemPosition]);
        }

        bool DiffUtil.ICallback.AreContentsTheSame(int oldItemPosition, int newItemPosition)
        {
            if (_closure == null || _closure.Version != Version)
                return true;

            var changedItems = _changedItems;
            return changedItems == null || !changedItems.ContainsKey((oldItemPosition, CollectionMetadata.ReloadItem));
        }

        void DiffUtil.IListUpdateCallback.OnInserted(int position, int finalPosition, int count) => NotifyInserted(finalPosition, count);

        void DiffUtil.IListUpdateCallback.OnRemoved(int position, int count) => NotifyDeleted(position, count);

        void DiffUtil.IListUpdateCallback.OnMoved(int fromPosition, int toPosition, int fromOriginalPosition, int toFinalPosition) =>
            NotifyMoved(fromOriginalPosition, toFinalPosition);

        void DiffUtil.IListUpdateCallback.OnChanged(int position, int finalPosition, int count, bool isMove)
        {
            if (_diffResult.IsEmpty || position == finalPosition && !isMove)
                NotifyReload(finalPosition, count);
            else
            {
                _pendingReloads ??= new List<(int, int)>();
                _pendingReloads.Add((finalPosition, count));
                _pendingReloadCount += count;
            }
        }

        protected sealed class Closure
        {
            public readonly int Version;
            private readonly ItemsSourceBindableCollectionAdapter _adapter;

            private Action? _endBatchUpdate;
            private Action<bool>? _endPerformUpdates;
            private Action? _performUpdates;
            private Action? _performReloads;
            private NSIndexPath[]? _reloadPaths;

            private Closure(ItemsSourceBindableCollectionAdapter adapter, int version)
            {
                _adapter = adapter;
                Version = version;
            }

            public Action EndBatchUpdate => _endBatchUpdate ??= EndBatchUpdateImpl;

            public Action PerformUpdates => _performUpdates ??= PerformUpdatesIml;

            public Action<bool> EndPerformUpdates => _endPerformUpdates ??= EndPerformUpdatesImpl;

            private Action PerformReloads => _performReloads ??= PerformReloadsIml;

            private bool IsValidVersion => Version == _adapter.Version;

            public static Closure GetClosure(ItemsSourceBindableCollectionAdapter adapter, int version)
            {
                var closure = adapter._closure;
                if (closure == null || closure.Version != version)
                {
                    closure = new Closure(adapter, version);
                    adapter._closure = closure;
                }

                return closure;
            }

            private void EndPerformUpdatesImpl(bool _)
            {
                if (!IsValidVersion)
                    return;

                var pendingReloads = _adapter._pendingReloads;
                if (pendingReloads != null && pendingReloads.Count != 0)
                {
                    _reloadPaths = new NSIndexPath[_adapter._pendingReloadCount];
                    var index = 0;
                    for (var i = 0; i < pendingReloads.Count; i++)
                    {
                        var pendingReload = pendingReloads[i];
                        for (var j = 0; j < pendingReload.count; j++)
                            _reloadPaths[index++] = _adapter.GetIndexPath(pendingReload.position + j);
                    }

                    pendingReloads.Clear();
                    _adapter.CollectionViewAdapter.PerformUpdates(PerformReloads, EndPerformUpdates);
                    return;
                }

                _adapter.ClearResetCache();
                _adapter.EndBatchUpdate(Version);
            }

            private void PerformUpdatesIml()
            {
                if (IsValidVersion)
                {
                    _adapter.ResetBase(_adapter._resetItems, null, true, Version);
                    _adapter._diffResult.DispatchUpdatesTo(_adapter);
                }
            }

            private void PerformReloadsIml()
            {
                if (_reloadPaths == null)
                    return;

                if (IsValidVersion)
                    _adapter.CollectionViewAdapter.ReloadItems(_reloadPaths);

                DisposeIndexPaths(_reloadPaths);
                _reloadPaths = null;
            }

            private void EndBatchUpdateImpl()
            {
                if (IsValidVersion)
                    _adapter.EndBatchUpdate(Version);
            }
        }
    }
}