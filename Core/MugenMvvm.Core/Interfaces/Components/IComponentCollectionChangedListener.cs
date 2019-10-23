using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionChangedListener<T> : IComponent<IComponentCollection<T>> where T : class
    {
        void OnAdded(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata);

        void OnRemoved(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata);
    }
}