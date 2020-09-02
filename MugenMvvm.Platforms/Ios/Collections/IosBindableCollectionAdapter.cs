using System;
using System.Collections.Generic;
using Foundation;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Internal;
using MugenMvvm.Ios.Interfaces;

namespace MugenMvvm.Ios.Collections
{
    public class IosBindableCollectionAdapter : BindableCollectionAdapter, DiffUtil.ICallback, DiffUtil.IListUpdateCallback
    {
        #region Fields

        private readonly List<object?> _beforeResetList;
        private readonly List<int> _reloadIndexes;
        private Closure? _closure;
        private DiffUtil.DiffResult _diffResult;
        private bool _isInitialized;

        #endregion

        #region Constructors

        public IosBindableCollectionAdapter(ICollectionViewAdapter collectionViewAdapter, IItemsSourceEqualityComparer? equalityComparer = null, IList<object?>? source = null, IThreadDispatcher? threadDispatcher = null)
            : base(source, threadDispatcher)
        {
            Should.NotBeNull(collectionViewAdapter, nameof(collectionViewAdapter));
            EqualityComparer = equalityComparer;
            CollectionViewAdapter = collectionViewAdapter;
            BatchSize = 2;
            _beforeResetList = new List<object?>();
            _reloadIndexes = new List<int>();
        }

        #endregion

        #region Properties

        public ICollectionViewAdapter CollectionViewAdapter { get; }

        public IItemsSourceEqualityComparer? EqualityComparer { get; }

        bool DiffUtil.IListUpdateCallback.IsUseFinalPosition => true;

        protected override bool IsAlive => CollectionViewAdapter.IsAlive;

        #endregion

        #region Implementation of interfaces

        int DiffUtil.ICallback.GetOldListSize() => _beforeResetList.Count;

        int DiffUtil.ICallback.GetNewListSize() => Count;

        bool DiffUtil.ICallback.AreItemsTheSame(int oldItemPosition, int newItemPosition)
        {
            if (EqualityComparer == null)
                return Equals(_beforeResetList[oldItemPosition], this[newItemPosition]);
            return EqualityComparer.AreItemsTheSame(_beforeResetList[oldItemPosition], this[newItemPosition]);
        }

        bool DiffUtil.ICallback.AreContentsTheSame(int oldItemPosition, int newItemPosition) => !_reloadIndexes.Contains(oldItemPosition);

        void DiffUtil.IListUpdateCallback.OnInserted(int position, int count) => NotifyInserted(position, count);

        void DiffUtil.IListUpdateCallback.OnRemoved(int position, int count) => NotifyDeleted(position, count);

        void DiffUtil.IListUpdateCallback.OnMoved(int fromPosition, int toPosition) => NotifyMoved(fromPosition, toPosition);

        void DiffUtil.IListUpdateCallback.OnChanged(int position, int count, bool moved)
        {
            if (moved)
                return;

            for (var i = 0; i < count; i++)
            {
                if (position >= _beforeResetList.Count)
                    break;

                if (_diffResult.ConvertOldPositionToNew(position) == position)
                    NotifyReload(position, 1);

                ++position;
            }
        }

        #endregion

        #region Methods

        public virtual void Reload(object? item)
        {
            var index = IndexOf(item);
            if (index >= 0)
                Reload(index);
        }

        public virtual void Reload(int index)
        {
            _reloadIndexes.Add(index);
            AddEvent(CollectionChangedEvent.Changed(null, index, null), Version);
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

        protected override void OnItemChanged(object? item, int index, object? args, bool batchUpdate, int version)
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

        protected virtual NSIndexPath GetIndexPath(int index) => NSIndexPath.FromRowSection(index, 0);

        protected NSIndexPath[] GetIndexPaths(int startingPosition, int count)
        {
            var newIndexPaths = new NSIndexPath[count];
            for (var i = 0; i < count; i++)
                newIndexPaths[i] = GetIndexPath(i + startingPosition);
            return newIndexPaths;
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

        protected static void DisposeIndexPaths(NSIndexPath[] paths)
        {
            for (var i = 0; i < paths.Length; i++)
                paths[i].Dispose();
        }

        #endregion

        #region Nested types

        protected sealed class Closure
        {
            #region Fields

            private readonly IosBindableCollectionAdapter _adapter;
            private readonly int _version;

            private Action? _endBatchUpdate;
            private Action<bool>? _endPerformUpdates;
            private Action? _performUpdates;

            #endregion

            #region Constructors

            private Closure(IosBindableCollectionAdapter adapter, int version)
            {
                _adapter = adapter;
                _version = version;
            }

            #endregion

            #region Properties

            public Action EndBatchUpdate => _endBatchUpdate ??= EndBatchUpdateImpl;

            public Action PerformUpdates => _performUpdates ??= PerformUpdatesIml;

            public Action<bool> EndPerformUpdates => _endPerformUpdates ??= EndPerformUpdatesImpl;

            #endregion

            #region Methods

            public static Closure GetClosure(IosBindableCollectionAdapter adapter, int version)
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
                _adapter._beforeResetList.Clear();
                _adapter._reloadIndexes.Clear();
                _adapter._diffResult = default;
                _adapter.EndBatchUpdate(_version);
            }

            private void PerformUpdatesIml() => _adapter._diffResult.DispatchUpdatesTo(_adapter);

            private void EndBatchUpdateImpl() => _adapter.EndBatchUpdate(_version);

            #endregion
        }

        #endregion
    }
}