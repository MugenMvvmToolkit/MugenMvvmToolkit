using System;
using System.Collections.Generic;
using MugenMvvm.Entities;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Entities
{
    public interface IEntityStateSnapshot
    {
        Type EntityType { get; }

        bool HasChanges(object entity, object? member = null, IReadOnlyMetadataContext? metadata = null);

        void Restore(object entity, IReadOnlyMetadataContext? metadata = null);

        ItemOrList<EntityStateValue, IReadOnlyList<EntityStateValue>> Dump(object entity, IReadOnlyMetadataContext? metadata = null);
    }
}