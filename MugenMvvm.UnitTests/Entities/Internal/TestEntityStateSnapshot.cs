using System;
using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Entities;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTests.Entities.Internal
{
    public class TestEntityStateSnapshot : IEntityStateSnapshot
    {
        #region Properties

        public Type EntityType { get; set; } = typeof(object);

        public Func<object, object?, IReadOnlyMetadataContext?, bool>? HasChanges { get; set; }

        public Action<object, IReadOnlyMetadataContext?>? Restore { get; set; }

        public Func<object, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<EntityStateValue>>? Dump { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IEntityStateSnapshot.HasChanges(object entity, object? member, IReadOnlyMetadataContext? metadata) => HasChanges?.Invoke(entity, member, metadata) ?? false;

        void IEntityStateSnapshot.Restore(object entity, IReadOnlyMetadataContext? metadata) => Restore?.Invoke(entity, metadata);

        ItemOrIReadOnlyList<EntityStateValue> IEntityStateSnapshot.Dump(object entity, IReadOnlyMetadataContext? metadata) => Dump?.Invoke(entity, metadata) ?? Default.Array<EntityStateValue>();

        #endregion
    }
}