using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionListener<T> : IComponent<IComponentCollection<T>> where T : class
    {
        bool OnAdding(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext metadata);

        void OnAdded(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext metadata);

        bool OnRemoving(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext metadata);

        void OnRemoved(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext metadata);

        bool OnClearing(IComponentCollection<T> collection, IReadOnlyMetadataContext metadata);

        void OnCleared(IComponentCollection<T> collection, T[] oldItems, IReadOnlyMetadataContext metadata);
    }
}