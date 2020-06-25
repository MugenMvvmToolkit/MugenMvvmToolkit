using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Entities.Internal
{
    public class TestEntityStateChangedListener : IEntityStateChangedListener, IHasPriority
    {
        #region Properties

        public Action<IEntityTrackingCollection, object, EntityState, EntityState, IReadOnlyMetadataContext?>? OnEntityStateChanged { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IEntityStateChangedListener.OnEntityStateChanged(IEntityTrackingCollection collection, object entity, EntityState from, EntityState to, IReadOnlyMetadataContext? metadata)
        {
            OnEntityStateChanged?.Invoke(collection, entity, from, to, metadata);
        }

        #endregion
    }
}