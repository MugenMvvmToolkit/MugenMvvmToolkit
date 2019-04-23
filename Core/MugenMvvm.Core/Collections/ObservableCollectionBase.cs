using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Collections
{
    public abstract class ObservableCollectionBase<T> : IObservableCollection<T>, IReadOnlyList<T>, IObservableCollectionDecoratorManager<T>
    {
        #region Fields

        private int _batchCount;
        private int _batchCountDecorators;
        private IComponentCollection<IObservableCollectionChangedListener<T>>? _decoratorListeners;
        private IComponentCollection<IObservableCollectionDecorator<T>>? _decorators;
        private IComponentCollection<IObservableCollectionChangedListener<T>>? _listeners;

        #endregion

        #region Constructors

        protected ObservableCollectionBase(IComponentCollectionProvider? componentCollectionProvider = null)
        {
            ComponentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        protected IComponentCollectionProvider? ComponentCollectionProvider { get; }

        public abstract int Count { get; }

        public abstract bool IsReadOnly { get; }

        public abstract T this[int index] { get; set; }

        public IComponentCollection<IObservableCollectionChangedListener<T>> Listeners
        {
            get
            {
                if (_listeners == null)
                    ComponentCollectionProvider.LazyInitialize(ref _listeners, this);

                return _listeners;
            }
        }

        public IComponentCollection<IObservableCollectionDecorator<T>> Decorators
        {
            get
            {
                if (_decorators == null)
                    ComponentCollectionProvider.LazyInitialize(ref _decorators, this);

                return _decorators;
            }
        }

        public IComponentCollection<IObservableCollectionChangedListener<T>> DecoratorListeners
        {
            get
            {
                if (_decoratorListeners == null)
                    ComponentCollectionProvider.LazyInitialize(ref _decoratorListeners, this);

                return _decoratorListeners;
            }
        }

        IObservableCollection<T> IObservableCollectionDecoratorManager<T>.Collection => this;

        #endregion

        #region Implementation of interfaces

        public IEnumerable<T> DecorateItems()
        {
            return DecorateItems(null);
        }

        public IDisposable BeginBatchUpdate(BatchUpdateCollectionMode mode = BatchUpdateCollectionMode.Both)
        {
            var hasListeners = mode.HasFlagEx(BatchUpdateCollectionMode.Listeners);
            var hasDecorators = mode.HasFlagEx(BatchUpdateCollectionMode.Decorators);
            if (hasListeners && Interlocked.Increment(ref _batchCount) == 1)
                OnBeginBatchUpdate(false);

            if (hasDecorators && Interlocked.Increment(ref _batchCountDecorators) == 1)
                OnBeginBatchUpdate(true);

            if (!hasListeners && !hasDecorators)
                return Default.Disposable;

            return WeakActionToken.Create(this, Default.BoolToObject(hasListeners), Default.BoolToObject(hasDecorators),
                (@base, b1, b2) => @base.EndBatchUpdate((bool) b1, (bool) b2));
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

        IEnumerable<T> IObservableCollectionDecoratorManager<T>.DecorateItems(IObservableCollectionDecorator<T> decorator)
        {
            Should.NotBeNull(decorator, nameof(decorator));
            return DecorateItems(decorator);
        }

        void IObservableCollectionDecoratorManager<T>.OnItemChanged(IObservableCollectionDecorator<T> decorator, T item, int index, object? args)
        {
            OnItemChanged(decorator, item, index, args);
        }

        void IObservableCollectionDecoratorManager<T>.OnAdded(IObservableCollectionDecorator<T> decorator, T item, int index)
        {
            Should.NotBeNull(decorator, nameof(decorator));
            OnAdded(decorator, item, index);
        }

        void IObservableCollectionDecoratorManager<T>.OnReplaced(IObservableCollectionDecorator<T> decorator, T oldItem, T newItem, int index)
        {
            Should.NotBeNull(decorator, nameof(decorator));
            OnReplaced(decorator, oldItem, newItem, index);
        }

        void IObservableCollectionDecoratorManager<T>.OnMoved(IObservableCollectionDecorator<T> decorator, T item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(decorator, nameof(decorator));
            OnMoved(decorator, item, oldIndex, newIndex);
        }

        void IObservableCollectionDecoratorManager<T>.OnRemoved(IObservableCollectionDecorator<T> decorator, T item, int index)
        {
            Should.NotBeNull(decorator, nameof(decorator));
            OnRemoved(decorator, item, index);
        }

        void IObservableCollectionDecoratorManager<T>.OnReset(IObservableCollectionDecorator<T> decorator, IEnumerable<T> items)
        {
            Should.NotBeNull(decorator, nameof(decorator));
            OnReset(decorator, items);
        }

        void IObservableCollectionDecoratorManager<T>.OnCleared(IObservableCollectionDecorator<T> decorator)
        {
            Should.NotBeNull(decorator, nameof(decorator));
            OnCleared(decorator);
        }

        #endregion

        #region Methods

        protected abstract IEnumerator<T> GetEnumeratorInternal();

        protected virtual void OnBeginBatchUpdate(bool decorators)
        {
            var listeners = decorators ? GetDecoratorListeners() : GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnBeginBatchUpdate(this);
        }

        protected virtual void OnEndBatchUpdate(bool decorators)
        {
            var listeners = decorators ? GetDecoratorListeners() : GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnEndBatchUpdate(this);
        }

        protected virtual IEnumerable<T> DecorateItems(IObservableCollectionDecorator<T>? decorator)
        {
            IEnumerable<T> items = this;
            var decorators = GetDecorators(decorator, out var indexOf);
            if (decorators.Length != 0)
            {
                for (var i = 0; i < indexOf.GetValueOrDefault(decorators.Length); i++)
                    items = decorators[i].DecorateItems(items);
            }

            return items;
        }

        protected virtual bool OnAdding(T item, int index)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
            {
                if (listeners[i] is IObservableCollectionChangingListener<T> listener && !listener.OnAdding(this, item, index))
                    return false;
            }

            return true;
        }

        protected virtual bool OnReplacing(T oldItem, T newItem, int index)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
            {
                if (listeners[i] is IObservableCollectionChangingListener<T> listener && !listener.OnReplacing(this, oldItem, newItem, index))
                    return false;
            }

            return true;
        }

        protected virtual bool OnMoving(T item, int oldIndex, int newIndex)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
            {
                if (listeners[i] is IObservableCollectionChangingListener<T> listener && !listener.OnMoving(this, item, oldIndex, newIndex))
                    return false;
            }

            return true;
        }

        protected virtual bool OnRemoving(T item, int index)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
            {
                if (listeners[i] is IObservableCollectionChangingListener<T> listener && !listener.OnRemoving(this, item, index))
                    return false;
            }

            return true;
        }

        protected virtual bool OnResetting(IEnumerable<T> items)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
            {
                if (listeners[i] is IObservableCollectionChangingListener<T> listener && !listener.OnResetting(this, items))
                    return false;
            }

            return true;
        }

        protected virtual bool OnClearing()
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
            {
                if (listeners[i] is IObservableCollectionChangingListener<T> listener && !listener.OnClearing(this))
                    return false;
            }

            return true;
        }

        protected virtual void OnAdded(T item, int index)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnAdded(this, item, index);

            OnAdded(null, item, index);
        }

        protected virtual void OnReplaced(T oldItem, T newItem, int index)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReplaced(this, oldItem, newItem, index);

            OnReplaced(null, oldItem, newItem, index);
        }

        protected virtual void OnMoved(T item, int oldIndex, int newIndex)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnMoved(this, item, oldIndex, newIndex);

            OnMoved(null, item, oldIndex, newIndex);
        }

        protected virtual void OnRemoved(T item, int index)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnRemoved(this, item, index);

            OnRemoved(null, item, index);
        }

        protected virtual void OnReset(IEnumerable<T> items)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReset(this, items);

            OnReset(null, items);
        }

        protected virtual void OnCleared()
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnCleared(this);

            OnCleared(null);
        }

        protected virtual void OnItemChanged(IObservableCollectionDecorator<T>? decorator, T item, int index, object? args)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnItemChanged(this, item, index, args);

            var decorators = GetDecorators(decorator, out var indexOf);
            if (decorators.Length != 0)
            {
                for (var i = indexOf.GetValueOrDefault(-1) + 1; i < decorators.Length; i++)
                {
                    if (!decorators[i].OnItemChanged(ref item, ref index, ref args))
                        return;
                }
            }

            listeners = GetDecoratorListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnItemChanged(this, item, index, args);
        }

        protected virtual void OnAdded(IObservableCollectionDecorator<T>? decorator, T item, int index)
        {
            var decorators = GetDecorators(decorator, out var indexOf);
            if (decorators.Length != 0)
            {
                for (var i = indexOf.GetValueOrDefault(-1) + 1; i < decorators.Length; i++)
                {
                    if (!decorators[i].OnAdded(ref item, ref index))
                        return;
                }
            }

            var listeners = GetDecoratorListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnAdded(this, item, index);
        }

        protected virtual void OnReplaced(IObservableCollectionDecorator<T>? decorator, T oldItem, T newItem, int index)
        {
            var decorators = GetDecorators(decorator, out var indexOf);
            if (decorators.Length != 0)
            {
                for (var i = indexOf.GetValueOrDefault(-1) + 1; i < decorators.Length; i++)
                {
                    if (!decorators[i].OnReplaced(ref oldItem, ref newItem, ref index))
                        return;
                }
            }

            var listeners = GetDecoratorListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReplaced(this, oldItem, newItem, index);
        }

        protected virtual void OnMoved(IObservableCollectionDecorator<T>? decorator, T item, int oldIndex, int newIndex)
        {
            var decorators = GetDecorators(decorator, out var indexOf);
            if (decorators.Length != 0)
            {
                for (var i = indexOf.GetValueOrDefault(-1) + 1; i < decorators.Length; i++)
                {
                    if (!decorators[i].OnMoved(ref item, ref oldIndex, ref newIndex))
                        return;
                }
            }

            var listeners = GetDecoratorListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnMoved(this, item, oldIndex, newIndex);
        }

        protected virtual void OnRemoved(IObservableCollectionDecorator<T>? decorator, T item, int index)
        {
            var decorators = GetDecorators(decorator, out var indexOf);
            if (decorators.Length != 0)
            {
                for (var i = indexOf.GetValueOrDefault(-1) + 1; i < decorators.Length; i++)
                {
                    if (!decorators[i].OnRemoved(ref item, ref index))
                        return;
                }
            }

            var listeners = GetDecoratorListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnRemoved(this, item, index);
        }

        protected virtual void OnReset(IObservableCollectionDecorator<T>? decorator, IEnumerable<T> items)
        {
            var decorators = GetDecorators(decorator, out var indexOf);
            if (decorators.Length != 0)
            {
                for (var i = indexOf.GetValueOrDefault(-1) + 1; i < decorators.Length; i++)
                {
                    if (!decorators[i].OnReset(ref items))
                        return;
                }
            }

            var listeners = GetDecoratorListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReset(this, items);
        }

        protected virtual void OnCleared(IObservableCollectionDecorator<T>? decorator)
        {
            var decorators = GetDecorators(decorator, out var indexOf);
            if (decorators.Length != 0)
            {
                for (var i = indexOf.GetValueOrDefault(-1) + 1; i < decorators.Length; i++)
                {
                    if (!decorators[i].OnCleared())
                        return;
                }
            }

            var listeners = GetDecoratorListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnCleared(this);
        }

        protected static bool IsCompatibleObject(object value)
        {
            if (value is T)
                return true;

            if (value == null)
                return default(T) == null;

            return false;
        }

        protected IObservableCollectionChangedListener<T>[] GetListeners()
        {
            return _listeners.GetItemsOrDefault();
        }

        protected IObservableCollectionChangedListener<T>[] GetDecoratorListeners()
        {
            return _decoratorListeners.GetItemsOrDefault();
        }

        protected IObservableCollectionDecorator<T>[] GetDecorators(IObservableCollectionDecorator<T>? decorator, out int? indexOf)
        {
            indexOf = null;
            var decorators = _decorators?.GetItems();
            if (decorator == null || decorators == null)
                return decorators ?? Default.EmptyArray<IObservableCollectionDecorator<T>>();

            for (var i = 0; i < decorators.Length; i++)
            {
                if (ReferenceEquals(decorators[i], decorator))
                {
                    indexOf = i;
                    break;
                }
            }

            return decorators;
        }

        private void EndBatchUpdate(bool hasListeners, bool hasDecorators)
        {
            if (hasListeners && Interlocked.Decrement(ref _batchCount) == 0)
                OnEndBatchUpdate(false);

            if (hasDecorators && Interlocked.Decrement(ref _batchCountDecorators) == 0)
                OnEndBatchUpdate(true);
        }

        #endregion
    }
}