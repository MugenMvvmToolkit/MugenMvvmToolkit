using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerAddingCallback
    {
        bool OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}