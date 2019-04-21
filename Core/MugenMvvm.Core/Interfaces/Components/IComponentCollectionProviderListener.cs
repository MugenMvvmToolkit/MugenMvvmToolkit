using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionProviderListener : IListener
    {
        void OnComponentCollectionCreated<T>(IComponentCollectionProvider provider, IComponentCollection<T> componentCollection, IReadOnlyMetadataContext metadata) where T : class;
    }
}