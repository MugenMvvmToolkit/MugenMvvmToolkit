using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IHasComponentAddedHandler : IComponentOwner
    {
        void OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}