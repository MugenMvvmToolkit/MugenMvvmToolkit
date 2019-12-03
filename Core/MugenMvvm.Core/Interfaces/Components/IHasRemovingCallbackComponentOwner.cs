using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IHasRemovingCallbackComponentOwner : IComponentOwner
    {
        bool OnComponentRemoving(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}