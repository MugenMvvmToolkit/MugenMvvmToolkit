using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class EntityComponentExtensions
    {
        #region Methods

        public static IEntityStateSnapshot? TryGetSnapshot<TState>(this IEntityStateSnapshotProviderComponent[] components, object entity, in TState state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(entity, nameof(entity));
            for (var i = 0; i < components.Length; i++)
            {
                var snapshot = components[i].TryGetSnapshot(entity, state, metadata);
                if (snapshot != null)
                    return snapshot;
            }

            return null;
        }

        public static IEntityTrackingCollection? TryGetTrackingCollection<TRequest>(this IEntityTrackingCollectionProviderComponent[] components, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var collection = components[i].TryGetTrackingCollection(request, metadata);
                if (collection != null)
                    return collection;
            }

            return null;
        }

        public static void OnSnapshotCreated<TState>(this IEntityManagerListener[] listeners, IEntityManager entityManager, IEntityStateSnapshot snapshot, object entity, in TState state,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(entityManager, nameof(entityManager));
            Should.NotBeNull(snapshot, nameof(snapshot));
            Should.NotBeNull(entity, nameof(entity));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnSnapshotCreated(entityManager, snapshot, entity, state, metadata);
        }

        public static void OnTrackingCollectionCreated<TRequest>(this IEntityManagerListener[] listeners, IEntityManager entityManager, IEntityTrackingCollection collection, in TRequest request,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(entityManager, nameof(entityManager));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnTrackingCollectionCreated(entityManager, collection, request, metadata);
        }

        public static EntityState OnEntityStateChanging(this IEntityStateChangingListener[] listeners, IEntityTrackingCollection collection, object entity, EntityState from, EntityState to,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(entity, nameof(entity));
            Should.NotBeNull(to, nameof(from));
            for (var i = 0; i < listeners.Length; i++)
                to = listeners[i].OnEntityStateChanging(collection, entity, from, to, metadata);
            return to;
        }

        public static void OnEntityStateChanged(this IEntityStateChangedListener[] listeners, IEntityTrackingCollection collection, object entity, EntityState from, EntityState to,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(entity, nameof(entity));
            Should.NotBeNull(to, nameof(from));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnEntityStateChanged(collection, entity, from, to, metadata);
        }

        #endregion
    }
}