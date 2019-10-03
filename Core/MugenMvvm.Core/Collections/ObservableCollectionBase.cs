using System;
using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    public abstract class ObservableCollectionBase<T> : IObservableCollection<T>, IReadOnlyList<T>, IObservableCollectionDecoratorManager<T>
    {
        #region Fields

        private int _batchCount;
        private int _batchCountDecorators;
        private IComponentCollection<IComponent<IObservableCollection<T>>>? _components;
        private readonly IComponentCollectionProvider? _componentCollectionProvider;

        #endregion

        #region Constructors

        protected ObservableCollectionBase(IComponentCollectionProvider? componentCollectionProvider = null)
        {
            _componentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        protected IComponentCollectionProvider ComponentCollectionProvider => _componentCollectionProvider.ServiceIfNull();

        public bool HasComponents => _components != null && _components.HasItems;

        public abstract int Count { get; }

        public abstract bool IsReadOnly { get; }

        public abstract T this[int index] { get; set; }

        IObservableCollection<T> IObservableCollectionDecoratorManager<T>.Collection => this;

        public IComponentCollection<IComponent<IObservableCollection<T>>> Components
        {
            get
            {
                if (_components == null)
                    ComponentCollectionProvider.LazyInitialize(ref _components, this);

                return _components!;
            }
        }

        #endregion

        #region Implementation of interfaces

        public IEnumerable<T> DecorateItems()
        {
            return DecorateItems(null);
        }

        public IDisposable BeginBatchUpdate(BatchUpdateCollectionMode mode = null)
        {
            if (mode == null)
                mode = BatchUpdateCollectionMode.Both;
            var hasListeners = mode.HasFlag(BatchUpdateCollectionMode.Listeners);
            var hasDecorators = mode.HasFlag(BatchUpdateCollectionMode.DecoratorListeners);
            if (!hasListeners && !hasDecorators)
                return Default.Disposable;

            using (Lock())
            {
                if (hasListeners && _batchCount++ == 1)
                    OnBeginBatchUpdate(false);

                if (hasDecorators && _batchCountDecorators++ == 1)
                    OnBeginBatchUpdate(true);

                return WeakActionToken.Create(this, Default.BoolToObject(hasListeners), Default.BoolToObject(hasDecorators),
                    (@base, b1, b2) => @base.EndBatchUpdate((bool)b1, (bool)b2));
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

        public abstract IDisposable Lock();

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
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionBatchUpdateListener<T> listener && listener.IsDecoratorComponent == decorators)
                    listener.OnBeginBatchUpdate(this);
            }
        }

        protected virtual void OnEndBatchUpdate(bool decorators)
        {
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionBatchUpdateListener<T> listener && listener.IsDecoratorComponent == decorators)
                    listener.OnEndBatchUpdate(this);
            }
        }

        protected virtual IEnumerable<T> DecorateItems(IDecoratorObservableCollectionComponent<T>? decorator)
        {
            IEnumerable<T> items = this;
            var decorators = GetDecorators(decorator, out var indexOf);
            if (decorators.Length != 0)
            {
                for (var i = 0; i < indexOf.GetValueOrDefault(decorators.Length); i++)
                {
                    if (decorators[i] is IDecoratorObservableCollectionComponent<T> d)
                        items = d.DecorateItems(items);
                }
            }

            return items;
        }

        protected virtual bool OnAdding(T item, int index)
        {
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionObservableCollectionComponent<T> listener && !listener.CanAdd(this, item, index))
                    return false;
            }

            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangingListener<T> listener)
                    listener.OnAdding(this, item, index);
            }

            return true;
        }

        protected virtual bool OnReplacing(T oldItem, T newItem, int index)
        {
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionObservableCollectionComponent<T> listener && !listener.CanReplace(this, oldItem, newItem, index))
                    return false;
            }

            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangingListener<T> listener)
                    listener.OnReplacing(this, oldItem, newItem, index);
            }

            return true;
        }

        protected virtual bool OnMoving(T item, int oldIndex, int newIndex)
        {
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionObservableCollectionComponent<T> listener && !listener.CanMove(this, item, oldIndex, newIndex))
                    return false;
            }

            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangingListener<T> listener)
                    listener.OnMoving(this, item, oldIndex, newIndex);
            }

            return true;
        }

        protected virtual bool OnRemoving(T item, int index)
        {
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionObservableCollectionComponent<T> listener && !listener.CanRemove(this, item, index))
                    return false;
            }

            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangingListener<T> listener)
                    listener.OnRemoving(this, item, index);
            }

            return true;
        }

        protected virtual bool OnResetting(IEnumerable<T> items)
        {
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionObservableCollectionComponent<T> listener && !listener.CanReset(this, items))
                    return false;
            }

            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangingListener<T> listener)
                    listener.OnResetting(this, items);
            }

            return true;
        }

        protected virtual bool OnClearing()
        {
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionObservableCollectionComponent<T> listener && !listener.CanClear(this))
                    return false;
            }

            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangingListener<T> listener)
                    listener.OnClearing(this);
            }

            return true;
        }

        protected virtual void OnAdded(T item, int index)
        {
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangedListener<T> listener && !listener.IsDecoratorComponent)
                    listener.OnAdded(this, item, index);
            }

            OnAdded(null, item, index);
        }

        protected virtual void OnReplaced(T oldItem, T newItem, int index)
        {
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangedListener<T> listener && !listener.IsDecoratorComponent)
                    listener.OnReplaced(this, oldItem, newItem, index);
            }

            OnReplaced(null, oldItem, newItem, index);
        }

        protected virtual void OnMoved(T item, int oldIndex, int newIndex)
        {
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangedListener<T> listener && !listener.IsDecoratorComponent)
                    listener.OnMoved(this, item, oldIndex, newIndex);
            }

            OnMoved(null, item, oldIndex, newIndex);
        }

        protected virtual void OnRemoved(T item, int index)
        {
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangedListener<T> listener && !listener.IsDecoratorComponent)
                    listener.OnRemoved(this, item, index);
            }

            OnRemoved(null, item, index);
        }

        protected virtual void OnReset(IEnumerable<T> items)
        {
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangedListener<T> listener && !listener.IsDecoratorComponent)
                    listener.OnReset(this, items);
            }

            OnReset(null, items);
        }

        protected virtual void OnCleared()
        {
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangedListener<T> listener && !listener.IsDecoratorComponent)
                    listener.OnCleared(this);
            }

            OnCleared(null);
        }

        protected virtual void OnItemChanged(IDecoratorObservableCollectionComponent<T>? decorator, T item, int index, object? args)
        {
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangedListener<T> listener && !listener.IsDecoratorComponent)
                    listener.OnItemChanged(this, item, index, args);
            }

            var decorators = GetDecorators(decorator, out var indexOf);
            if (decorators.Length != 0)
            {
                for (var i = indexOf.GetValueOrDefault(-1) + 1; i < decorators.Length; i++)
                {
                    if (decorators[i] is IDecoratorObservableCollectionComponent<T> component && !component.OnItemChanged(ref item, ref index, ref args))
                        return;
                }
            }

            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangedListener<T> listener && listener.IsDecoratorComponent)
                    listener.OnItemChanged(this, item, index, args);
            }
        }

        protected virtual void OnAdded(IDecoratorObservableCollectionComponent<T>? decorator, T item, int index)
        {
            var decorators = GetDecorators(decorator, out var indexOf);
            if (decorators.Length != 0)
            {
                for (var i = indexOf.GetValueOrDefault(-1) + 1; i < decorators.Length; i++)
                {
                    if (decorators[i] is IDecoratorObservableCollectionComponent<T> component && !component.OnAdded(ref item, ref index))
                        return;
                }
            }

            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangedListener<T> listener && listener.IsDecoratorComponent)
                    listener.OnAdded(this, item, index);
            }
        }

        protected virtual void OnReplaced(IDecoratorObservableCollectionComponent<T>? decorator, T oldItem, T newItem, int index)
        {
            var decorators = GetDecorators(decorator, out var indexOf);
            if (decorators.Length != 0)
            {
                for (var i = indexOf.GetValueOrDefault(-1) + 1; i < decorators.Length; i++)
                {
                    if (decorators[i] is IDecoratorObservableCollectionComponent<T> component && !component.OnReplaced(ref oldItem, ref newItem, ref index))
                        return;
                }
            }

            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangedListener<T> listener && listener.IsDecoratorComponent)
                    listener.OnReplaced(this, oldItem, newItem, index);
            }
        }

        protected virtual void OnMoved(IDecoratorObservableCollectionComponent<T>? decorator, T item, int oldIndex, int newIndex)
        {
            var decorators = GetDecorators(decorator, out var indexOf);
            if (decorators.Length != 0)
            {
                for (var i = indexOf.GetValueOrDefault(-1) + 1; i < decorators.Length; i++)
                {
                    if (decorators[i] is IDecoratorObservableCollectionComponent<T> component && !component.OnMoved(ref item, ref oldIndex, ref newIndex))
                        return;
                }
            }

            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangedListener<T> listener && listener.IsDecoratorComponent)
                    listener.OnMoved(this, item, oldIndex, newIndex);
            }
        }

        protected virtual void OnRemoved(IDecoratorObservableCollectionComponent<T>? decorator, T item, int index)
        {
            var decorators = GetDecorators(decorator, out var indexOf);
            if (decorators.Length != 0)
            {
                for (var i = indexOf.GetValueOrDefault(-1) + 1; i < decorators.Length; i++)
                {
                    if (decorators[i] is IDecoratorObservableCollectionComponent<T> component && !component.OnRemoved(ref item, ref index))
                        return;
                }
            }

            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangedListener<T> listener && listener.IsDecoratorComponent)
                    listener.OnRemoved(this, item, index);
            }
        }

        protected virtual void OnReset(IDecoratorObservableCollectionComponent<T>? decorator, IEnumerable<T> items)
        {
            var decorators = GetDecorators(decorator, out var indexOf);
            if (decorators.Length != 0)
            {
                for (var i = indexOf.GetValueOrDefault(-1) + 1; i < decorators.Length; i++)
                {
                    if (decorators[i] is IDecoratorObservableCollectionComponent<T> component && !component.OnReset(ref items))
                        return;
                }
            }

            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangedListener<T> listener && listener.IsDecoratorComponent)
                    listener.OnReset(this, items);
            }
        }

        protected virtual void OnCleared(IDecoratorObservableCollectionComponent<T>? decorator)
        {
            var decorators = GetDecorators(decorator, out var indexOf);
            if (decorators.Length != 0)
            {
                for (var i = indexOf.GetValueOrDefault(-1) + 1; i < decorators.Length; i++)
                {
                    if (decorators[i] is IDecoratorObservableCollectionComponent<T> component && !component.OnCleared())
                        return;
                }
            }

            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IObservableCollectionChangedListener<T> listener && listener.IsDecoratorComponent)
                    listener.OnCleared(this);
            }
        }

        protected static bool IsCompatibleObject(object value)
        {
            if (value is T)
                return true;

            if (value == null)
                return default(T)! == null;

            return false;
        }

        protected IComponent<IObservableCollection<T>>[] GetDecorators(IDecoratorObservableCollectionComponent<T>? decorator, out int? indexOf)
        {
            indexOf = null;
            var components = this.GetComponents();
            if (decorator == null || components == null)
                return components ?? Default.EmptyArray<IComponent<IObservableCollection<T>>>();

            for (var i = 0; i < components.Length; i++)
            {
                if (ReferenceEquals(components[i], decorator))
                {
                    indexOf = i;
                    break;
                }
            }

            return components;
        }

        private void EndBatchUpdate(bool hasListeners, bool hasDecorators)
        {
            using (Lock())
            {
                if (hasListeners && _batchCount-- == 0)
                    OnEndBatchUpdate(false);

                if (hasDecorators && _batchCountDecorators-- == 0)
                    OnEndBatchUpdate(true);
            }
        }

        #endregion
    }
}