using System;
using System.Collections.Generic;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Android.Collections
{
    public class ItemsSourceBindableCollectionAdapter : BindableCollectionAdapter, DiffUtil.ICallback, DiffUtil.IListUpdateCallback
    {
        protected readonly List<IItemsSourceObserver> Observers;

        private List<object?>? _beforeResetList;
        private int _diffSupportedCount;
        private bool _isAlive;

        public ItemsSourceBindableCollectionAdapter(IDiffableEqualityComparer? diffableComparer, IList<object?>? source = null, IThreadDispatcher? threadDispatcher = null)
            : base(source, threadDispatcher)
        {
            DiffableComparer = diffableComparer;
            Observers = new List<IItemsSourceObserver>();
            _isAlive = true;
        }

        public IDiffableEqualityComparer? DiffableComparer { get; }

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

        protected override void OnReset(IEnumerable<object?>? items, bool batchUpdate, int version)
        {
            if (_diffSupportedCount > 0)
            {
                if (_beforeResetList == null)
                    _beforeResetList = new List<object?>(this);
                else
                    _beforeResetList.AddRange(this);
                base.OnReset(items, batchUpdate, version);
                DiffUtil.CalculateDiff(this).DispatchUpdatesTo(this);
                _beforeResetList.Clear();
            }
            else
            {
                base.OnReset(items, batchUpdate, version);
                for (var i = 0; i < Observers.Count; i++)
                    GetObserver(i)?.OnReset();
            }
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

        int DiffUtil.ICallback.GetOldListSize() => _beforeResetList!.Count;

        int DiffUtil.ICallback.GetNewListSize() => Count;

        bool DiffUtil.ICallback.AreItemsTheSame(int oldItemPosition, int newItemPosition)
        {
            if (DiffableComparer == null)
                return Equals(_beforeResetList![oldItemPosition], this[newItemPosition]);
            return DiffableComparer.AreItemsTheSame(_beforeResetList![oldItemPosition], this[newItemPosition]);
        }

        bool DiffUtil.ICallback.AreContentsTheSame(int oldItemPosition, int newItemPosition) => true;

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
        }
    }
}