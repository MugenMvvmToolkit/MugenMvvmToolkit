using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwner<in T> where T : class
    {
        void OnComponentAdded(T component, IReadOnlyMetadataContext metadata);

        void OnComponentRemoved(T component, IReadOnlyMetadataContext metadata);
    }
}