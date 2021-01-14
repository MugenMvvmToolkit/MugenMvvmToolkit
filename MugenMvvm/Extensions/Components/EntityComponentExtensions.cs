using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class EntityComponentExtensions
    {
        public static IEntityStateSnapshot? TryGetSnapshot(this ItemOrArray<IEntityStateSnapshotProviderComponent> components, IEntityManager entityManager, object entity,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(entity, nameof(entity));
            foreach (var c in components)
            {
                var snapshot = c.TryGetSnapshot(entityManager, entity, metadata);
                if (snapshot != null)
                    return snapshot;
            }

            return null;
        }

        public static IEntityTrackingCollection? TryGetTrackingCollection(this ItemOrArray<IEntityTrackingCollectionProviderComponent> components, IEntityManager entityManager,
            object? request, IReadOnlyMetadataContext? metadata)
        {
            foreach (var c in components)
            {
                var collection = c.TryGetTrackingCollection(entityManager, request, metadata);
                if (collection != null)
                    return collection;
            }

            return null;
        }

        public static void OnSnapshotCreated(this ItemOrArray<IEntityManagerListener> listeners, IEntityManager entityManager, IEntityStateSnapshot snapshot, object entity,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(entityManager, nameof(entityManager));
            Should.NotBeNull(snapshot, nameof(snapshot));
            Should.NotBeNull(entity, nameof(entity));
            foreach (var c in listeners)
                c.OnSnapshotCreated(entityManager, snapshot, entity, metadata);
        }

        public static void OnTrackingCollectionCreated(this ItemOrArray<IEntityManagerListener> listeners, IEntityManager entityManager, IEntityTrackingCollection collection,
            object? request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(entityManager, nameof(entityManager));
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnTrackingCollectionCreated(entityManager, collection, request, metadata);
        }

        public static EntityState OnEntityStateChanging(this ItemOrArray<IEntityStateChangingListener> listeners, IEntityTrackingCollection collection, object entity,
            EntityState from, EntityState to,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(entity, nameof(entity));
            Should.NotBeNull(to, nameof(from));
            foreach (var c in listeners)
                to = c.OnEntityStateChanging(collection, entity, from, to, metadata);
            return to;
        }

        public static void OnEntityStateChanged(this ItemOrArray<IEntityStateChangedListener> listeners, IEntityTrackingCollection collection, object entity, EntityState from,
            EntityState to,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(entity, nameof(entity));
            Should.NotBeNull(to, nameof(from));
            foreach (var c in listeners)
                c.OnEntityStateChanged(collection, entity, from, to, metadata);
        }
    }
}