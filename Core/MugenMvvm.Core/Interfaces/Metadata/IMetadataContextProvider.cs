using System.Collections.Generic;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextProvider : IComponentOwner<IMetadataContextProvider> //todo review opts, api nullable = null parameters
    {
        IReadOnlyMetadataContext GetReadOnlyMetadataContext(object? target, IEnumerable<MetadataContextValue>? values);

        IMetadataContext GetMetadataContext(object? target, IEnumerable<MetadataContextValue>? values);
    }
}