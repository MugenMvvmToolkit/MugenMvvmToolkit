using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IHasAddedCallbackComponentOwner : IComponentOwner
    {
        void OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}