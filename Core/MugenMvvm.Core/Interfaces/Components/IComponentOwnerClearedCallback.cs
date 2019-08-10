using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerClearedCallback<T> where T : class
    {
        void OnComponentCleared(IComponentCollection<T> collection, T[] oldItems, IReadOnlyMetadataContext? metadata);
    }
}