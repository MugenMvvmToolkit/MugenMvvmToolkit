using System.Collections.Generic;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextProvider : IHasListeners<IMetadataContextProviderListener>
    {
        IComponentCollection<IMetadataContextFactory> MetadataContextFactories { get; }

        IReadOnlyMetadataContext GetReadOnlyMetadataContext(object? target, IEnumerable<MetadataContextValue>? values);

        IMetadataContext GetMetadataContext(object? target, IEnumerable<MetadataContextValue>? values);

        IObservableMetadataContext GetObservableMetadataContext(object? target, IEnumerable<MetadataContextValue>? values);
    }
}