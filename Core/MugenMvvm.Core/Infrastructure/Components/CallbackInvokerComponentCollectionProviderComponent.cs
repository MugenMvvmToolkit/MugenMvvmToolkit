using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvm.Enums;
using MugenMvvm;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Components
{
    public static class CallbackInvokerComponentCollectionProviderComponent
    {
        #region Fields

        private static readonly Dictionary<Type, Func<object?, object?[], object?>?> AttachDelegates =
            new Dictionary<Type, Func<object?, object?[], object?>?>(MemberInfoEqualityComparer.Instance);

        private static readonly Dictionary<Type, Func<object?, object?[], object?>?> DetachDelegates =
            new Dictionary<Type, Func<object?, object?[], object?>?>(MemberInfoEqualityComparer.Instance);

        private static readonly MethodInfo AttachDetachMethodInfo = GetAttachDetachMethod();

        #endregion

        #region Methods

        public static IComponentCollectionListener<T> GetComponentCollectionListener<T>() where T : class
        {
            return CallbackComponentCollectionProviderListenerImpl<T>.Instance;
        }

        public static IComponentCollectionProviderListener GetComponentCollectionProviderListener()
        {
            return CallbackComponentCollectionProviderListenerImpl<object>.Instance;
        }

        private static MethodInfo GetAttachDetachMethod()
        {
            var m = typeof(CallbackInvokerComponentCollectionProviderComponent).GetMethodUnified(nameof(AttachDetachIml), MemberFlags.StaticOnly);
            Should.BeSupported(m != null, nameof(AttachDetachMethodInfo));
            return m;
        }

        private static void Attach<T>(IComponentCollection<T> collection, object component, IReadOnlyMetadataContext? metadata) where T : class
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

            func?.Invoke(null, new[] { collection.Owner, component, Default.TrueObject, metadata });
        }

        private static void Detach<T>(IComponentCollection<T> collection, object component, IReadOnlyMetadataContext? metadata) where T : class
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

            func?.Invoke(null, new[] { collection.Owner, component, Default.FalseObject, metadata });
        }

        private static Func<object?, object?[], object?>? GetAttachFunc(Type targetType, Type interfaceType)
        {
            Func<object?, object?[], object?>? result = null;
            foreach (var i in targetType.GetInterfacesUnified().Where(type => type.IsGenericTypeUnified()))
            {
                if (i.GetGenericTypeDefinition() != interfaceType)
                    continue;

                var methodInvoker = AttachDetachMethodInfo.MakeGenericMethod(i.GetGenericArgumentsUnified().First()).GetMethodInvoker();
                if (result == null)
                    result = methodInvoker;
                else
                    result += methodInvoker;
            }

            return result;
        }

        private static void AttachDetachIml<T>(object owner, object target, bool attach, IReadOnlyMetadataContext? metadata) where T : class
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

        private sealed class CallbackComponentCollectionProviderListenerImpl<T> : IComponentCollectionListener<T>, IComponentCollectionProviderListener
            where T : class
        {
            #region Fields

            public static readonly CallbackComponentCollectionProviderListenerImpl<T> Instance = new CallbackComponentCollectionProviderListenerImpl<T>();

            #endregion

            #region Implementation of interfaces

            public int GetPriority(object source)
            {
                return 0;
            }

            public bool OnAdding(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata)
            {
                if (collection.Owner is IComponentOwnerAddingCallback<T> callback)
                    return callback.OnComponentAdding(collection, component, metadata);
                return true;
            }

            public void OnAdded(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata)
            {
                if (component is IAttachableComponent)
                    Attach(collection, component, metadata);

                if (collection.Owner is IComponentOwnerAddedCallback<T> callback)
                    callback.OnComponentAdded(collection, component, metadata);
            }

            public bool OnRemoving(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata)
            {
                if (collection.Owner is IComponentOwnerRemovingCallback<T> callback)
                    return callback.OnComponentRemoving(collection, component, metadata);
                return true;
            }

            public void OnRemoved(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata)
            {
                if (component is IDetachableComponent)
                    Detach(collection, component, metadata);

                if (collection.Owner is IComponentOwnerRemovedCallback<T> callback)
                    callback.OnComponentRemoved(collection, component, metadata);
            }

            public bool OnClearing(IComponentCollection<T> collection, IReadOnlyMetadataContext? metadata)
            {
                if (collection.Owner is IComponentOwnerClearingCallback<T> callback)
                    return callback.OnComponentClearing(collection, collection.GetItems(), metadata);
                return true;
            }

            public void OnCleared(IComponentCollection<T> collection, T[] oldItems, IReadOnlyMetadataContext? metadata)
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

            public void OnComponentCollectionCreated<TItem>(IComponentCollectionProvider provider, IComponentCollection<TItem> componentCollection,
                IReadOnlyMetadataContext? metadata)
                where TItem : class
            {
                componentCollection.AddComponent(CallbackComponentCollectionProviderListenerImpl<TItem>.Instance, metadata);
            }

            #endregion
        }

        #endregion
    }
}