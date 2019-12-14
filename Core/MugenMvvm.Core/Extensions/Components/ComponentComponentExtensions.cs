using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class ComponentComponentExtensions
    {
        #region Methods

        public static bool OnAdding(this IComponentCollectionChangingListener[] listeners, IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            for (var i = 0; i < listeners.Length; i++)
            {
                if (!listeners[i].OnAdding(collection, component, metadata))
                    return false;
            }

            return true;
        }

        public static void OnAdded(this IComponentCollectionChangedListener[] listeners, IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnAdded(collection, component, metadata);
        }

        public static bool OnRemoving(this IComponentCollectionChangingListener[] listeners, IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            for (var i = 0; i < listeners.Length; i++)
            {
                if (!listeners[i].OnRemoving(collection, component, metadata))
                    return false;
            }

            return true;
        }

        public static void OnRemoved(this IComponentCollectionChangedListener[] listeners, IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnRemoved(collection, component, metadata);
        }

        public static TComponent[] Decorate<TComponent>(this IDecoratorComponentCollectionComponent[] decorators, List<TComponent> components, IReadOnlyMetadataContext? metadata) where TComponent : class
        {
            Should.NotBeNull(decorators, nameof(decorators));
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < decorators.Length; i++)
            {
                if (decorators[i] is IDecoratorComponentCollectionComponent<TComponent> decorator)
                    decorator.Decorate(components, metadata);
            }

            return components.ToArray();
        }

        public static TComponent[] Decorate<TComponent>(this IDecoratorComponentCollectionComponent<TComponent>[] decorators, List<TComponent> components, IReadOnlyMetadataContext? metadata) where TComponent : class
        {
            Should.NotBeNull(decorators, nameof(decorators));
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < decorators.Length; i++)
                decorators[i].Decorate(components, metadata);
            return components.ToArray();
        }

        public static bool HasDecorators<TComponent>(this IDecoratorComponentCollectionComponent[] decorators) where TComponent : class
        {
            Should.NotBeNull(decorators, nameof(decorators));
            for (var i = 0; i < decorators.Length; i++)
            {
                if (decorators[i] is IDecoratorComponentCollectionComponent<TComponent>)
                    return true;
            }

            return false;
        }

        public static void Decorate<TComponent>(this IDecoratorComponentCollectionComponent<TComponent> decorator, object owner, IList<TComponent> components, IComparer<TComponent> comparer, ref TComponent[] decoratorComponents)
            where TComponent : class
        {
            Should.NotBeNull(decorator, nameof(decorator));
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(comparer, nameof(comparer));
            Should.NotBeNull(decoratorComponents, nameof(decoratorComponents));
            var currentPriority = MugenExtensions.GetComponentPriority(decorator, owner);
            for (var i = 0; i < components.Count; i++)
            {
                var component = components[i];
                if (ReferenceEquals(decorator, component))
                    continue;

                var priority = MugenExtensions.GetComponentPriority(component, owner);
                if (priority < currentPriority)
                {
                    MugenExtensions.AddOrdered(ref decoratorComponents, component, comparer);
                    components.RemoveAt(i--);
                }
                else if (priority == currentPriority)
                    ExceptionManager.ThrowDecoratorComponentWithTheSamePriorityNotSupported(priority, decorator, component);
            }
        }

        public static void OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (component is IAttachableComponent attachable)
                attachable.OnAttached(collection.Owner, metadata);

            (collection.Owner as IHasAddedCallbackComponentOwner)?.OnComponentAdded(collection, component, metadata);
        }

        public static void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (component is IDetachableComponent detachable)
                detachable.OnDetached(collection.Owner, metadata);

            (collection.Owner as IHasRemovedCallbackComponentOwner)?.OnComponentRemoved(collection, component, metadata);
        }

        public static bool OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (collection.Owner is IHasAddingCallbackComponentOwner callback && !callback.OnComponentAdding(collection, component, metadata))
                return false;

            if (component is IAttachableComponent attachable)
                return attachable.OnAttaching(collection.Owner, metadata);
            return true;
        }

        public static bool OnComponentRemoving(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (collection.Owner is IHasRemovingCallbackComponentOwner callback && !callback.OnComponentRemoving(collection, component, metadata))
                return false;

            if (component is IDetachableComponent detachable)
                return detachable.OnDetaching(collection.Owner, metadata);
            return true;
        }

        #endregion
    }
}