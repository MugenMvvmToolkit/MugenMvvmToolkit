using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class EntityComponentExtensions
    {
        #region Methods

        public static IEntityStateSnapshot? TryGetSnapshot(this IEntityStateSnapshotProviderComponent[] components, IEntityManager entityManager, object entity, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(entity, nameof(entity));
            for (var i = 0; i < components.Length; i++)
            {
                var snapshot = components[i].TryGetSnapshot(entityManager, entity, metadata);
                if (snapshot != null)
                    return snapshot;
            }

            return null;
        }

        public static IEntityTrackingCollection? TryGetTrackingCollection(this IEntityTrackingCollectionProviderComponent[] components, IEntityManager entityManager, object? request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var collection = components[i].TryGetTrackingCollection(entityManager, request, metadata);
                if (collection != null)
                    return collection;
            }

            return null;
        }

        public static void OnSnapshotCreated(this IEntityManagerListener[] listeners, IEntityManager entityManager, IEntityStateSnapshot snapshot, object entity, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(entityManager, nameof(entityManager));
            Should.NotBeNull(snapshot, nameof(snapshot));
            Should.NotBeNull(entity, nameof(entity));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnSnapshotCreated(entityManager, snapshot, entity, metadata);
        }

        public static void OnTrackingCollectionCreated(this IEntityManagerListener[] listeners, IEntityManager entityManager, IEntityTrackingCollection collection, object? request, IReadOnlyMetadataContext? metadata)
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