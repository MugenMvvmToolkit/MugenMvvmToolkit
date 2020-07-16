using System;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Entities.Internal
{
    public class TestEntityTrackingCollectionProviderComponent : IEntityTrackingCollectionProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IEntityManager? _entityManager;

        #endregion

        #region Constructors

        public TestEntityTrackingCollectionProviderComponent(IEntityManager? entityManager)
        {
            _entityManager = entityManager;
        }

        #endregion

        #region Properties

        public Func<object?, IReadOnlyMetadataContext?, IEntityTrackingCollection?>? TryGetTrackingCollection { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IEntityTrackingCollection? IEntityTrackingCollectionProviderComponent.TryGetTrackingCollection(IEntityManager entityManager, object? request, IReadOnlyMetadataContext? metadata)
        {
            _entityManager?.ShouldEqual(entityManager);
            return TryGetTrackingCollection?.Invoke(request, metadata);
        }

        #endregion
    }
}