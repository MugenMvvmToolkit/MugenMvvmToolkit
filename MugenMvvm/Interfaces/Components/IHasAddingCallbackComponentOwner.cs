using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IHasAddingCallbackComponentOwner : IComponentOwner
    {
        bool OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}