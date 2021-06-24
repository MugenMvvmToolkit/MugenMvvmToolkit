using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IHasComponentChangedHandler : IComponentOwner
    {
        void OnComponentChanged(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata);
    }
}