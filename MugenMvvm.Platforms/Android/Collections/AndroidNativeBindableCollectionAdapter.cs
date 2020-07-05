using System.Collections.Generic;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Collections;

namespace MugenMvvm.Android.Collections
{
    public sealed class AndroidNativeBindableCollectionAdapter : BindableCollectionAdapterBase<object?>
    {
        #region Constructors

        public AndroidNativeBindableCollectionAdapter()
        {
            Observers = new List<IItemsSourceObserver>();
        }

        #endregion

        #region Properties

        public List<IItemsSourceObserver> Observers { get; }

        #endregion

        #region Methods

        protected override void OnAddedInternal(object? item, int index, bool batch)
        {
            base.OnAddedInternal(item, index, batch);
            for (var i = 0; i < Observers.Count; i++)
                Observers[i].OnItemInserted(index);
        }

        protected override void OnMovedInternal(object? item, int oldIndex, int newIndex, bool batch)
        {
            base.OnMovedInternal(item, oldIndex, newIndex, batch);
            for (var i = 0; i < Observers.Count; i++)
                Observers[i].OnItemMoved(oldIndex, newIndex);
        }

        protected override void OnRemovedInternal(object? item, int index, bool batch)
        {
            base.OnRemovedInternal(item, index, batch);
            for (var i = 0; i < Observers.Count; i++)
                Observers[i].OnItemRemoved(index);
        }

        protected override void OnReplacedInternal(object? oldItem, object? newItem, int index, bool batch)
        {
            base.OnReplacedInternal(oldItem, newItem, index, batch);
            for (var i = 0; i < Observers.Count; i++)
                Observers[i].OnItemChanged(index);
        }

        protected override void OnResetInternal(IEnumerable<object?> items, bool batch)
        {
            base.OnResetInternal(items, batch);
            for (var i = 0; i < Observers.Count; i++)
                Observers[i].OnReset();
        }

        protected override void OnClearedInternal(bool batch)
        {
            base.OnClearedInternal(batch);
            for (var i = 0; i < Observers.Count; i++)
                Observers[i].OnReset();
        }

        #endregion
    }
}