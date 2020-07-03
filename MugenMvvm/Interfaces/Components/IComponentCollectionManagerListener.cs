using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionManagerListener : IComponent<IComponentCollectionManager>
    {
        void OnComponentCollectionCreated(IComponentCollectionManager collectionManager, IComponentCollection collection, IReadOnlyMetadataContext? metadata);
    }
}