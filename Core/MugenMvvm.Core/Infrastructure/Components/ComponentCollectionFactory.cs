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

            protected override void AddInternal(T component)
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

            private int GetPriority(T component)
            {
                if (component is IListener listener)
                    return listener.GetPriority(Owner);
                return ((IHasPriority) component).Priority;
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

            public void Add(T component)
            {
                Should.NotBeNull(component, nameof(component));
                lock (this)
                {
                    AddInternal(component);
                }

                IReadOnlyMetadataContext? metadata = null;

                if (component is IAttachableComponent attachable)
                {
                    metadata = GetAttachMetadata(component);
                    Attach(attachable, metadata);
                }

                if (Owner is IComponentOwner<T> owner)
                    owner.OnComponentAdded(component, metadata ?? GetAttachMetadata(component));
            }

            public bool Remove(T component)
            {
                Should.NotBeNull(component, nameof(component));
                bool removed;
                lock (this)
                {
                    removed = RemoveInternal(component);
                }

                IReadOnlyMetadataContext? metadata = null;

                if (removed && component is IDetachableComponent detachable)
                {
                    metadata = GetDetachMetadata(component);
                    Detach(detachable, metadata);
                }

                if (removed && Owner is IComponentOwner<T> owner)
                    owner.OnComponentRemoved(component, metadata ?? GetDetachMetadata(component));

                return removed;
            }

            public void Clear()
            {
                var oldItems = Items;
                ClearInternal();
                var owner = Owner as IComponentOwner<T>;
                for (var i = 0; i < oldItems.Length; i++)
                {
                    IReadOnlyMetadataContext? metadata = null;

                    var component = oldItems[i];
                    if (component is IDetachableComponent detachable)
                    {
                        metadata = GetDetachMetadata(component);
                        Detach(detachable, metadata);
                    }

                    owner?.OnComponentRemoved(component, metadata ?? GetDetachMetadata(component));
                }
            }

            public T[] GetItems()
            {
                return Items;
            }

            #endregion

            #region Methods

            protected virtual void AddInternal(T component)
            {
                var array = new T[Items.Length + 1];
                Array.Copy(Items, array, Items.Length);
                array[array.Length - 1] = component;
                Items = array;
            }

            protected virtual bool RemoveInternal(T component)
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

            protected virtual void ClearInternal()
            {
                Items = Default.EmptyArray<T>();
            }

            protected virtual void Attach(IAttachableComponent component, IReadOnlyMetadataContext metadata)
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

                func?.Invoke(null, new[] {Owner, component, Default.TrueObject, metadata});
            }

            protected virtual void Detach(IDetachableComponent component, IReadOnlyMetadataContext metadata)
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

                func?.Invoke(null, new[] {Owner, component, Default.FalseObject, metadata});
            }

            protected virtual IReadOnlyMetadataContext GetAttachMetadata(T component)
            {
                return Default.MetadataContext;
            }

            protected virtual IReadOnlyMetadataContext GetDetachMetadata(T component)
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