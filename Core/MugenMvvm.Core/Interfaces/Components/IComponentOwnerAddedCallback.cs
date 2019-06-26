using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerAddedCallback<in T> where T : class
    {
        void OnComponentAdded(object collection, T component, IReadOnlyMetadataContext? metadata);
    }
}