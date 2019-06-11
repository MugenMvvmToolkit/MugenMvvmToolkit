using System.Collections.Generic;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IChildMetadataContextProvider : IHasPriority
    {
        IReadOnlyMetadataContext? TryGetReadOnlyMetadataContext(IMetadataContextProvider provider, object? target, IEnumerable<MetadataContextValue>? values);

        IMetadataContext? TryGetMetadataContext(IMetadataContextProvider provider, object? target, IEnumerable<MetadataContextValue>? values);

        IObservableMetadataContext? TryGetObservableMetadataContext(IMetadataContextProvider provider, object? target, IEnumerable<MetadataContextValue>? values);
    }
}