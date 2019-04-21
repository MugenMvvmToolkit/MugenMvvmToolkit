using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionListener : IListener
    {
        bool OnAdding<T>(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext metadata) where T : class;

        void OnAdded<T>(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext metadata) where T : class;

        bool OnRemoving<T>(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext metadata) where T : class;

        void OnRemoved<T>(IComponentCollection<T> collection, T component, IReadOnlyMetadataContext metadata) where T : class;

        bool OnClearing<T>(IComponentCollection<T> collection, IReadOnlyMetadataContext metadata) where T : class;

        void OnCleared<T>(IComponentCollection<T> collection, T[] oldItems, IReadOnlyMetadataContext metadata) where T : class;
    }
}