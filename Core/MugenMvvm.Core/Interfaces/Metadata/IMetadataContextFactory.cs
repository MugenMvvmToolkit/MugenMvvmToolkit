using System.Collections.Generic;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextFactory : IHasPriority
    {
        IReadOnlyMetadataContext? TryGetReadOnlyMetadataContext(object? target, IEnumerable<MetadataContextValue>? values);

        IMetadataContext? TryGetMetadataContext(object? target, IEnumerable<MetadataContextValue>? values);

        IObservableMetadataContext? TryGetObservableMetadataContext(object? target, IEnumerable<MetadataContextValue>? values);
    }
}