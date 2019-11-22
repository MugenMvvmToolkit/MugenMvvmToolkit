using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] GetItemsOrDefault<T>(this IComponentCollection<T>? componentCollection) where T : class
        {
            return componentCollection?.GetComponents() ?? Default.EmptyArray<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DefaultIfNull<T>(this T? component) where T : class, IComponent
        {
            return component ?? MugenService.Instance<T>();
        }

        public static void AddComponent<T>(this IComponentOwner<T> componentOwner, IComponent<T> component, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            componentOwner.Components.Add(component, metadata);
        }

        public static void RemoveComponent<T>(this IComponentOwner<T> componentOwner, IComponent<T> component, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            if (componentOwner.HasComponents)
                componentOwner.Components.Remove(component, metadata);
        }

        public static void ClearComponents<T>(this IComponentOwner<T> componentOwner, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            if (componentOwner.HasComponents)
                componentOwner.Components.Clear(metadata);
        }

        public static IComponent<T>[] GetComponents<T>(this IComponentOwner<T> componentOwner) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            if (componentOwner.HasComponents)
                return componentOwner.Components.GetComponents();
            return Default.EmptyArray<IComponent<T>>();
        }

        public static TType? GetComponent<T, TType>(this IComponentOwner<T> owner, bool optional)
            where T : class
            where TType : class, IComponent<T>
        {
            Should.NotBeNull(owner, nameof(owner));
            var components = owner.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is TType component)
                    return component;
            }

            if (!optional)
                ExceptionManager.ThrowCannotGetComponent(owner, typeof(T));
            return null;
        }

        public static bool LazyInitialize<T>(this IComponentCollectionProvider? componentCollectionProvider, [NotNull] ref IComponentCollection<T>? item, object target,
            IReadOnlyMetadataContext? metadata = null)
            where T : class
        {
            return item == null && LazyInitialize(ref item, componentCollectionProvider.DefaultIfNull().GetComponentCollection<T>(target, metadata));
        }

        public static int GetComponentPriority(object component, object? owner)
        {
            if (owner != null && component is IHasComponentPriority c)
                return c.GetPriority(owner);
            if (component is IHasPriority p)
                return p.Priority;
            return 0;
        }

        public static void ComponentTrackerOnAdded<TComponent, TComponentBase>(ref TComponent[] items,
            IComponentCollection<IComponent<TComponentBase>> collection, IComponent<TComponentBase> component)
            where TComponent : class
            where TComponentBase : class
        {
            if (component is TComponent c)
                AddComponentOrdered(ref items, c, collection.Owner);
        }

        public static void ComponentTrackerOnRemoved<TComponent, TComponentBase>(ref TComponent[] items, IComponent<TComponentBase> component)
            where TComponent : class
            where TComponentBase : class
        {
            if (component is TComponent c)
                Remove(ref items, c);
        }

        public static void SingletonComponentTrackerOnAdded<TComponent, TComponentBase>(ref TComponent? currentComponent, bool autoDetachOld,
            IComponentCollection<IComponent<TComponentBase>> collection, IComponent<TComponentBase> component, IReadOnlyMetadataContext? metadata)
            where TComponent : class
            where TComponentBase : class
        {
            if (!(component is TComponent c))
                return;

            if (autoDetachOld)
            {
                if (ReferenceEquals(c, currentComponent))
                    return;
                if (currentComponent != null)
                    collection.Remove((IComponent<TComponentBase>)currentComponent, metadata);
            }

            currentComponent = c;
        }

        public static void SingletonComponentTrackerOnRemoved<TComponent, TComponentBase>(ref TComponent? currentComponent, IComponent<TComponentBase> component)
            where TComponent : class
            where TComponentBase : class
        {
            if (ReferenceEquals(currentComponent, component))
                currentComponent = null;
        }

        public static void OnComponentAddedHandler<T>(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata) where T : class
        {
            if (component is IAttachableComponent attachable)
                attachable.OnAttached(collection.Owner, metadata);

            (collection.Owner as IComponentOwnerAddedCallback<T>)?.OnComponentAdded(collection, component, metadata);
        }

        public static void OnComponentRemovedHandler<T>(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata) where T : class
        {
            if (component is IDetachableComponent detachable)
                detachable.OnDetached(collection.Owner, metadata);

            (collection.Owner as IComponentOwnerRemovedCallback<T>)?.OnComponentRemoved(collection, component, metadata);
        }

        public static bool OnComponentAddingHandler<T>(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata) where T : class
        {
            if (collection.Owner is IComponentOwnerAddingCallback<T> callback && !callback.OnComponentAdding(collection, component, metadata))
                return false;

            if (component is IAttachableComponent attachable)
                return attachable.OnAttaching(collection.Owner, metadata);
            return true;
        }

        public static bool OnComponentRemovingHandler<T>(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata) where T : class
        {
            if (collection.Owner is IComponentOwnerRemovingCallback<T> callback && !callback.OnComponentRemoving(collection, component, metadata))
                return false;

            if (component is IDetachableComponent detachable)
                return detachable.OnDetaching(collection.Owner, metadata);
            return true;
        }

        #endregion
    }
}