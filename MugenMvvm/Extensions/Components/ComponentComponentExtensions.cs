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

        public static bool CanAdd(object? components, IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is IConditionComponentCollectionComponent condition && !condition.CanAdd(collection, component, metadata))
                        return false;
                }
            }
            else if (components is IConditionComponentCollectionComponent condition && !condition.CanAdd(collection, component, metadata))
                return false;

            return true;
        }

        public static void OnComponentAdding(object? components, IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    var comp = c[i];
                    if (comp != component)
                        (comp as IComponentCollectionChangingListener)?.OnAdding(collection, component, metadata);
                }
            }
            else if (components != component)
                (components as IComponentCollectionChangingListener)?.OnAdding(collection, component, metadata);
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

        public static bool CanRemove(object? components, IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is IConditionComponentCollectionComponent condition && condition != component && !condition.CanRemove(collection, component, metadata))
                        return false;
                }
            }
            else if (components is IConditionComponentCollectionComponent condition && condition != component && !condition.CanRemove(collection, component, metadata))
                return false;

            return true;
        }

        public static void OnComponentRemoving(object? components, IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    var comp = c[i];
                    if (comp != component)
                        (comp as IComponentCollectionChangingListener)?.OnRemoving(collection, component, metadata);
                }
            }
            else if (components != component)
                (components as IComponentCollectionChangingListener)?.OnRemoving(collection, component, metadata);
        }

        public static void OnComponentRemoved(object? components, IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    var comp = c[i];
                    if (comp != component)
                        (comp as IComponentCollectionChangedListener)?.OnRemoved(collection, component, metadata);
                }
            }
            else if (components != component)
                (components as IComponentCollectionChangedListener)?.OnRemoved(collection, component, metadata);
        }

        public static bool CanAdd(this ItemOrArray<IConditionComponentCollectionComponent> components, IComponentCollection collection, object component,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            foreach (var c in components)
            {
                if (!c.CanAdd(collection, component, metadata))
                    return false;
            }

            return true;
        }

        public static void OnAdding(this ItemOrArray<IComponentCollectionChangingListener> listeners, IComponentCollection collection, object component,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            foreach (var c in listeners)
                c.OnAdding(collection, component, metadata);
        }

        public static void OnAdded(this ItemOrArray<IComponentCollectionChangedListener> listeners, IComponentCollection collection, object component,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            foreach (var c in listeners)
                c.OnAdded(collection, component, metadata);
        }

        public static bool CanRemove(this ItemOrArray<IConditionComponentCollectionComponent> components, IComponentCollection collection, object component,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            foreach (var c in components)
            {
                if (!c.CanRemove(collection, component, metadata))
                    return false;
            }

            return true;
        }

        public static void OnRemoving(this ItemOrArray<IComponentCollectionChangingListener> listeners, IComponentCollection collection, object component,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            foreach (var c in listeners)
                c.OnRemoving(collection, component, metadata);
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
            (component as IAttachableComponent)?.OnAttached(collection.Owner, metadata);
            (collection.Owner as IHasComponentAddedHandler)?.OnComponentAdded(collection, component, metadata);
        }

        public static void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            (collection.Owner as IHasComponentRemovedHandler)?.OnComponentRemoved(collection, component, metadata);
            (component as IDetachableComponent)?.OnDetached(collection.Owner, metadata);
        }

        public static void OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            (component as IAttachableComponent)?.OnAttaching(collection.Owner, metadata);
            (collection.Owner as IHasComponentAddingHandler)?.OnComponentAdding(collection, component, metadata);
        }

        public static void OnComponentRemoving(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            (collection.Owner as IHasComponentRemovingHandler)?.OnComponentRemoving(collection, component, metadata);
            (component as IDetachableComponent)?.OnDetaching(collection.Owner, metadata);
        }

        public static bool CanAdd(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (collection.Owner is IHasComponentAddConditionHandler callback && !callback.CanAddComponent(collection, component, metadata))
                return false;

            if (component is IHasAttachConditionComponent attachable)
                return attachable.CanAttach(collection.Owner, metadata);
            return true;
        }

        public static bool CanRemove(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(component, nameof(component));
            if (collection.Owner is IHasComponentRemoveConditionHandler callback && !callback.CanRemoveComponent(collection, component, metadata))
                return false;

            if (component is IHasDetachConditionComponent detachable)
                return detachable.CanDetach(collection.Owner, metadata);
            return true;
        }
    }
}