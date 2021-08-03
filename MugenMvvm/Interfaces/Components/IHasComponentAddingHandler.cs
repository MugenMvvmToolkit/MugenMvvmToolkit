using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IHasComponentAddingHandler : IComponentOwner
    {
        void OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}