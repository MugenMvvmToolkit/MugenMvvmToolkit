using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerAddingCallback<T> where T : class
    {
        bool OnComponentAdding(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata);
    }
}