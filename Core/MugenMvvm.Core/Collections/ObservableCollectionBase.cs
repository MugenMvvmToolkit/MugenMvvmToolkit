using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Collections
{
    public abstract class ObservableCollectionBase<T> : ComponentOwnerBase<IObservableCollection<T>>, IObservableCollection<T>, IReadOnlyList<T>, ActionToken.IHandler
    {
        #region Fields

        private int _batchCount;

        #endregion

        #region Constructors

        protected ObservableCollectionBase(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Properties

        public abstract int Count { get; }

        public abstract bool IsReadOnly { get; }

        public abstract T this[int index] { get; set; }

        #endregion

        #region Implementation of interfaces

        void ActionToken.IHandler.Invoke(object? _, object? __)
        {
            using (this.TryLock())
            {
                if (--_batchCount == 0)
                    OnEndBatchUpdate();
            }
        }

        public ActionToken BeginBatchUpdate()
        {
            using (this.TryLock())
            {
                if (++_batchCount == 1)
                    OnBeginBatchUpdate();
            }

            return new ActionToken(this);
        }

        public abstract void Add(T item);

        public abstract void Clear();

        public abstract bool Contains(T item);

        public abstract void CopyTo(T[] array, int arrayIndex);

        public abstract bool Remove(T item);

        public abstract int IndexOf(T item);

        public abstract void Insert(int index, T item);

        public abstract void RemoveAt(int index);

        public abstract void Move(int oldIndex, int newIndex);

        public abstract void Reset(IEnumerable<T> items);

        public abstract void RaiseItemChanged(T item, object? args);

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        #endregion

        #region Methods

        protected abstract IEnumerator<T> GetEnumeratorInternal();

        protected virtual void OnBeginBatchUpdate()
        {
            var components = GetComponents<IObservableCollectionBatchUpdateListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnBeginBatchUpdate(this);
        }

        protected virtual void OnEndBatchUpdate()
        {
            var components = GetComponents<IObservableCollectionBatchUpdateListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnEndBatchUpdate(this);
        }

        protected virtual bool OnAdding(T item, int index)
        {
            var conditionComponents = GetComponents<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanAdd(this, item, index))
                    return false;
            }

            var components = GetComponents<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnAdding(this, item, index);

            return true;
        }

        protected virtual bool OnReplacing(T oldItem, T newItem, int index)
        {
            var conditionComponents = GetComponents<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanReplace(this, oldItem, newItem, index))
                    return false;
            }

            var components = GetComponents<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnReplacing(this, oldItem, newItem, index);

            return true;
        }

        protected virtual bool OnMoving(T item, int oldIndex, int newIndex)
        {
            var conditionComponents = GetComponents<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanMove(this, item, oldIndex, newIndex))
                    return false;
            }

            var components = GetComponents<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnMoving(this, item, oldIndex, newIndex);

            return true;
        }

        protected virtual bool OnRemoving(T item, int index)
        {
            var conditionComponents = GetComponents<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanRemove(this, item, index))
                    return false;
            }

            var components = GetComponents<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnRemoving(this, item, index);

            return true;
        }

        protected virtual bool OnResetting(IEnumerable<T> items)
        {
            var conditionComponents = GetComponents<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanReset(this, items))
                    return false;
            }

            var components = GetComponents<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnResetting(this, items);

            return true;
        }

        protected virtual bool OnClearing()
        {
            var conditionComponents = GetComponents<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanClear(this))
                    return false;
            }

            var components = GetComponents<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnClearing(this);

            return true;
        }

        protected virtual void OnAdded(T item, int index)
        {
            var components = GetComponents<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnAdded(this, item, index);
        }

        protected virtual void OnReplaced(T oldItem, T newItem, int index)
        {
            var components = GetComponents<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnReplaced(this, oldItem, newItem, index);
        }

        protected virtual void OnMoved(T item, int oldIndex, int newIndex)
        {
            var components = GetComponents<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnMoved(this, item, oldIndex, newIndex);
        }

        protected virtual void OnRemoved(T item, int index)
        {
            var components = GetComponents<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnRemoved(this, item, index);
        }

        protected virtual void OnReset(IEnumerable<T> items)
        {
            var components = GetComponents<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnReset(this, items);
        }

        protected virtual void OnCleared()
        {
            var components = GetComponents<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnCleared(this);
        }

        protected virtual void OnItemChanged(IDecoratorObservableCollectionComponent<T>? decorator, T item, int index, object? args)
        {
            var components = GetComponents<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnItemChanged(this, item, index, args);
        }

        protected static bool IsCompatibleObject(object value)
        {
            if (value is T)
                return true;

            if (value == null)
                return Default.IsNullable<T>();

            return false;
        }

        #endregion
    }
}