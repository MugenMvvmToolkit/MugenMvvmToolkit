using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerRemovedCallback<T> where T : class
    {
        void OnComponentRemoved(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata);
    }
}