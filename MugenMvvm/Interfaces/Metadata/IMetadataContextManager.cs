using System.Collections.Generic;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextManager : IComponentOwner<IMetadataContextManager>, IComponent<IMugenApplication>
    {
        IReadOnlyMetadataContext? TryGetReadOnlyMetadataContext(object? target = null, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values = default);

        IMetadataContext? TryGetMetadataContext(object? target = null, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values = default);
    }
}