using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionProviderListener : IComponent<IComponentCollectionProvider>
    {
        void OnComponentCollectionCreated<T>(IComponentCollectionProvider provider, IComponentCollection<T> componentCollection, IReadOnlyMetadataContext? metadata) where T : class;
    }
}