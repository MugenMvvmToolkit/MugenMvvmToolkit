using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Interfaces.Metadata.Components
{
    public interface IMetadataContextProviderComponent : IComponent<IMetadataContextManager>
    {
        IReadOnlyMetadataContext? TryGetReadOnlyMetadataContext(IMetadataContextManager metadataContextManager, object? target, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values);

        IMetadataContext? TryGetMetadataContext(IMetadataContextManager metadataContextManager, object? target, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values);
    }
}