using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Interfaces.Metadata.Components
{
    public interface IMetadataContextProviderComponent : IComponent<IMetadataContextProvider>
    {
        IReadOnlyMetadataContext? TryGetReadOnlyMetadataContext(object? target, IEnumerable<MetadataContextValue>? values);

        IMetadataContext? TryGetMetadataContext(object? target, IEnumerable<MetadataContextValue>? values);
    }
}