using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IConditionComponentCollectionComponent : IComponent<IComponentCollection>
    {
        bool CanAdd(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);

        bool CanRemove(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}