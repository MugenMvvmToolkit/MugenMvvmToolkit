using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Infrastructure.Components
{
    public class ComponentCollectionFactory : IComponentCollectionFactory
    {
        #region Fields

        private static readonly Dictionary<Type, Func<object?, object?[], object?>?> AttachDelegates;
        private static readonly Dictionary<Type, Func<object?, object?[], object?>?> DetachDelegates;
        private static readonly MethodInfo AttachDetachMethodInfo;

        #endregion

        #region Constructors

        static ComponentCollectionFactory()
        {
            AttachDelegates = new Dictionary<Type, Func<object?, object?[], object?>?>(MemberInfoEqualityComparer.Instance);
            DetachDelegates = new Dictionary<Type, Func<object?, object?[], object?>?>(MemberInfoEqualityComparer.Instance);
            AttachDetachMethodInfo = typeof(ComponentCollectionFactory).GetMethodUnified(nameof(AttachDetachIml), MemberFlags.StaticOnly);
            Should.BeSupported(AttachDetachMethodInfo != null, nameof(AttachDetachMethodInfo));
        }

        #endregion

        #region Implementation of interfaces

        public IComponentCollection<T> GetComponentCollection<T>(object owner, IReadOnlyMetadataContext metadata) where T : class
        {
            Should.NotBeNull(owner, nameof(owner));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetComponentCollectionInternal<T>(owner, metadata);
        }

        #endregion

        #region Methods

        protected virtual IComponentCollection<T> GetComponentCollectionInternal<T>(object owner, IReadOnlyMetadataContext metadata) where T : class
        {
            if (typeof(IHasPriority).IsAssignableFromUnified(typeof(T)) || typeof(IListener).IsAssignableFromUnified(typeof(T)))
                return new OrderedArrayComponentCollection<T>(owner);
            return new ArrayComponentCollection<T>(owner);
        }

        private static void AttachDetachIml<T>(object owner, object target, bool attach, IReadOnlyMetadataContext metadata) where T : class
        {
            if (attach)
            {
                if (target is IAttachableComponent<T> component && owner is T value)
                    component.OnAttached(value, metadata);
            }
            else
            {
                if (target is IDetachableComponent<T> component && owner is T value)
                    component.OnDetached(value, metadata);
            }
        }

        #endregion

        #region Nested types

        public class OrderedArrayComponentCollection<T> : ArrayComponentCollection<T> where T : class
        {
            #region Constructors

            public OrderedArrayComponentCollection(object owner) : base(owner)
            {
            }

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
                    {
                        array[i] = item;
                        added = true;
                        --i;
                    }
                    else
                        array[i] = oldItem;
                }

                if (!added)
                    array[array.Length - 1] = item;
                Items = array;
            }

            private int GetPriority(T item)
            {
                if (item is IListener listener)
                    return listener.GetPriority(Owner);
                return ((IHasPriority)item).Priority;
            }

            #endregion
        }

        public class ArrayComponentCollection<T> : IComponentCollection<T> where T : class
        {
            #region Fields

            protected readonly object Owner;

            protected T[] Items;

            #endregion

            #region Constructors

            public ArrayComponentCollection(object owner)
            {
                Owner = owner;
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

                if (item is IAttachableComponent attachable)
                    Attach(attachable);
            }

            public void Remove(T item)
            {
                Should.NotBeNull(item, nameof(item));
                lock (this)
                {
                    RemoveInternal(item);
                }

                if (item is IDetachableComponent detachable)
                    Detach(detachable);
            }

            public void Clear()
            {
                Items = Default.EmptyArray<T>();
            }

            public T[] GetItems()
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
                Items = array;
            }

            protected virtual void RemoveInternal(T item)
            {
                T[]? array = null;
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

            protected virtual void Attach(IAttachableComponent component)
            {
                var type = component.GetType();
                Func<object?, object?[], object?>? func;
                lock (AttachDelegates)
                {
                    if (!AttachDelegates.TryGetValue(type, out func))
                    {
                        func = GetAttachFunc(type, typeof(IAttachableComponent<>));
                        AttachDelegates[type] = func;
                    }
                }

                func?.Invoke(null, new[] { Owner, component, Default.TrueObject, GetAttachMetadata() });
            }

            protected virtual void Detach(IDetachableComponent component)
            {
                var type = component.GetType();
                Func<object?, object?[], object?>? func;
                lock (DetachDelegates)
                {
                    if (!DetachDelegates.TryGetValue(type, out func))
                    {
                        func = GetAttachFunc(type, typeof(IDetachableComponent<>));
                        DetachDelegates[type] = func;
                    }
                }

                func?.Invoke(null, new[] { Owner, component, Default.FalseObject, GetDetachMetadata() });
            }

            protected virtual IReadOnlyMetadataContext GetAttachMetadata()
            {
                return Default.MetadataContext;
            }

            protected virtual IReadOnlyMetadataContext GetDetachMetadata()
            {
                return Default.MetadataContext;
            }

            private static Func<object?, object?[], object?>? GetAttachFunc(Type targetType, Type interfaceType)
            {
                Func<object?, object?[], object?>? result = null;
                foreach (var i in targetType.GetInterfacesUnified().Where(type => type.IsGenericTypeUnified()))
                {
                    if (i.GetGenericTypeDefinition() != interfaceType)
                        continue;

                    var methodDelegate = Service<IReflectionManager>.Instance.GetMethodDelegate(AttachDetachMethodInfo.MakeGenericMethod(i.GetGenericArgumentsUnified()[0]));
                    if (result == null)
                        result = methodDelegate;
                    else
                        result += methodDelegate;
                }

                return result;
            }

            #endregion
        }

        #endregion
    }
}