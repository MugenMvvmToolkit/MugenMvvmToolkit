using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionChangedListener<T> : IComponent<IComponentCollection<T>> where T : class
    {
        void OnAdded(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata);

        void OnRemoved(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata);

        void OnCleared(IComponentCollection<T> collection, ItemOrList<T?, T[]> oldItems, IReadOnlyMetadataContext? metadata);
    }
}