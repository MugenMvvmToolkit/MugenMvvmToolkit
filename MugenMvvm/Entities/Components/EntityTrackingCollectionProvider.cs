﻿using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

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

        public IEntityTrackingCollection TryGetTrackingCollection(IEntityManager entityManager, object? request, IReadOnlyMetadataContext? metadata)
        {
            var collection = new EntityTrackingCollection(request as IEqualityComparer<object>, _componentCollectionManager);
            collection.AddComponent(EntityStateTransitionManager.Instance);
            return collection;
        }

        #endregion
    }
}