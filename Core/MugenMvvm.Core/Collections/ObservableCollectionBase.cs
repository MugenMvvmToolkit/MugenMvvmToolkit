using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Collections
{
    public abstract class ObservableCollectionBase<T> : ComponentOwnerBase<IObservableCollection<T>>, IObservableCollection<T>, IReadOnlyList<T>,
        IObservableCollectionDecoratorManager<T>, ActionToken.IHandler
    {
        #region Fields

        private int _batchCount;
        private int _batchCountDecorators;

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

        public IObservableCollectionDecoratorManager<T> DecoratorManager => this;

        #endregion

        #region Implementation of interfaces

        void ActionToken.IHandler.Invoke(object? state1, object? state2)
        {
            var hasListeners = (bool)state1!;
            var hasDecorators = (bool)state2!;
            using (Lock())
            {
                if (hasListeners && _batchCount-- == 0)
                    OnEndBatchUpdate(false);

                if (hasDecorators && _batchCountDecorators-- == 0)
                    OnEndBatchUpdate(true);
            }
        }

        public IEnumerable<T> DecorateItems()
        {
            return DecorateItems(null);
        }

        public ActionToken BeginBatchUpdate(BatchUpdateCollectionMode mode = BatchUpdateCollectionMode.Both)
        {
            var hasListeners = mode.HasFlagEx(BatchUpdateCollectionMode.Listeners);
            var hasDecorators = mode.HasFlagEx(BatchUpdateCollectionMode.DecoratorListeners);
            if (!hasListeners && !hasDecorators)
                return default;

            using (Lock())
            {
                if (hasListeners && _batchCount++ == 1)
                    OnBeginBatchUpdate(false);

                if (hasDecorators && _batchCountDecorators++ == 1)
                    OnBeginBatchUpdate(true);

                return new ActionToken(this, BoxingExtensions.Box(hasListeners), BoxingExtensions.Box(hasDecorators));
            }
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

        public abstract ActionToken Lock();

        IEnumerable<T> IObservableCollectionDecoratorManager<T>.DecorateItems(IDecoratorObservableCollectionComponent<T> decorator)
        {
            Should.NotBeNull(decorator, nameof(decorator));
            return DecorateItems(decorator);
        }

        void IObservableCollectionDecoratorManager<T>.OnItemChanged(IDecoratorObservableCollectionComponent<T> decorator, T item, int index, object? args)
        {
            OnItemChanged(decorator, item, index, args);
        }

        void IObservableCollectionDecoratorManager<T>.OnAdded(IDecoratorObservableCollectionComponent<T> decorator, T item, int index)
        {
            Should.NotBeNull(decorator, nameof(decorator));
            OnAdded(decorator, item, index);
        }

        void IObservableCollectionDecoratorManager<T>.OnReplaced(IDecoratorObservableCollectionComponent<T> decorator, T oldItem, T newItem, int index)
        {
            Should.NotBeNull(decorator, nameof(decorator));
            OnReplaced(decorator, oldItem, newItem, index);
        }

        void IObservableCollectionDecoratorManager<T>.OnMoved(IDecoratorObservableCollectionComponent<T> decorator, T item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(decorator, nameof(decorator));
            OnMoved(decorator, item, oldIndex, newIndex);
        }

        void IObservableCollectionDecoratorManager<T>.OnRemoved(IDecoratorObservableCollectionComponent<T> decorator, T item, int index)
        {
            Should.NotBeNull(decorator, nameof(decorator));
            OnRemoved(decorator, item, index);
        }

        void IObservableCollectionDecoratorManager<T>.OnReset(IDecoratorObservableCollectionComponent<T> decorator, IEnumerable<T> items)
        {
            Should.NotBeNull(decorator, nameof(decorator));
            OnReset(decorator, items);
        }

        void IObservableCollectionDecoratorManager<T>.OnCleared(IDecoratorObservableCollectionComponent<T> decorator)
        {
            Should.NotBeNull(decorator, nameof(decorator));
            OnCleared(decorator);
        }

        #endregion

        #region Methods

        protected abstract IEnumerator<T> GetEnumeratorInternal();

        protected virtual void OnBeginBatchUpdate(bool decorators)
        {
            var components = GetComponents<IObservableCollectionBatchUpdateListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (component.IsDecoratorComponent == decorators)
                    component.OnBeginBatchUpdate(this);
            }
        }

        protected virtual void OnEndBatchUpdate(bool decorators)
        {
            var components = GetComponents<IObservableCollectionBatchUpdateListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var listener = components[i];
                if (listener.IsDecoratorComponent == decorators)
                    listener.OnEndBatchUpdate(this);
            }
        }

        protected virtual IEnumerable<T> DecorateItems(IDecoratorObservableCollectionComponent<T>? decorator)
        {
            IEnumerable<T> items = this;
            var decorators = GetDecorators(decorator, out var startIndex, true);
            for (var i = 0; i < startIndex; i++)
                items = decorators[i].DecorateItems(items);

            return items;
        }

        protected virtual bool OnAdding(T item, int index)
        {
            var conditionComponents = GetComponents<IConditionObservableCollectionComponent<T>>(null);
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanAdd(this, item, index))
                    return false;
            }

            var components = GetComponents<IObservableCollectionChangingListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
                components[i].OnAdding(this, item, index);

            return true;
        }

        protected virtual bool OnReplacing(T oldItem, T newItem, int index)
        {
            var conditionComponents = GetComponents<IConditionObservableCollectionComponent<T>>(null);
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanReplace(this, oldItem, newItem, index))
                    return false;
            }

            var components = GetComponents<IObservableCollectionChangingListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
                components[i].OnReplacing(this, oldItem, newItem, index);

            return true;
        }

        protected virtual bool OnMoving(T item, int oldIndex, int newIndex)
        {
            var conditionComponents = GetComponents<IConditionObservableCollectionComponent<T>>(null);
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanMove(this, item, oldIndex, newIndex))
                    return false;
            }

            var components = GetComponents<IObservableCollectionChangingListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
                components[i].OnMoving(this, item, oldIndex, newIndex);

            return true;
        }

        protected virtual bool OnRemoving(T item, int index)
        {
            var conditionComponents = GetComponents<IConditionObservableCollectionComponent<T>>(null);
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanRemove(this, item, index))
                    return false;
            }

            var components = GetComponents<IObservableCollectionChangingListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
                components[i].OnRemoving(this, item, index);

            return true;
        }

        protected virtual bool OnResetting(IEnumerable<T> items)
        {
            var conditionComponents = GetComponents<IConditionObservableCollectionComponent<T>>(null);
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanReset(this, items))
                    return false;
            }

            var components = GetComponents<IObservableCollectionChangingListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
                components[i].OnResetting(this, items);

            return true;
        }

        protected virtual bool OnClearing()
        {
            var conditionComponents = GetComponents<IConditionObservableCollectionComponent<T>>(null);
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanClear(this))
                    return false;
            }

            var components = GetComponents<IObservableCollectionChangingListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
                components[i].OnClearing(this);

            return true;
        }

        protected virtual void OnAdded(T item, int index)
        {
            var components = GetComponents<IObservableCollectionChangedListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var listener = components[i];
                if (!listener.IsDecoratorComponent)
                    listener.OnAdded(this, item, index);
            }

            OnAdded(null, item, index);
        }

        protected virtual void OnReplaced(T oldItem, T newItem, int index)
        {
            var components = GetComponents<IObservableCollectionChangedListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var listener = components[i];
                if (!listener.IsDecoratorComponent)
                    listener.OnReplaced(this, oldItem, newItem, index);
            }

            OnReplaced(null, oldItem, newItem, index);
        }

        protected virtual void OnMoved(T item, int oldIndex, int newIndex)
        {
            var components = GetComponents<IObservableCollectionChangedListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var listener = components[i];
                if (!listener.IsDecoratorComponent)
                    listener.OnMoved(this, item, oldIndex, newIndex);
            }

            OnMoved(null, item, oldIndex, newIndex);
        }

        protected virtual void OnRemoved(T item, int index)
        {
            var components = GetComponents<IObservableCollectionChangedListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var listener = components[i];
                if (!listener.IsDecoratorComponent)
                    listener.OnRemoved(this, item, index);
            }

            OnRemoved(null, item, index);
        }

        protected virtual void OnReset(IEnumerable<T> items)
        {
            var components = GetComponents<IObservableCollectionChangedListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var listener = components[i];
                if (!listener.IsDecoratorComponent)
                    listener.OnReset(this, items);
            }

            OnReset(null, items);
        }

        protected virtual void OnCleared()
        {
            var components = GetComponents<IObservableCollectionChangedListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var listener = components[i];
                if (!listener.IsDecoratorComponent)
                    listener.OnCleared(this);
            }

            OnCleared(null);
        }

        protected virtual void OnItemChanged(IDecoratorObservableCollectionComponent<T>? decorator, T item, int index, object? args)
        {
            var components = GetComponents<IObservableCollectionChangedListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var listener = components[i];
                if (!listener.IsDecoratorComponent)
                    listener.OnItemChanged(this, item, index, args);
            }

            var decorators = GetDecorators(decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnItemChanged(ref item, ref index, ref args))
                    return;
            }

            for (var i = 0; i < components.Length; i++)
            {
                var listener = components[i];
                if (listener.IsDecoratorComponent)
                    listener.OnItemChanged(this, item, index, args);
            }
        }

        protected virtual void OnAdded(IDecoratorObservableCollectionComponent<T>? decorator, T item, int index)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnAdded(ref item, ref index))
                    return;
            }

            var components = GetComponents<IObservableCollectionChangedListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var listener = components[i];
                if (listener.IsDecoratorComponent)
                    listener.OnAdded(this, item, index);
            }
        }

        protected virtual void OnReplaced(IDecoratorObservableCollectionComponent<T>? decorator, T oldItem, T newItem, int index)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnReplaced(ref oldItem, ref newItem, ref index))
                    return;
            }

            var components = GetComponents<IObservableCollectionChangedListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var listener = components[i];
                if (listener.IsDecoratorComponent)
                    listener.OnReplaced(this, oldItem, newItem, index);
            }
        }

        protected virtual void OnMoved(IDecoratorObservableCollectionComponent<T>? decorator, T item, int oldIndex, int newIndex)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnMoved(ref item, ref oldIndex, ref newIndex))
                    return;
            }

            var components = GetComponents<IObservableCollectionChangedListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var listener = components[i];
                if (listener.IsDecoratorComponent)
                    listener.OnMoved(this, item, oldIndex, newIndex);
            }
        }

        protected virtual void OnRemoved(IDecoratorObservableCollectionComponent<T>? decorator, T item, int index)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnRemoved(ref item, ref index))
                    return;
            }

            var components = GetComponents<IObservableCollectionChangedListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var listener = components[i];
                if (listener.IsDecoratorComponent)
                    listener.OnRemoved(this, item, index);
            }
        }

        protected virtual void OnReset(IDecoratorObservableCollectionComponent<T>? decorator, IEnumerable<T> items)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnReset(ref items))
                    return;
            }

            var components = GetComponents<IObservableCollectionChangedListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var listener = components[i];
                if (listener.IsDecoratorComponent)
                    listener.OnReset(this, items);
            }
        }

        protected virtual void OnCleared(IDecoratorObservableCollectionComponent<T>? decorator)
        {
            var decorators = GetDecorators(decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnCleared())
                    return;
            }

            var components = GetComponents<IObservableCollectionChangedListener<T>>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var listener = components[i];
                if (listener.IsDecoratorComponent)
                    listener.OnCleared(this);
            }
        }

        protected static bool IsCompatibleObject(object value)
        {
            if (value is T)
                return true;

            if (value == null)
                return Default.IsNullable<T>();

            return false;
        }

        protected IDecoratorObservableCollectionComponent<T>[] GetDecorators(IDecoratorObservableCollectionComponent<T>? decorator, out int index, bool isLengthDefault = false)
        {
            var components = GetComponents<IDecoratorObservableCollectionComponent<T>>(null);
            index = isLengthDefault ? components.Length : 0;
            if (decorator == null)
                return components;

            for (var i = 0; i < components.Length; i++)
            {
                if (ReferenceEquals(components[i], decorator))
                {
                    index = i;
                    if (!isLengthDefault)
                        ++index;
                    break;
                }
            }

            return components;
        }

        #endregion
    }
}