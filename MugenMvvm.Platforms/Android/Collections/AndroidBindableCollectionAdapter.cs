using System;
using System.Collections.Generic;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Collections;

namespace MugenMvvm.Android.Collections
{
    public class AndroidBindableCollectionAdapter : BindableCollectionAdapter
    {
        #region Fields

        private bool _isAlive;

        #endregion

        #region Constructors

        public AndroidBindableCollectionAdapter()
        {
            Observers = new List<IItemsSourceObserver>();
            _isAlive = true;
        }

        #endregion

        #region Properties

        protected override bool IsAlive => _isAlive;

        public List<IItemsSourceObserver> Observers { get; }

        #endregion

        #region Methods

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

        protected override void OnReset(IEnumerable<object?> items, bool batchUpdate, int version)
        {
            base.OnReset(items, batchUpdate, version);
            for (var i = 0; i < Observers.Count; i++)
                GetObserver(i)?.OnReset();
        }

        protected override void OnCleared(bool batchUpdate, int version)
        {
            base.OnCleared(batchUpdate, version);
            for (var i = 0; i < Observers.Count; i++)
                GetObserver(i)?.OnReset();
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

        #endregion
    }
}