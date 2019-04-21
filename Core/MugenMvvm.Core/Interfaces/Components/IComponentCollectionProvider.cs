using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionProvider : IHasListeners<IComponentCollectionProviderListener>
    {
        IComponentCollection<IComponentCollectionFactory> ComponentCollectionFactories { get; }

        IComponentCollection<T> GetComponentCollection<T>(object owner, IReadOnlyMetadataContext metadata) where T : class;
    }
}