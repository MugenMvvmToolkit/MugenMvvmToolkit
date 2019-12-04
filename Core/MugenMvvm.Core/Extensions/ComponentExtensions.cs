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

        public static void ComponentDecoratorOnAdded<TComponent>(object decorator, IComponentCollection collection, object component,
            [NotNullIfNotNull("decoratorComponents")]ref TComponent[]? decoratorComponents, [NotNullIfNotNull("components")]ref TComponent[]? components)
            where TComponent : class
        {
            Should.NotBeNull(collection, nameof(collection));
            if (component is TComponent c)
                AddDecoratorComponent(decorator, GetComponentPriority(decorator, collection.Owner), c, collection.Owner, ref decoratorComponents, ref components);
        }

        public static void ComponentDecoratorOnRemoved<TComponent>(object decorator, object component,
            [NotNullIfNotNull("decoratorComponents")]ref TComponent[]? decoratorComponents, [NotNullIfNotNull("components")]ref TComponent[]? components)
            where TComponent : class
        {
            if (!(component is TComponent c))
                return;
            if (decoratorComponents != null)
                Remove(ref decoratorComponents, c);
            if (components != null)
                Remove(ref components, c);
        }

        public static void ComponentDecoratorInitialize<TComponent>(object decorator, IComponentOwner owner, IReadOnlyMetadataContext? metadata,
            [NotNullIfNotNull("decoratorComponents")]ref TComponent[]? decoratorComponents, [NotNullIfNotNull("components")]ref TComponent[]? components)
            where TComponent : class
        {
            Should.NotBeNull(owner, nameof(owner));
            var currentPriority = MugenExtensions.GetComponentPriority(decorator, owner);
            var allComponents = owner.Components.GetComponents<object>(metadata);

            for (int i = 0; i < allComponents.Length; i++)
            {
                if (allComponents[i] is TComponent c)
                    AddDecoratorComponent(decorator, currentPriority, c, owner, ref decoratorComponents, ref components);
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

        private static void AddDecoratorComponent<TComponent>(object decorator, int currentPriority, TComponent component, object owner,
            [NotNullIfNotNull("decoratorComponents")]ref TComponent[]? decoratorComponents, [NotNullIfNotNull("components")]ref TComponent[]? components)
            where TComponent : class
        {
            var priority = GetComponentPriority(component, owner);
            if (decoratorComponents != null)
            {
                if (ReferenceEquals(component, decorator) || priority > currentPriority)
                    AddComponentOrdered(ref decoratorComponents, component, owner);
                else if (priority == currentPriority)
                    ExceptionManager.ThrowDecoratorComponentWithTheSamePriorityNotSupported(priority, decorator, component);
            }

            if (components != null && !ReferenceEquals(decorator, component))
            {
                if (priority < currentPriority)
                    AddComponentOrdered(ref components, component, owner);
                else if (priority == currentPriority)
                    ExceptionManager.ThrowDecoratorComponentWithTheSamePriorityNotSupported(priority, decorator, component);
            }
        }

        #endregion
    }
}