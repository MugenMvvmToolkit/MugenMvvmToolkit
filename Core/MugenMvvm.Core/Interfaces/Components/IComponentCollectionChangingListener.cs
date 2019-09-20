using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionChangingListener<T> : IComponent<IComponentCollection<T>> where T : class
    {
        bool OnAdding(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata);

        bool OnRemoving(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata);

        bool OnClearing(IComponentCollection<T> collection, IReadOnlyMetadataContext? metadata);
    }
}