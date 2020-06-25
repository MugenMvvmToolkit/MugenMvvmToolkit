using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Interfaces.Metadata.Components
{
    public interface IMetadataContextProviderComponent : IComponent<IMetadataContextProvider>
    {
        IReadOnlyMetadataContext? TryGetReadOnlyMetadataContext(object? target, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values);

        IMetadataContext? TryGetMetadataContext(object? target, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values);
    }
}