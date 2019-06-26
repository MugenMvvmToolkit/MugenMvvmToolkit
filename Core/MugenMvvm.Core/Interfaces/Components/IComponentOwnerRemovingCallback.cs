using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerRemovingCallback<in T> where T : class
    {
        bool OnComponentRemoving(object collection, T component, IReadOnlyMetadataContext? metadata);
    }
}