using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerRemovingCallback
    {
        bool OnComponentRemoving(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}