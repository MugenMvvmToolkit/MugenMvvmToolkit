using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Metadata.Components
{
    public interface IMetadataContextValueManagerComponent : IComponent<IMetadataContext>
    {
        int GetCount(IMetadataContext context);

        IEnumerable<KeyValuePair<IMetadataContextKey, object?>> GetValues(IMetadataContext context);

        bool Contains(IMetadataContext context, IMetadataContextKey contextKey);

        bool TryGetValue(IMetadataContext context, IMetadataContextKey contextKey, MetadataOperationType operationType, out object? rawValue);

        bool TrySetValue(IMetadataContext context, IMetadataContextKey contextKey, object? rawValue);

        bool TryRemove(IMetadataContext context, IMetadataContextKey contextKey);

        void Clear(IMetadataContext context);
    }
}