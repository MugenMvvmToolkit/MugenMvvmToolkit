using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IHasComponentRemovedHandler : IComponentOwner
    {
        void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}