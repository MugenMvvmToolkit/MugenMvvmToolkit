using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IHasComponentRemoveConditionHandler : IComponentOwner
    {
        bool CanRemoveComponent(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}