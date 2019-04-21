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
    public sealed class ComponentCollectionCallbackListener : IComponentCollectionListener, IComponentCollectionProviderListener
    {
        #region Fields

        private static readonly Dictionary<Type, Func<object?, object?[], object?>?> AttachDelegates;
        private static readonly Dictionary<Type, Func<object?, object?[], object?>?> DetachDelegates;
        private static readonly MethodInfo AttachDetachMethodInfo;

        #endregion

        #region Constructors

        static ComponentCollectionCallbackListener()
        {
            AttachDelegates = new Dictionary<Type, Func<object?, object?[], object?>?>(MemberInfoEqualityComparer.Instance);
            DetachDelegates = new Dictionary<Type, Func<object?, object?[], object?>?>(MemberInfoEqualityComparer.Instance);
            AttachDetachMethodInfo = typeof(ComponentCollectionCallbackListener).GetMethodUnified(nameof(AttachDetachIml), MemberFlags.StaticOnly);
            Should.BeSupported(AttachDetachMethodInfo != null, nameof(AttachDetachMethodInfo));
        }

        #endregion

        #region Properties

        public int ComponentCollectionProviderListenerPriority { get; set; }

        public int ComponentCollectionListenerPriority { get; set; }

        #endregion

        #region Implementation of interfaces

        int IListener.GetPriority(object source)
        {
            if (source is IComponentCollectionProvider)
                return ComponentCollectionProviderListenerPriority;
            return ComponentCollectionListenerPriority;
        }

        bool IComponentCollectionListener.OnAdding<T>(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext metadata)
        {
            if (collection.Owner is IComponentOwnerAddingCallback<T> callback)
                return callback.OnComponentAdding(collection, component, metadata);
            return true;
        }

        void IComponentCollectionListener.OnAdded<T>(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext metadata)
        {
            if (component is IAttachableComponent)
                Attach(collection, component, metadata);

            if (collection.Owner is IComponentOwnerAddedCallback<T> callback)
                callback.OnComponentAdded(collection, component, metadata);
        }

        bool IComponentCollectionListener.OnRemoving<T>(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext metadata)
        {
            if (collection.Owner is IComponentOwnerRemovingCallback<T> callback)
                return callback.OnComponentRemoving(collection, component, metadata);
            return true;
        }

        void IComponentCollectionListener.OnRemoved<T>(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext metadata)
        {
            if (component is IDetachableComponent)
                Detach(collection, component, metadata);

            if (collection.Owner is IComponentOwnerRemovedCallback<T> callback)
                callback.OnComponentRemoved(collection, component, metadata);
        }

        bool IComponentCollectionListener.OnClearing<T>(IComponentCollection<T> collection, IReadOnlyMetadataContext metadata)
        {
            if (collection.Owner is IComponentOwnerClearingCallback<T> callback)
                return callback.OnComponentClearing(collection, collection.GetItems(), metadata);
            return true;
        }

        void IComponentCollectionListener.OnCleared<T>(IComponentCollection<T> collection, T[] oldItems, IReadOnlyMetadataContext metadata)
        {
            var clearedCallback = collection.Owner as IComponentOwnerClearedCallback<T>;
            var removedCallback = clearedCallback == null ? collection.Owner as IComponentOwnerRemovedCallback<T> : null;
            for (var i = 0; i < oldItems.Length; i++)
            {
                var oldItem = oldItems[i];
                if (oldItem is IDetachableComponent)
                    Detach(collection, oldItem, metadata);
                removedCallback?.OnComponentRemoved(collection, oldItem, metadata);
            }

            clearedCallback?.OnComponentCleared(collection, oldItems, metadata);
        }

        void IComponentCollectionProviderListener.OnComponentCollectionCreated<T>(IComponentCollectionProvider provider, IComponentCollection<T> componentCollection,
            IReadOnlyMetadataContext metadata)
        {
            componentCollection.AddListener(this, metadata);
        }

        #endregion

        #region Methods

        private static void Attach<T>(IComponentCollection<T> collection, object component, IReadOnlyMetadataContext metadata) where T : class
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

            func?.Invoke(null, new[] {collection.Owner, component, Default.TrueObject, metadata});
        }

        private static void Detach<T>(IComponentCollection<T> collection, object component, IReadOnlyMetadataContext metadata) where T : class
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

            func?.Invoke(null, new[] {collection.Owner, component, Default.FalseObject, metadata});
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
    }
}