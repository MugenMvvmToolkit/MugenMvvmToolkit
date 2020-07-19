using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions.Components
{
    public static class ComponentComponentExtensions
    {
        #region Methods

        public static IComponentCollection? TryGetComponentCollection(this IComponentCollectionProviderComponent[] components, IComponentCollectionManager collectionManager, object owner, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(collectionManager, nameof(collectionManager));
            Should.NotBeNull(owner, nameof(owner));
            for (int i = 0; i < components.Length; i++)
            {
                var collection = components[i].TryGetComponentCollection(collectionManager, owner, metadata);
                if (collection != null)
                    return collection;
            }

            return null;
        }

        public static void OnComponentCollectionCreated(this IComponentCollectionManagerListener[] listeners, IComponentCollectionManager collectionManager, IComponentCollection collection, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collectionManager, nameof(collectionManager));
            Should.NotBeNull(collection, nameof(collection));
            for (int i = 0; i < listeners.Length; i++)
                listeners[i].OnComponentCollectionCreated(collectionManager, collection, metadata);
        }

        public static bool HasComponent<TComponent>(object? components) where TComponent : class, IComponent
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is TComponent)
                        return true;
                }

                return false;
            }

            return components is TComponent;
        }

        public static bool OnComponentAdding(object? components, IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is IComponentCollectionChangingListener listener && !listener.OnAdding(collection, component, metadata))
                        return false;
                }
            }
            else if (components is IComponentCollectionChangingListener listener && !listener.OnAdding(collection, component, metadata))
                return false;

            return true;
        }

        public static void OnComponentAdded(object? components, IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    var comp = c[i];
                    if (!ReferenceEquals(comp, component))
                        (comp as IComponentCollectionChangedListener)?.OnAdded(collection, component, metadata);
                }
            }
            else if (!ReferenceEquals(components, component))
                (components as IComponentCollectionChangedListener)?.OnAdded(collection, component, metadata);
        }

        public static bool OnComponentRemoving(object? components, IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is IComponentCollectionChangingListener listener && !ReferenceEquals(listener, component) && !listener.OnRemoving(collection, component, metadata))
                        return false;
                }
            }
            else if (components is IComponentCollectionChangingListener listener && !ReferenceEquals(listener, component) && !listener.OnRemoving(collection, component, metadata))
                return false;

            return true;
        }

        public static void OnComponentRemoved(object? components, int startIndex, IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (components is object[] c)
            {
                for (var i = startIndex; i < c.Length; i++)
                    (c[i] as IComponentCollectionChangedListener)?.OnRemoved(collection, component, metadata);
            }
            else if (startIndex == 0)
                (components as IComponentCollectionChangedListener)?.OnRemoved(collection, component, metadata);
        }

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

        public static TComponent[] Decorate<TComponent>(this IComponentCollectionDecorator[] decorators, IComponentCollection collection, List<TComponent> components, IReadOnlyMetadataContext? metadata) where TComponent : class
        {
            Should.NotBeNull(decorators, nameof(decorators));
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < decorators.Length; i++)
            {
                if (decorators[i] is IComponentCollectionDecorator<TComponent> decorator)
                    decorator.Decorate(collection, components, metadata);
            }

            return components.ToArray();
        }

        public static TComponent[] Decorate<TComponent>(this IComponentCollectionDecorator<TComponent>[] decorators, IComponentCollection collection, List<TComponent> components, IReadOnlyMetadataContext? metadata) where TComponent : class
        {
            Should.NotBeNull(decorators, nameof(decorators));
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < decorators.Length; i++)
                decorators[i].Decorate(collection, components, metadata);
            return components.ToArray();
        }

        public static bool HasDecorators<TComponent>(this IComponentCollectionDecorator[] decorators) where TComponent : class
        {
            Should.NotBeNull(decorators, nameof(decorators));
            for (var i = 0; i < decorators.Length; i++)
            {
                if (decorators[i] is IComponentCollectionDecorator<TComponent>)
                    return true;
            }

            return false;
        }

        public static TComponent[] Decorate<TComponent>(this IComponentCollectionDecorator<TComponent> decorator, IList<TComponent> components)
            where TComponent : class
        {
            Should.NotBeNull(decorator, nameof(decorator));
            Should.NotBeNull(components, nameof(components));
            if (!(decorator is TComponent decoratorComponent))
                return Default.Array<TComponent>();

            var index = components.IndexOf(decoratorComponent);
            if (index < 0)
                return Default.Array<TComponent>();

            ++index;
            var length = components.Count - index;
            if (length == 0)
                return Default.Array<TComponent>();

            var result = new TComponent[length];
            int position = 0;
            for (int i = index; i < components.Count; i++)
            {
                result[position++] = components[i];
                components.RemoveAt(i);
                --i;
            }

            return result;
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