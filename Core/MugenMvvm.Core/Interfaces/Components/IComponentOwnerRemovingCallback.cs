using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerRemovingCallback<T> where T : class
    {
        bool OnComponentRemoving(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext? metadata);
    }
}