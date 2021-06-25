using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Collections
{
    public class DiffableBindableCollectionAdapter : BindableCollectionAdapter, DiffUtil.IListUpdateCallback, DiffUtil.ICallback
    {
        private IReadOnlyList<object?>? _resetItems;
        private bool _resetBatchUpdate;
        private int _resetVersion;
        private int? _diffUtilAsyncLimit;
        private int? _diffUtilMaxLimit;

        public DiffableBindableCollectionAdapter(IList<object?>? source = null, IThreadDispatcher? threadDispatcher = null)
            : base(source, threadDispatcher)
        {
        }

        public IDiffableEqualityComparer? DiffableComparer { get; set; }

        public bool DetectMoves { get; set; } = true;

        public bool NotifyOnReload { get; set; }

        public int DiffUtilAsyncLimit
        {
            get => _diffUtilAsyncLimit.GetValueOrDefault(CollectionMetadata.DiffUtilAsyncLimit);
            set => _diffUtilAsyncLimit = value;
        }

        public int DiffUtilMaxLimit
        {
            get => _diffUtilMaxLimit.GetValueOrDefault(CollectionMetadata.DiffUtilMaxLimit);
            set => _diffUtilMaxLimit = value;
        }

        protected virtual void OnClear(bool batchUpdate, int version) => Items.Clear();

        protected virtual bool AreItemsTheSame(int oldItemPosition, int newItemPosition)
        {
            if (DiffableComparer == null)
                return Equals(Items[oldItemPosition], _resetItems![newItemPosition]);
            return DiffableComparer.AreItemsTheSame(Items[oldItemPosition], _resetItems![newItemPosition]);
        }

        protected virtual bool AreContentsTheSame(int oldItemPosition, int newItemPosition)
        {
            if (DiffableComparer is IContentDiffableEqualityComparer contentDiffable)
                return contentDiffable.AreContentsTheSame(Items[oldItemPosition], _resetItems![newItemPosition]);
            return true;
        }

        protected virtual void OnInsertedDiff(int position, int finalPosition, int count)
        {
            for (var i = 0; i < count; i++)
                OnAdded(_resetItems![finalPosition + i], position + i, _resetBatchUpdate, _resetVersion);
        }

        protected virtual void OnRemovedDiff(int position, int count)
        {
            for (var i = count - 1; i >= 0; i--)
                OnRemoved(Items[position + i], position + i, _resetBatchUpdate, _resetVersion);
        }

        protected virtual void Reload(IEnumerable<object?> items, bool batchUpdate, int version)
        {
            if (NotifyOnReload)
            {
                OnClear(batchUpdate, version);
                var index = 0;
                foreach (var item in items)
                    OnAdded(item, index++, batchUpdate, version);
            }
            else
                base.OnReset(items, batchUpdate, version);
        }

        protected virtual void OnMovedDiff(int fromPosition, int toPosition, int fromOriginalPosition, int toFinalPosition)
            => OnMoved(_resetItems![toFinalPosition], fromPosition, toPosition, _resetBatchUpdate, _resetVersion);

        protected virtual void OnChangedDiff(int position, int finalPosition, int count, bool isMove)
            => OnChanged(_resetItems![finalPosition], position, null, _resetBatchUpdate, _resetVersion);

        protected virtual ActionToken SuspendItems() => Items is ISuspendable suspendable ? suspendable.Suspend(this) : default;

        protected sealed override async void OnReset(IEnumerable<object?>? items, bool batchUpdate, int version)
        {
            using var _ = SuspendItems();
            if (items == null)
            {
                OnClear(batchUpdate, version);
                return;
            }

            if (Items.Count == 0 || Items.Count > DiffUtilMaxLimit)
            {
                Reload(items, batchUpdate, version);
                ResetCache?.Clear();
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
                Reload(_resetItems, batchUpdate, version);
                _resetItems = null;
                ResetCache?.Clear();
                return;
            }

            _resetVersion = version;
            _resetBatchUpdate = batchUpdate;
            var isAsync = Items.Count > DiffUtilAsyncLimit && _resetItems.Count > DiffUtilAsyncLimit;
            if (!batchUpdate && isAsync && !ReferenceEquals(_resetItems, ResetCache))
            {
                MugenExtensions.Reset(ref ResetCache, _resetItems);
                _resetItems = ResetCache;
            }

            if (isAsync)
                BeginBatchUpdate(version);
            try
            {
                var diff = await DiffUtil.CalculateDiffAsync(this, isAsync, DetectMoves);
                if (Version == version)
                {
                    diff.DispatchUpdatesTo(this);
                    _resetItems = null;
                    ResetCache?.Clear();
                    if (isAsync)
                        EndBatchUpdate(version);
                }
            }
            catch
            {
                if (Version == version)
                    throw;
            }
        }

        int DiffUtil.ICallback.GetOldListSize()
        {
            if (_resetVersion != Version)
                return 0;
            return Items.Count;
        }

        int DiffUtil.ICallback.GetNewListSize()
        {
            if (_resetVersion != Version)
                return 0;
            return _resetItems!.Count;
        }

        bool DiffUtil.ICallback.AreItemsTheSame(int oldItemPosition, int newItemPosition)
        {
            if (_resetVersion != Version)
                return true;
            return AreItemsTheSame(oldItemPosition, newItemPosition);
        }

        bool DiffUtil.ICallback.AreContentsTheSame(int oldItemPosition, int newItemPosition)
        {
            if (_resetVersion != Version)
                return true;
            return AreContentsTheSame(oldItemPosition, newItemPosition);
        }

        void DiffUtil.IListUpdateCallback.OnInserted(int position, int finalPosition, int count) => OnInsertedDiff(position, finalPosition, count);

        void DiffUtil.IListUpdateCallback.OnRemoved(int position, int count) => OnRemovedDiff(position, count);

        void DiffUtil.IListUpdateCallback.OnMoved(int fromPosition, int toPosition, int fromOriginalPosition, int toFinalPosition)
            => OnMovedDiff(fromPosition, toPosition, fromOriginalPosition, toFinalPosition);

        void DiffUtil.IListUpdateCallback.OnChanged(int position, int finalPosition, int count, bool isMove)
            => OnChangedDiff(position, finalPosition, count, isMove);
    }
}