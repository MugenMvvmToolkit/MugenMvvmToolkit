using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Interfaces.Metadata.Components
{
    public interface IMetadataContextProviderComponent : IComponent<IMetadataContextManager>//todo remove
    {
        IReadOnlyMetadataContext? TryGetReadOnlyMetadataContext(IMetadataContextManager metadataContextManager, object? target, ItemOrList<KeyValuePair<IMetadataContextKey, object?>, IReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>>> values);

        IMetadataContext? TryGetMetadataContext(IMetadataContextManager metadataContextManager, object? target, ItemOrList<KeyValuePair<IMetadataContextKey, object?>, IReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>>> values);
    }
}