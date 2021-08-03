using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IHasComponentAddConditionHandler : IComponentOwner
    {
        bool CanAddComponent(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}