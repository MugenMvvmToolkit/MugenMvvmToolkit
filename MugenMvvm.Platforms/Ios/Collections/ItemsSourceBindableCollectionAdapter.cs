using System;
using System.Collections.Generic;
using Foundation;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Ios.Interfaces;

namespace MugenMvvm.Ios.Collections
{
    public class ItemsSourceBindableCollectionAdapter : BindableCollectionAdapter, DiffUtil.ICallback, DiffUtil.IListUpdateCallback
    {
        private readonly List<object?> _beforeResetList;
        private readonly HashSet<int> _reloadIndexes;
        private Closure? _closure;
        private DiffUtil.DiffResult _diffResult;
        private bool _isInitialized;
        private int _pendingReloadCount;
        private List<(int position, int count)>? _pendingReloads;

        public ItemsSourceBindableCollectionAdapter(ICollectionViewAdapter collectionViewAdapter,
            IDiffableEqualityComparer? diffableComparer, IList<object?>? source = null, IThreadDispatcher? threadDispatcher = null)
            : base(source, threadDispatcher)
        {
            Should.NotBeNull(collectionViewAdapter, nameof(collectionViewAdapter));
            DiffableComparer = diffableComparer;
            CollectionViewAdapter = collectionViewAdapter;
            BatchSize = 2;
            _beforeResetList = new List<object?>();
            _reloadIndexes = new HashSet<int>();
        }

        public ICollectionViewAdapter CollectionViewAdapter { get; }

        public IDiffableEqualityComparer? DiffableComparer { get; }

        protected override bool IsAlive => CollectionViewAdapter.IsAlive;

        protected static void DisposeIndexPaths(NSIndexPath[] paths)
        {
            for (var i = 0; i < paths.Length; i++)
                paths[i].Dispose();
        }

        public virtual void Reload(object? item)
        {
            var index = IndexOf(item);
            if (index >= 0)
                Reload(index);
        }

        public virtual void Reload(int index)
        {
            if (_reloadIndexes.Add(index))
                AddEvent(CollectionChangedEvent.Changed(null, index, null), Version);
        }

        protected virtual NSIndexPath GetIndexPath(int index) => NSIndexPath.FromRowSection(index, 0);

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
            NotifyReload(index, 1);
            _reloadIndexes.Remove(index);
        }

        protected override void OnReset(IEnumerable<object?>? items, bool batchUpdate, int version)
        {
            BeginBatchUpdate(version);
            var closure = Closure.GetClosure(this, version);
            if (items == null || !_isInitialized)
            {
                _isInitialized = true;
                base.OnReset(items, batchUpdate, version);
                CollectionViewAdapter.ReloadData(closure.EndBatchUpdate);
                return;
            }

            _beforeResetList.AddRange(this);
            base.OnReset(items, batchUpdate, version);

            _diffResult = DiffUtil.CalculateDiff(this);
            CollectionViewAdapter.PerformUpdates(closure.PerformUpdates, closure.EndPerformUpdates);
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

        int DiffUtil.ICallback.GetOldListSize() => _beforeResetList.Count;

        int DiffUtil.ICallback.GetNewListSize() => Count;

        bool DiffUtil.ICallback.AreItemsTheSame(int oldItemPosition, int newItemPosition)
        {
            if (DiffableComparer == null)
                return Equals(_beforeResetList[oldItemPosition], this[newItemPosition]);
            return DiffableComparer.AreItemsTheSame(_beforeResetList[oldItemPosition], this[newItemPosition]);
        }

        bool DiffUtil.ICallback.AreContentsTheSame(int oldItemPosition, int newItemPosition) => !_reloadIndexes.Contains(oldItemPosition);

        void DiffUtil.IListUpdateCallback.OnInserted(int position, int finalPosition, int count) => NotifyInserted(finalPosition, count);

        void DiffUtil.IListUpdateCallback.OnRemoved(int position, int count) => NotifyDeleted(position, count);

        void DiffUtil.IListUpdateCallback.OnMoved(int fromPosition, int toPosition, int fromOriginalPosition, int toFinalPosition) =>
            NotifyMoved(fromOriginalPosition, toFinalPosition);

        void DiffUtil.IListUpdateCallback.OnChanged(int position, int finalPosition, int count, bool moved)
        {
            if (moved)
                return;

            _pendingReloads ??= new List<(int, int)>();
            _pendingReloads.Add((finalPosition, count));
            _pendingReloadCount += count;
        }

        protected sealed class Closure
        {
            private readonly ItemsSourceBindableCollectionAdapter _adapter;
            private readonly int _version;

            private Action? _endBatchUpdate;
            private Action<bool>? _endPerformUpdates;
            private Action? _performUpdates;

            private Closure(ItemsSourceBindableCollectionAdapter adapter, int version)
            {
                _adapter = adapter;
                _version = version;
            }

            public Action EndBatchUpdate => _endBatchUpdate ??= EndBatchUpdateImpl;

            public Action PerformUpdates => _performUpdates ??= PerformUpdatesIml;

            public Action<bool> EndPerformUpdates => _endPerformUpdates ??= EndPerformUpdatesImpl;

            public static Closure GetClosure(ItemsSourceBindableCollectionAdapter adapter, int version)
            {
                var closure = adapter._closure;
                if (closure == null || closure._version != version)
                {
                    closure = new Closure(adapter, version);
                    adapter._closure = closure;
                }

                return closure;
            }

            private void EndPerformUpdatesImpl(bool _)
            {
                var pendingReloads = _adapter._pendingReloads;
                if (pendingReloads != null && pendingReloads.Count != 0)
                {
                    var indexPaths = new NSIndexPath[_adapter._pendingReloadCount];
                    var index = 0;
                    for (var i = 0; i < pendingReloads.Count; i++)
                    {
                        var pendingReload = pendingReloads[i];
                        for (var j = 0; j < pendingReload.count; j++)
                            indexPaths[index++] = _adapter.GetIndexPath(pendingReload.position + j);
                    }

                    _adapter.CollectionViewAdapter.ReloadItems(indexPaths);
                    DisposeIndexPaths(indexPaths);
                    pendingReloads.Clear();
                }

                _adapter._beforeResetList.Clear();
                _adapter._reloadIndexes.Clear();
                _adapter._diffResult = default;
                _adapter._pendingReloadCount = 0;
                _adapter.EndBatchUpdate(_version);
            }

            private void PerformUpdatesIml() => _adapter._diffResult.DispatchUpdatesTo(_adapter);

            private void EndBatchUpdateImpl() => _adapter.EndBatchUpdate(_version);
        }
    }
}