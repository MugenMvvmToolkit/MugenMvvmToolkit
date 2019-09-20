using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerAddedCallback<T> where T : class
    {
        void OnComponentAdded(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata);
    }
}