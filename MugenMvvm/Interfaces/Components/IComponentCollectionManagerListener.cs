using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionManagerListener : IComponent<IComponentCollectionManager>
    {
        void OnComponentCollectionCreated(IComponentCollectionManager provider, IComponentCollection componentCollection, IReadOnlyMetadataContext? metadata);
    }
}