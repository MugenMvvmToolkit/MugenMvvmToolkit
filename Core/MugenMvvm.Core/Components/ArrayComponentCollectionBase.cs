using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Components
{
    public abstract class ArrayComponentCollectionBase<T> : IComponentCollection<T> where T : class
    {
        #region Fields

        private IComponentCollection<IComponent<IComponentCollection<T>>>? _components;
        protected T[] Items;

        #endregion

        #region Constructors

        protected ArrayComponentCollectionBase()
        {
            Items = Default.EmptyArray<T>();
        }

        #endregion

        #region Properties

        public abstract object Owner { get; }

        protected abstract bool IsOrdered { get; }

        protected abstract bool IsSynchronized { get; }

        public bool HasItems => Items.Length > 0;

        bool IComponentOwner<IComponentCollection<T>>.HasComponents => _components != null && _components.HasItems;

        IComponentCollection<IComponent<IComponentCollection<T>>> IComponentOwner<IComponentCollection<T>>.Components
        {
            get
            {
                if (_components == null)
                    Service<IComponentCollectionProvider>.Instance.LazyInitialize(ref _components, this);
                return _components;
            }
        }

        #endregion

        #region Implementation of interfaces

        public T[] GetItems()
        {
            return Items;
        }

        public bool Add(T component, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(component, nameof(component));

            var defaultListener = CallbackInvokerComponentCollectionComponent.GetComponentCollectionListener<T>();
            if (!defaultListener.OnAdding(this, component, metadata))
                return false;

            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IComponentCollectionChangingListener<T> listener && !listener.OnAdding(this, component, metadata))
                    return false;
            }

            if (IsSynchronized)
            {
                if (!AddLockImpl(component, metadata))
                    return false;
            }
            else
            {
                if (!AddInternal(component, metadata))
                    return false;
            }

            for (var i = 0; i < components.Length; i++)
                (components[i] as IComponentCollectionChangedListener<T>)?.OnAdded(this, component, metadata);

            defaultListener.OnAdded(this, component, metadata);

            return true;
        }

        public bool Remove(T component, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(component, nameof(component));

            var defaultListener = CallbackInvokerComponentCollectionComponent.GetComponentCollectionListener<T>();
            if (!defaultListener.OnRemoving(this, component, metadata))
                return false;

            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IComponentCollectionChangingListener<T> listener && !listener.OnRemoving(this, component, metadata))
                    return false;
            }

            if (IsSynchronized)
            {
                if (!RemoveLockImpl(component, metadata))
                    return false;
            }
            else
            {
                if (!RemoveInternal(component, metadata))
                    return false;
            }

            for (var i = 0; i < components.Length; i++)
                (components[i] as IComponentCollectionChangedListener<T>)?.OnRemoved(this, component, metadata);

            defaultListener.OnRemoved(this, component, metadata);

            return true;
        }

        public bool Clear(IReadOnlyMetadataContext? metadata = null)
        {
            var defaultListener = CallbackInvokerComponentCollectionComponent.GetComponentCollectionListener<T>();
            if (!defaultListener.OnClearing(this, metadata))
                return false;

            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IComponentCollectionChangingListener<T> listener && !listener.OnClearing(this, metadata))
                    return false;
            }

            var oldItems = Items;
            if (!ClearInternal(metadata))
                return false;

            for (var i = 0; i < components.Length; i++)
                (components[i] as IComponentCollectionChangedListener<T>)?.OnCleared(this, oldItems, metadata);

            defaultListener.OnCleared(this, oldItems, metadata);
            Array.Clear(oldItems, 0, oldItems.Length);
            return true;
        }

        #endregion

        #region Methods

        protected virtual bool AddInternal(T component, IReadOnlyMetadataContext? metadata)
        {
            if (IsOrdered)
                AddOrdered(component);
            else
            {
                var array = new T[Items.Length + 1];
                Array.Copy(Items, array, Items.Length);
                array[array.Length - 1] = component;
                Items = array;
            }

            return true;
        }

        protected virtual bool RemoveInternal(T component, IReadOnlyMetadataContext? metadata)
        {
            return MugenExtensions.Remove(ref Items, component);
        }

        protected virtual bool ClearInternal(IReadOnlyMetadataContext? metadata)
        {
            Items = Default.EmptyArray<T>();
            return true;
        }

        protected virtual int GetPriority(T component)
        {
            if (component is IComponent c)
                return c.GetPriority(Owner);
            return ((IHasPriority)component).Priority;
        }

        private bool AddLockImpl(T component, IReadOnlyMetadataContext? metadata)
        {
            lock (this)
            {
                return AddInternal(component, metadata);
            }
        }

        private bool RemoveLockImpl(T component, IReadOnlyMetadataContext? metadata)
        {
            lock (this)
            {
                return RemoveInternal(component, metadata);
            }
        }

        private void AddOrdered(T component)
        {
            var array = new T[Items.Length + 1];
            var added = false;
            var priority = GetPriority(component);
            for (var i = 0; i < Items.Length; i++)
            {
                if (added)
                {
                    array[i + 1] = Items[i];
                    continue;
                }

                var oldItem = Items[i];
                var compareTo = priority.CompareTo(GetPriority(oldItem));
                if (compareTo > 0)
                {
                    array[i] = component;
                    added = true;
                    --i;
                }
                else
                    array[i] = oldItem;
            }

            if (!added)
                array[array.Length - 1] = component;
            Items = array;
        }

        #endregion
    }
}