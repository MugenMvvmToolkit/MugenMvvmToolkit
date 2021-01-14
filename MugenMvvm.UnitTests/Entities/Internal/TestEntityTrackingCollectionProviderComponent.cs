using System;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Entities.Internal
{
    public class TestEntityTrackingCollectionProviderComponent : IEntityTrackingCollectionProviderComponent, IHasPriority
    {
        private readonly IEntityManager? _entityManager;

        public TestEntityTrackingCollectionProviderComponent(IEntityManager? entityManager)
        {
            _entityManager = entityManager;
        }

        public Func<object?, IReadOnlyMetadataContext?, IEntityTrackingCollection?>? TryGetTrackingCollection { get; set; }

        public int Priority { get; set; }

        IEntityTrackingCollection? IEntityTrackingCollectionProviderComponent.TryGetTrackingCollection(IEntityManager entityManager, object? request,
            IReadOnlyMetadataContext? metadata)
        {
            _entityManager?.ShouldEqual(entityManager);
            return TryGetTrackingCollection?.Invoke(request, metadata);
        }
    }
}