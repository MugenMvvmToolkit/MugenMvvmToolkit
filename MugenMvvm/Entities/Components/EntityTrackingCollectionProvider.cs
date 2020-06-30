using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Entities.Components
{
    public sealed class EntityTrackingCollectionProvider : IEntityTrackingCollectionProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IComponentCollectionManager? _componentCollectionManager;

        #endregion

        #region Constructors

        public EntityTrackingCollectionProvider(IComponentCollectionManager? componentCollectionManager = null)
        {
            _componentCollectionManager = componentCollectionManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = EntityComponentPriority.TrackingCollectionProvider;

        #endregion

        #region Implementation of interfaces

        public IEntityTrackingCollection? TryGetTrackingCollection<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            IEqualityComparer<object>? comparer;
            if (TypeChecker.IsValueType<TRequest>())
                comparer = null;
            else
                comparer = request as IEqualityComparer<object>;
            var collection = new EntityTrackingCollection(comparer, _componentCollectionManager);
            collection.AddComponent(EntityStateTransitionManager.Instance);
            return collection;
        }

        #endregion
    }
}