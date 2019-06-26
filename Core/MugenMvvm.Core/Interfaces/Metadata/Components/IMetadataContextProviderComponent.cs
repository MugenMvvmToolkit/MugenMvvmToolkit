using System.Collections.Generic;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextProviderComponent : IComponent<IMetadataContextProvider>
    {
        IReadOnlyMetadataContext? TryGetReadOnlyMetadataContext(object? target, IEnumerable<MetadataContextValue>? values);

        IMetadataContext? TryGetMetadataContext(object? target, IEnumerable<MetadataContextValue>? values);
    }
}