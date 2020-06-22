using System;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Entities.Internal
{
    public class TestEntityTrackingCollectionProviderComponent : IEntityTrackingCollectionProviderComponent, IHasPriority
    {
        #region Properties

        public Func<object, Type, IReadOnlyMetadataContext?, IEntityTrackingCollection?>? TryGetTrackingCollection { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IEntityTrackingCollection? IEntityTrackingCollectionProviderComponent.TryGetTrackingCollection<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryGetTrackingCollection?.Invoke(request!, typeof(TRequest), metadata);
        }

        #endregion
    }
}