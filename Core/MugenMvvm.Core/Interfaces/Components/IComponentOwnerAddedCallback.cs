using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerAddedCallback//todo rename
    {
        void OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}