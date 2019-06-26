using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerClearedCallback<in T> where T : class
    {
        void OnComponentCleared(object collection, T[] oldItems, IReadOnlyMetadataContext metadata);
    }
}