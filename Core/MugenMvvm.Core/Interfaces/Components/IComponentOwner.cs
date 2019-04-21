using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerAddingCallback<in T> where T : class
    {
        bool OnComponentAdding(object collection, T component, IReadOnlyMetadataContext metadata);
    }

    public interface IComponentOwnerAddedCallback<in T> where T : class
    {
        void OnComponentAdded(object collection, T component, IReadOnlyMetadataContext metadata);
    }

    public interface IComponentOwnerRemovingCallback<in T> where T : class
    {
        bool OnComponentRemoving(object collection, T component, IReadOnlyMetadataContext metadata);
    }

    public interface IComponentOwnerRemovedCallback<in T> where T : class
    {
        void OnComponentRemoved(object collection, T component, IReadOnlyMetadataContext metadata);
    }

    public interface IComponentOwnerClearingCallback<in T> where T : class
    {
        bool OnComponentClearing(object collection, T[] items, IReadOnlyMetadataContext metadata);
    }

    public interface IComponentOwnerClearedCallback<in T> where T : class
    {
        void OnComponentCleared(object collection, T[] oldItems, IReadOnlyMetadataContext metadata);
    }
}