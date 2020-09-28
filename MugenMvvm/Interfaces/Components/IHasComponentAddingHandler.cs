using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IHasComponentAddingHandler : IComponentOwner
    {
        bool OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}