using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Components
{
    public static class CallbackInvokerComponentCollectionComponent
    {
        #region Fields

        private static readonly Dictionary<Type, Func<object?, object?[], object?>?> AttachDelegates =
            new Dictionary<Type, Func<object?, object?[], object?>?>(MemberInfoEqualityComparer.Instance);

        private static readonly Dictionary<Type, Func<object?, object?[], object?>?> DetachDelegates =
            new Dictionary<Type, Func<object?, object?[], object?>?>(MemberInfoEqualityComparer.Instance);

        private static readonly MethodInfo AttachDetachMethodInfo = GetAttachDetachMethod();

        #endregion

        #region Methods

        public static ComponentCollectionListener<TItem> GetComponentCollectionListener<TItem>() where TItem : class
        {
            return ComponentCollectionListener<TItem>.Instance;
        }

        private static MethodInfo GetAttachDetachMethod()
        {
            var m = typeof(CallbackInvokerComponentCollectionComponent).GetMethodUnified(nameof(AttachDetachIml), MemberFlags.StaticOnly);
            Should.BeSupported(m != null, nameof(AttachDetachMethodInfo));
            return m;
        }

        private static bool Attach<T>(IComponentCollection<T> collection, object component, bool preBind, IReadOnlyMetadataContext? metadata) where T : class
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

            if (func == null)
                return true;
            return (bool)func.Invoke(null, new[] { collection.Owner, component, Default.TrueObject, Default.BoolToObject(preBind), metadata });
        }

        private static bool Detach<T>(IComponentCollection<T> collection, object component, bool preBind, IReadOnlyMetadataContext? metadata) where T : class
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

            if (func == null)
                return true;

            return (bool)func.Invoke(null, new[] { collection.Owner, component, Default.FalseObject, Default.BoolToObject(preBind), metadata });
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

        private static object AttachDetachIml<T>(object owner, object target, bool attach, bool preBind, IReadOnlyMetadataContext? metadata) where T : class
        {
            if (attach)
            {
                if (target is IAttachableComponent<T> component && owner is T value)
                {
                    if (preBind)
                        return Default.BoolToObject(component.OnAttaching(value, metadata));
                    component.OnAttached(value, metadata);
                }
            }
            else
            {
                if (target is IDetachableComponent<T> component && owner is T value)
                {
                    if (preBind)
                        return Default.BoolToObject(component.OnDetaching(value, metadata));
                    component.OnDetached(value, metadata);
                }
            }

            return Default.TrueObject;
        }

        #endregion

        #region Nested types

        public sealed class ComponentCollectionListener<T> : IComponentCollectionChangedListener<T>, IComponentCollectionProviderListener
            where T : class
        {
            #region Fields

            public static readonly ComponentCollectionListener<T> Instance = new ComponentCollectionListener<T>();

            #endregion

            #region Implementation of interfaces

            public int GetPriority(object source)
            {
                return 0;
            }

            public void OnAdded(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata)
            {
                if (component is IAttachableComponent)
                    Attach(collection, component, false, metadata);

                if (collection.Owner is IComponentOwnerAddedCallback<T> callback)
                    callback.OnComponentAdded(collection, component, metadata);
            }

            public void OnRemoved(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata)
            {
                if (component is IDetachableComponent)
                    Detach(collection, component, false, metadata);

                if (collection.Owner is IComponentOwnerRemovedCallback<T> callback)
                    callback.OnComponentRemoved(collection, component, metadata);
            }

            public void OnCleared(IComponentCollection<T> collection, T[] oldItems, IReadOnlyMetadataContext? metadata)
            {
                var clearedCallback = collection.Owner as IComponentOwnerClearedCallback<T>;
                var removedCallback = clearedCallback == null ? collection.Owner as IComponentOwnerRemovedCallback<T> : null;
                for (var i = 0; i < oldItems.Length; i++)
                {
                    var oldItem = oldItems[i];
                    if (oldItem is IDetachableComponent)
                        Detach(collection, oldItem, false, metadata);
                    removedCallback?.OnComponentRemoved(collection, oldItem, metadata);
                }

                clearedCallback?.OnComponentCleared(collection, oldItems, metadata);
            }

            public void OnComponentCollectionCreated<TItem>(IComponentCollectionProvider provider, IComponentCollection<TItem> componentCollection,
                IReadOnlyMetadataContext? metadata)
                where TItem : class
            {
                componentCollection.AddComponent(ComponentCollectionListener<TItem>.Instance, metadata);
            }

            #endregion

            #region Methods

            public bool OnAdding(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata)
            {
                if (collection.Owner is IComponentOwnerAddingCallback<T> callback
                    && !callback.OnComponentAdding(collection, component, metadata))
                    return false;

                return Attach(collection, component, true, metadata);
            }

            public bool OnRemoving(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata)
            {
                if (collection.Owner is IComponentOwnerRemovingCallback<T> callback
                    && !callback.OnComponentRemoving(collection, component, metadata))
                    return false;
                return Detach(collection, component, true, metadata);
            }

            public bool OnClearing(IComponentCollection<T> collection, IReadOnlyMetadataContext? metadata)
            {
                if (collection.Owner is IComponentOwnerClearingCallback<T> callback)
                    return callback.OnComponentClearing(collection, collection.GetItems(), metadata);
                return true;
            }

            #endregion
        }

        #endregion
    }
}