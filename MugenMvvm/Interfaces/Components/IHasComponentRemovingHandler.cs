using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IHasComponentRemovingHandler : IComponentOwner
    {
        void OnComponentRemoving(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}