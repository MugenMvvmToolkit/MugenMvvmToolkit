using System;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Entities
{
    public class TestEntityTrackingCollectionProviderComponent : IEntityTrackingCollectionProviderComponent, IHasPriority
    {
        public Func<IEntityManager, object?, IReadOnlyMetadataContext?, IEntityTrackingCollection?>? TryGetTrackingCollection { get; set; }

        public int Priority { get; set; }

        IEntityTrackingCollection? IEntityTrackingCollectionProviderComponent.TryGetTrackingCollection(IEntityManager entityManager, object? request,
            IReadOnlyMetadataContext? metadata) =>
            TryGetTrackingCollection?.Invoke(entityManager, request, metadata);
    }
}