using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerRemovedCallback
    {
        void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}