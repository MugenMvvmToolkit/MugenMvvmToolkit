using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionProviderListener : IComponent<IComponentCollectionProvider>
    {
        void OnComponentCollectionCreated(IComponentCollectionProvider provider, IComponentCollection componentCollection, IReadOnlyMetadataContext? metadata);
    }
}