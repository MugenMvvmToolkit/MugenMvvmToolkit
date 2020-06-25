using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionChangedListener : IComponent<IComponentCollection>
    {
        void OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);

        void OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}