using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Collections
{
    public class DiffableBindableCollectionAdapter : BindableCollectionAdapter, DiffUtil.IListUpdateCallback, DiffUtil.ICallback
    {
        private bool _resetBatchUpdate;
        private int _resetVersion;

        public DiffableBindableCollectionAdapter(IList<object?>? source = null, IThreadDispatcher? threadDispatcher = null)
            : base(source, threadDispatcher)
        {
        }

        public IDiffableEqualityComparer? DiffableComparer { get; set; }

        public bool DetectMoves { get; set; } = true;

        protected IReadOnlyList<object?>? ResetItems { get; private set; }

        public int GetOldListSize() => Items.Count;

        public int GetNewListSize() => ResetItems!.Count;

        protected virtual void OnClear(bool batchUpdate, int version) => Items.Clear();

        protected virtual void OnResetting(bool batchUpdate, int version)
        {
        }

        protected virtual void OnResetCompleted(bool batchUpdate, int version)
        {
        }

        protected virtual bool AreItemsTheSame(int oldItemPosition, int newItemPosition)
        {
            if (DiffableComparer == null)
                return Equals(Items[oldItemPosition], ResetItems![newItemPosition]);
            return DiffableComparer.AreItemsTheSame(Items[oldItemPosition], ResetItems![newItemPosition]);
        }

        protected virtual bool AreContentsTheSame(int oldItemPosition, int newItemPosition)
        {
            if (DiffableComparer is IContentDiffableEqualityComparer contentDiffable)
                return contentDiffable.AreContentsTheSame(Items[oldItemPosition], ResetItems![newItemPosition]);
            return true;
        }

        protected virtual void OnInsertedDiff(int position, int finalPosition, int count)
        {
            for (var i = 0; i < count; i++)
                OnAdded(ResetItems![finalPosition + i], position + i, _resetBatchUpdate, _resetVersion);
        }

        protected virtual void OnRemovedDiff(int position, int count)
        {
            for (var i = count - 1; i >= 0; i--)
                OnRemoved(Items[position + i], position + i, _resetBatchUpdate, _resetVersion);
        }

        protected virtual void OnMovedDiff(int fromPosition, int toPosition, int fromOriginalPosition, int toFinalPosition)
            => OnMoved(ResetItems![toFinalPosition], fromPosition, toPosition, _resetBatchUpdate, _resetVersion);

        protected virtual void OnChangedDiff(int position, int finalPosition, int count, bool isMove)
            => OnItemChanged(ResetItems![finalPosition], position, null, _resetBatchUpdate, _resetVersion);

        protected sealed override void OnReset(IEnumerable<object?>? items, bool batchUpdate, int version)
        {
            if (items == null)
            {
                OnClear(batchUpdate, version);
                return;
            }

            _resetVersion = version;
            _resetBatchUpdate = batchUpdate;
            if (items is IReadOnlyList<object?> list)
                ResetItems = list;
            else
            {
                ResetCache ??= new List<object?>();
                ResetCache.Clear();
                ResetCache.AddRange(items);
                ResetItems = ResetCache;
            }

            var diff = DiffUtil.CalculateDiff(this, DetectMoves);
            OnResetting(batchUpdate, version);
            diff.DispatchUpdatesTo(this);
            ResetCache?.Clear();
            ResetItems = null;
            OnResetCompleted(batchUpdate, version);
        }

        bool DiffUtil.ICallback.AreItemsTheSame(int oldItemPosition, int newItemPosition) => AreItemsTheSame(oldItemPosition, newItemPosition);

        bool DiffUtil.ICallback.AreContentsTheSame(int oldItemPosition, int newItemPosition) => AreContentsTheSame(oldItemPosition, newItemPosition);

        void DiffUtil.IListUpdateCallback.OnInserted(int position, int finalPosition, int count) => OnInsertedDiff(position, finalPosition, count);

        void DiffUtil.IListUpdateCallback.OnRemoved(int position, int count) => OnRemovedDiff(position, count);

        void DiffUtil.IListUpdateCallback.OnMoved(int fromPosition, int toPosition, int fromOriginalPosition, int toFinalPosition)
            => OnMovedDiff(fromPosition, toPosition, fromOriginalPosition, toFinalPosition);

        void DiffUtil.IListUpdateCallback.OnChanged(int position, int finalPosition, int count, bool isMove)
            => OnChangedDiff(position, finalPosition, count, isMove);
    }
}