using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Metadata.Components
{
    public interface IMetadataContextValueManagerComponent : IComponent<IMetadataContext>
    {
        int GetCount(IMetadataContext context);

        IEnumerable<KeyValuePair<IMetadataContextKey, object?>> GetValues(IMetadataContext context);

        bool Contains(IMetadataContext context, IMetadataContextKey contextKey);

        bool TryGetValue(IMetadataContext context, IMetadataContextKey contextKey, out object? rawValue);

        bool TrySetValue(IMetadataContext context, IMetadataContextKey contextKey, object? rawValue);

        bool TryClear(IMetadataContext context, IMetadataContextKey contextKey);

        void Clear(IMetadataContext context);
    }
}