using System.Collections.Generic;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextProvider : IComponentOwner<IMetadataContextProvider>
    {
        IReadOnlyMetadataContext GetReadOnlyMetadataContext(object? target = null, IEnumerable<MetadataContextValue>? values = null);

        IMetadataContext GetMetadataContext(object? target = null, IEnumerable<MetadataContextValue>? values = null);
    }
}