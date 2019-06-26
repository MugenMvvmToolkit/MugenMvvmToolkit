using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerAddingCallback<in T> where T : class
    {
        bool OnComponentAdding(object collection, T component, IReadOnlyMetadataContext metadata);
    }
}