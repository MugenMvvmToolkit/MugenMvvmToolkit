using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Infrastructure.Internal
{
    public class ComponentCollectionFactory : IComponentCollectionFactory
    {
        #region Implementation of interfaces

        public IComponentCollection<T> GetComponentCollection<T>(object target, IReadOnlyMetadataContext metadata) where T : class
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetComponentCollectionInternal<T>(target, metadata);
        }

        #endregion

        #region Methods

        protected virtual IComponentCollection<T> GetComponentCollectionInternal<T>(object target, IReadOnlyMetadataContext metadata) where T : class
        {
            if (typeof(T).IsAssignableFromUnified(typeof(IHasPriority)) || typeof(T).IsAssignableFromUnified(typeof(IListener)))
                return new ListenersArrayComponentCollection<T>(target);
            return new ArrayComponentCollection<T>();
        }

        #endregion

        #region Nested types

        public class ListenersArrayComponentCollection<T> : ArrayComponentCollection<T> where T : class
        {
            #region Constructors

            public ListenersArrayComponentCollection(object target)
            {
                Target = target;
            }

            #endregion

            #region Properties

            private object Target { get; }

            #endregion

            #region Methods

            protected override void AddInternal(T item)
            {
                var array = new T[Items.Length + 1];
                var added = false;
                var priority = GetPriority(item);
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
                        array[i] = oldItem;
                    else
                    {
                        array[i] = item;
                        added = true;
                        --i;
                    }
                }

                if (!added)
                    array[array.Length - 1] = item;
            }

            private int GetPriority(T item)
            {
                if (item is IListener listener)
                    return listener.GetPriority(Target);
                return ((IHasPriority) item).Priority;
            }

            #endregion
        }

        public class ArrayComponentCollection<T> : IComponentCollection<T> where T : class
        {
            #region Fields

            protected T[] Items;

            #endregion

            #region Constructors

            public ArrayComponentCollection()
            {
                Items = Default.EmptyArray<T>();
            }

            #endregion

            #region Properties

            public bool HasItems => Items.Length > 0;

            #endregion

            #region Implementation of interfaces

            public void Add(T item)
            {
                Should.NotBeNull(item, nameof(item));
                lock (this)
                {
                    AddInternal(item);
                }
            }

            public void Remove(T item)
            {
                Should.NotBeNull(item, nameof(item));
                lock (this)
                {
                    RemoveInternal(item);
                }
            }

            public void Clear()
            {
                Items = Default.EmptyArray<T>();
            }

            public IReadOnlyList<T> GetItems()
            {
                return Items;
            }

            #endregion

            #region Methods

            protected virtual void AddInternal(T item)
            {
                var array = new T[Items.Length + 1];
                Array.Copy(Items, array, Items.Length);
                array[array.Length - 1] = item;
            }

            protected virtual void RemoveInternal(T item)
            {
                T[] ? array = null;
                for (var i = 0; i < Items.Length; i++)
                {
                    if (array == null && EqualityComparer<T>.Default.Equals(item, Items[i]))
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
            }

            #endregion
        }

        #endregion
    }
}