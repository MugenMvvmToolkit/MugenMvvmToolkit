using System;
using MugenMvvm.Collections;
using MugenMvvm.Entities;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTests.Entities.Internal
{
    public class TestEntityStateSnapshot : IEntityStateSnapshot
    {
        public Func<object, object?, IReadOnlyMetadataContext?, bool>? HasChanges { get; set; }

        public Action<object, IReadOnlyMetadataContext?>? Restore { get; set; }

        public Func<object, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<EntityStateValue>>? Dump { get; set; }

        public Type EntityType { get; set; } = typeof(object);

        bool IEntityStateSnapshot.HasChanges(object entity, object? member, IReadOnlyMetadataContext? metadata) => HasChanges?.Invoke(entity, member, metadata) ?? false;

        void IEntityStateSnapshot.Restore(object entity, IReadOnlyMetadataContext? metadata) => Restore?.Invoke(entity, metadata);

        ItemOrIReadOnlyList<EntityStateValue> IEntityStateSnapshot.Dump(object entity, IReadOnlyMetadataContext? metadata) =>
            Dump?.Invoke(entity, metadata) ?? Array.Empty<EntityStateValue>();
    }
}