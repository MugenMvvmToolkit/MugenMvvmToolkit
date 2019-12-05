using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static void TryInvalidateCache<TComponent>(this IComponentOwner<TComponent>? owner, object? state = null, IReadOnlyMetadataContext? metadata = null)
            where TComponent : class
        {
            if (owner == null)
                return;
            (owner as IHasCache)?.Invalidate(state, metadata);
            var components = owner.GetComponents<IHasCache>(metadata);
            for (var i = 0; i < components.Length; i++)
                components[i].Invalidate(state, metadata);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DefaultIfNull<T>(this T? component) where T : class, IComponent
        {
            return component ?? MugenService.Instance<T>();
        }

        public static bool AddComponent<T>(this IComponentOwner<T> componentOwner, IComponent<T> component, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            return componentOwner.Components.Add(component, metadata);
        }

        public static bool RemoveComponent<T>(this IComponentOwner<T> componentOwner, IComponent<T> component, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            if (componentOwner.HasComponents)
                return componentOwner.Components.Remove(component, metadata);
            return false;
        }

        public static void ClearComponents(this IComponentOwner componentOwner, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            if (componentOwner.HasComponents)
                componentOwner.Components.Clear(metadata);
        }

        public static T[] GetComponents<T>(this IComponentOwner componentOwner, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            if (componentOwner.HasComponents)
                return componentOwner.Components.GetComponents<T>(metadata);
            return Default.EmptyArray<T>();
        }

        public static TComponent GetComponent<TComponent>(this IComponentOwner owner) where TComponent : class, IComponent
        {
            return owner.GetComponent<TComponent>(false)!;
        }

        public static TComponent? GetComponentOptional<TComponent>(this IComponentOwner owner) where TComponent : class, IComponent
        {
            return owner.GetComponent<TComponent>(true);
        }

        public static T[] GetComponentsOrDefault<T>(this IComponentCollection? collection, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            if (collection == null)
                return Default.EmptyArray<T>();
            return collection.GetComponents<T>(metadata);
        }

        public static bool LazyInitialize(this IComponentCollectionProvider? componentCollectionProvider, [NotNull] ref IComponentCollection? item, object target, IReadOnlyMetadataContext? metadata = null)
        {
            return item == null && LazyInitialize(ref item, componentCollectionProvider.DefaultIfNull().GetComponentCollection(target, metadata));
        }

        public static int GetPriority(this IComponent component, object? owner = null)
        {
            return GetComponentPriority(component, owner);
        }

        public static int GetComponentPriority(object component, object? owner)
        {
            var manager = MugenService.Optional<IComponentPriorityManager>();
            if (manager != null)
                return manager.GetPriority(component, owner);
            if (component is IHasPriority p)
                return p.Priority;
            return 0;
        }

        public static void ComponentDecoratorDecorate<TComponent>(TComponent decorator, object owner, IList<TComponent> components, IComparer<TComponent> comparer, ref TComponent[] decoratorComponents)
            where TComponent : class
        {
            var currentPriority = GetComponentPriority(decorator, owner);
            for (int i = 0; i < components.Count; i++)
            {
                var component = components[i];
                if (ReferenceEquals(decorator, component))
                    continue;

                var priority = GetComponentPriority(component, owner);
                if (priority < currentPriority)
                {
                    AddOrdered(ref decoratorComponents, component, comparer);
                    components.RemoveAt(i--);
                }
                else if (priority == currentPriority)
                    ExceptionManager.ThrowDecoratorComponentWithTheSamePriorityNotSupported(priority, decorator, component);
            }
        }

        public static void ComponentCollectionOnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IAttachableComponent attachable)
                attachable.OnAttached(collection.Owner, metadata);

            (collection.Owner as IHasAddedCallbackComponentOwner)?.OnComponentAdded(collection, component, metadata);
        }

        public static void ComponentCollectionOnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IDetachableComponent detachable)
                detachable.OnDetached(collection.Owner, metadata);

            (collection.Owner as IHasRemovedCallbackComponentOwner)?.OnComponentRemoved(collection, component, metadata);
        }

        public static bool ComponentCollectionOnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (collection.Owner is IHasAddingCallbackComponentOwner callback && !callback.OnComponentAdding(collection, component, metadata))
                return false;

            if (component is IAttachableComponent attachable)
                return attachable.OnAttaching(collection.Owner, metadata);
            return true;
        }

        public static bool ComponentCollectionOnComponentRemoving(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (collection.Owner is IHasRemovingCallbackComponentOwner callback && !callback.OnComponentRemoving(collection, component, metadata))
                return false;

            if (component is IDetachableComponent detachable)
                return detachable.OnDetaching(collection.Owner, metadata);
            return true;
        }

        private static TComponent? GetComponent<TComponent>(this IComponentOwner owner, bool optional)
            where TComponent : class, IComponent
        {
            Should.NotBeNull(owner, nameof(owner));
            var components = owner.GetComponents<TComponent>();
            if (components.Length != 0)
                return components[0];
            if (!optional)
                ExceptionManager.ThrowCannotGetComponent(owner, typeof(TComponent));
            return null;
        }

        #endregion
    }
}