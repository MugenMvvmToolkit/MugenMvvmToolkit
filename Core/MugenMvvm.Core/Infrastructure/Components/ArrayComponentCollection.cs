using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Components
{
    public class ArrayComponentCollection<T> : IComponentCollection<T> where T : class
    {
        #region Fields

        private IComponentCollection<IComponentCollectionListener>? _listeners;
        protected T[] Items;

        #endregion

        #region Constructors

        public ArrayComponentCollection(object owner)
        {
            Should.NotBeNull(owner, nameof(owner));
            Owner = owner;
            Items = Default.EmptyArray<T>();
        }

        #endregion

        #region Properties

        public object Owner { get; }

        public bool HasItems => Items.Length > 0;

        public IComponentCollection<IComponentCollectionListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    Service<IComponentCollectionProvider>.Instance.LazyInitialize(ref _listeners, this);
                return _listeners;
            }
        }

        protected bool HasListeners => _listeners != null && _listeners.HasItems;

        #endregion

        #region Implementation of interfaces

        public T[] GetItems()
        {
            return Items;
        }

        public bool Add(T component, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(component, nameof(component));
            if (metadata == null)
                metadata = Default.MetadataContext;
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
            {
                if (!listeners[i].OnAdding(this, component, metadata))
                    return false;
            }

            lock (this)
            {
                if (!AddInternal(component, metadata))
                    return false;
            }

            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnAdded(this, component, metadata);

            return true;
        }

        public bool Remove(T component, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(component, nameof(component));
            if (metadata == null)
                metadata = Default.MetadataContext;
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
            {
                if (!listeners[i].OnRemoving(this, component, metadata))
                    return false;
            }

            lock (this)
            {
                if (!RemoveInternal(component, metadata))
                    return false;
            }

            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnRemoved(this, component, metadata);

            return true;
        }

        public bool Clear(IReadOnlyMetadataContext? metadata = null)
        {
            if (metadata == null)
                metadata = Default.MetadataContext;
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
            {
                if (!listeners[i].OnClearing(this, metadata))
                    return false;
            }

            var oldItems = Items;
            if (!ClearInternal(metadata))
                return false;

            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnCleared(this, oldItems, metadata);

            Array.Clear(oldItems, 0, oldItems.Length);
            return true;
        }

        #endregion

        #region Methods

        protected virtual bool AddInternal(T component, IReadOnlyMetadataContext metadata)
        {
            var array = new T[Items.Length + 1];
            Array.Copy(Items, array, Items.Length);
            array[array.Length - 1] = component;
            Items = array;
            return true;
        }

        protected virtual bool RemoveInternal(T component, IReadOnlyMetadataContext metadata)
        {
            T[]? array = null;
            for (var i = 0; i < Items.Length; i++)
            {
                if (array == null && EqualityComparer<T>.Default.Equals(component, Items[i]))
                {
                    array = new T[Items.Length - 1];
                    Array.Copy(Items, 0, array, 0, i);
                    continue;
                }

                if (array != null)
                    array[i - 1] = Items[i];
            }

            if (array != null)
                Items = array;
            return array != null;
        }

        protected virtual bool ClearInternal(IReadOnlyMetadataContext metadata)
        {
            Items = Default.EmptyArray<T>();
            return true;
        }

        protected IComponentCollectionListener[] GetListeners()
        {
            return _listeners.GetItemsOrDefault();
        }

        #endregion
    }
}