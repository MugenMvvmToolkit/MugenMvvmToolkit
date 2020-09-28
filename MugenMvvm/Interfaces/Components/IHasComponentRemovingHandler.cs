using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IHasComponentRemovingHandler : IComponentOwner
    {
        bool OnComponentRemoving(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}