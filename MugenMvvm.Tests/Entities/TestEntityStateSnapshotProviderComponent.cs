using System;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Entities
{
    public class TestEntityStateSnapshotProviderComponent : IEntityStateSnapshotProviderComponent, IHasPriority
    {
        public Func<IEntityManager, object, IReadOnlyMetadataContext?, IEntityStateSnapshot?>? TryGetSnapshot { get; set; }

        public int Priority { get; set; }

        IEntityStateSnapshot? IEntityStateSnapshotProviderComponent.TryGetSnapshot(IEntityManager entityManager, object entity, IReadOnlyMetadataContext? metadata) =>
            TryGetSnapshot?.Invoke(entityManager, entity, metadata);
    }
}