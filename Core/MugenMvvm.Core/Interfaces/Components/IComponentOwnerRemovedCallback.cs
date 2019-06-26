using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerRemovedCallback<in T> where T : class
    {
        void OnComponentRemoved(object collection, T component, IReadOnlyMetadataContext metadata);
    }
}