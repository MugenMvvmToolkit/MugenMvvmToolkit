using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTests.Entities.Internal
{
    public class TestEntityStateChangingListener : IEntityStateChangingListener, IHasPriority
    {
        public Func<IEntityTrackingCollection, object, EntityState, EntityState, IReadOnlyMetadataContext?, EntityState>? OnEntityStateChanging { get; set; }

        public int Priority { get; set; }

        EntityState IEntityStateChangingListener.OnEntityStateChanging(IEntityTrackingCollection collection, object entity, EntityState from, EntityState to,
            IReadOnlyMetadataContext? metadata) =>
            OnEntityStateChanging?.Invoke(collection, entity, from, to, metadata) ?? to;
    }
}