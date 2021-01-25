using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class ComponentComponentExtensions
    {
        public static IComponentCollection? TryGetComponentCollection(this ItemOrArray<IComponentCollectionProviderComponent> components,
            IComponentCollectionManager collectionManager, object owner,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collectionManager, nameof(collectionManager));
            Should.NotBeNull(owner, nameof(owner));
            foreach (var c in components)
            {
                var collection = c.TryGetComponentCollection(collectionManager, owner, metadata);
                if (collection != null)
                    return collection;
            }

            return null;
        }

        public static void OnComponentCollectionCreated(this ItemOrArray<IComponentCollectionManagerListener> listeners, IComponentCollectionManager collectionManager,
            IComponentCollection collection,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collectionManager, nameof(collectionManager));
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnComponentCollectionCreated(collectionManager, collection, metadata);
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
                    if (comp != component)
                        (comp as IComponentCollectionChangedListener)?.OnAdded(collection, component, metadata);
                }
            }
            else if (components != component)
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
                    if (c[i] is IComponentCollectionChangingListener listener && listener != component && !listener.OnRemoving(collection, component, metadata))
                        return false;
                }
            }
            else if (components is IComponentCollectionChangingListener listener && listener != component && !listener.OnRemoving(collection, component, metadata))
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

        public static bool OnAdding(this ItemOrArray<IComponentCollectionChangingListener> listeners, IComponentCollection collection, object component,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            foreach (var c in listeners)
            {
                if (!c.OnAdding(collection, component, metadata))
                    return false;
            }

            return true;
        }

        public static void OnAdded(this ItemOrArray<IComponentCollectionChangedListener> listeners, IComponentCollection collection, object component,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            foreach (var c in listeners)
                c.OnAdded(collection, component, metadata);
        }

        public static bool OnRemoving(this ItemOrArray<IComponentCollectionChangingListener> listeners, IComponentCollection collection, object component,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            foreach (var c in listeners)
            {
                if (!c.OnRemoving(collection, component, metadata))
                    return false;
            }

            return true;
        }

        public static void OnRemoved(this ItemOrArray<IComponentCollectionChangedListener> listeners, IComponentCollection collection, object component,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            foreach (var c in listeners)
                c.OnRemoved(collection, component, metadata);
        }

        public static void Decorate<T>(this IComponentCollectionDecoratorBase[] decorators, IComponentCollection collection, ref ItemOrListEditor<T> components,
            IReadOnlyMetadataContext? metadata)
            where T : class
        {
            Should.NotBeNull(decorators, nameof(decorators));
            foreach (var decorator in decorators)
            {
                if (decorator is IComponentCollectionDecorator<T> d1)
                    d1.Decorate(collection, ref components, metadata);
                else if (decorator is IComponentCollectionDecorator d2)
                    d2.Decorate(collection, ref components, metadata);
            }
        }

        public static bool HasDecorators<T>(this IComponentCollectionDecoratorBase[] decorators, IReadOnlyMetadataContext? metadata) where T : class
        {
            Should.NotBeNull(decorators, nameof(decorators));
            foreach (var decorator in decorators)
            {
                if (decorator is IComponentCollectionDecorator<T>)
                    return true;
                if (decorator is IComponentCollectionDecorator d && d.CanDecorate<T>(metadata))
                    return true;
            }

            return false;
        }

        public static ItemOrArray<T> Decorate<T>(this IComponentCollectionDecorator<T> decorator, ref ItemOrListEditor<T> components)
            where T : class
        {
            Should.NotBeNull(decorator, nameof(decorator));
            if (decorator is not T decoratorComponent)
                return default;

            var index = components.IndexOf(decoratorComponent);
            if (index < 0)
                return default;

            ++index;
            var length = components.Count - index;
            if (length == 0)
                return default;
            if (length == 1)
            {
                var c = components[index];
                components.RemoveAt(index);
                return c;
            }

            var result = new T[length];
            var position = 0;
            for (var i = index; i < components.Count; i++)
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

            (collection.Owner as IHasComponentAddedHandler)?.OnComponentAdded(collection, component, metadata);
        }

        public static void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (component is IDetachableComponent detachable)
                detachable.OnDetached(collection.Owner, metadata);

            (collection.Owner as IHasComponentRemovedHandler)?.OnComponentRemoved(collection, component, metadata);
        }

        public static bool OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (collection.Owner is IHasComponentAddingHandler callback && !callback.OnComponentAdding(collection, component, metadata))
                return false;

            if (component is IAttachableComponent attachable)
                return attachable.OnAttaching(collection.Owner, metadata);
            return true;
        }

        public static bool OnComponentRemoving(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (collection.Owner is IHasComponentRemovingHandler callback && !callback.OnComponentRemoving(collection, component, metadata))
                return false;

            if (component is IDetachableComponent detachable)
                return detachable.OnDetaching(collection.Owner, metadata);
            return true;
        }
    }
}