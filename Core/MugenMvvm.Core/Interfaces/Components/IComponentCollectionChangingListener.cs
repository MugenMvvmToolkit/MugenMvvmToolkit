using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionChangingListener : IComponent<IComponentCollection>
    {
        bool OnAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);

        bool OnRemoving(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}