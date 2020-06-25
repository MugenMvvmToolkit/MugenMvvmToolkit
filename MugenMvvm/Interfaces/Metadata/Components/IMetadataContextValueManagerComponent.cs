using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Metadata.Components
{
    public interface IMetadataContextValueManagerComponent : IComponent<IMetadataContext>
    {
        int GetCount();

        IEnumerable<KeyValuePair<IMetadataContextKey, object?>> GetValues();

        bool Contains(IMetadataContextKey contextKey);

        bool TryGetValue(IMetadataContextKey contextKey, out object? rawValue);

        bool TrySetValue(IMetadataContextKey contextKey, object? rawValue);

        bool TryClear(IMetadataContextKey contextKey);

        void Clear();
    }
}