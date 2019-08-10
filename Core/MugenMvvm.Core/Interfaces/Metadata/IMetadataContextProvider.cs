using System.Collections.Generic;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextProvider : IComponentOwner<IMetadataContextProvider>, IComponent<IMugenApplication>
    {
        IReadOnlyMetadataContext GetReadOnlyMetadataContext(object? target = null, IEnumerable<MetadataContextValue>? values = null);

        IMetadataContext GetMetadataContext(object? target = null, IEnumerable<MetadataContextValue>? values = null);
    }
}