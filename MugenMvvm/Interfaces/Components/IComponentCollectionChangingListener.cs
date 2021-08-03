using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionChangingListener : IComponent<IComponentCollection>
    {
        void OnAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);

        void OnRemoving(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}