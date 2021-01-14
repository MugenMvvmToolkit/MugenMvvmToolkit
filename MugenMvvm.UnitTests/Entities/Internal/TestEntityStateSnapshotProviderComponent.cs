using System;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Entities.Internal
{
    public class TestEntityStateSnapshotProviderComponent : IEntityStateSnapshotProviderComponent, IHasPriority
    {
        private readonly IEntityManager? _entityManager;

        public TestEntityStateSnapshotProviderComponent(IEntityManager? entityManager)
        {
            _entityManager = entityManager;
        }

        public Func<object, IReadOnlyMetadataContext?, IEntityStateSnapshot?>? TryGetSnapshot { get; set; }

        public int Priority { get; set; }

        IEntityStateSnapshot? IEntityStateSnapshotProviderComponent.TryGetSnapshot(IEntityManager entityManager, object entity, IReadOnlyMetadataContext? metadata)
        {
            _entityManager?.ShouldEqual(entityManager);
            return TryGetSnapshot?.Invoke(entity, metadata);
        }
    }
}