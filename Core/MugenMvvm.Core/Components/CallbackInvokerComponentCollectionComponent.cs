using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public static class CallbackInvokerComponentCollectionComponent
    {
        #region Methods

        public static ComponentCollectionListener<TItem> GetComponentCollectionListener<TItem>() where TItem : class
        {
            return ComponentCollectionListener<TItem>.Instance;
        }

        #endregion

        #region Nested types

        public sealed class ComponentCollectionListener<T> : IComponentCollectionChangedListener<T>, IComponentCollectionChangingListener<T>, IComponentCollectionProviderListener
            where T : class
        {
            #region Fields

            public static readonly ComponentCollectionListener<T> Instance = new ComponentCollectionListener<T>();

            #endregion

            #region Implementation of interfaces

            public void OnAdded(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata)
            {
                if (component is IAttachableComponent attachable)
                    attachable.OnAttached(collection.Owner, metadata);

                (collection.Owner as IComponentOwnerAddedCallback<T>)?.OnComponentAdded(collection, component, metadata);
            }

            public void OnRemoved(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata)
            {
                if (component is IDetachableComponent detachable)
                    detachable.OnDetached(collection.Owner, metadata);

                (collection.Owner as IComponentOwnerRemovedCallback<T>)?.OnComponentRemoved(collection, component, metadata);
            }

            public bool OnAdding(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata)
            {
                if (collection.Owner is IComponentOwnerAddingCallback<T> callback && !callback.OnComponentAdding(collection, component, metadata))
                    return false;

                if (component is IAttachableComponent attachable)
                    return attachable.OnAttaching(collection.Owner, metadata);
                return true;
            }

            public bool OnRemoving(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata)
            {
                if (collection.Owner is IComponentOwnerRemovingCallback<T> callback && !callback.OnComponentRemoving(collection, component, metadata))
                    return false;

                if (component is IDetachableComponent detachable)
                    return detachable.OnDetaching(collection.Owner, metadata);
                return true;
            }

            public void OnComponentCollectionCreated<TItem>(IComponentCollectionProvider provider, IComponentCollection<TItem> componentCollection,
                IReadOnlyMetadataContext? metadata)
                where TItem : class
            {
                componentCollection.AddComponent(ComponentCollectionListener<TItem>.Instance, metadata);
            }

            #endregion
        }

        #endregion
    }
}