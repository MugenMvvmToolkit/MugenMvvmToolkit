using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollection<T> : IHasListeners<IComponentCollectionListener> where T : class
    {
        object Owner { get; }

        bool HasItems { get; }

        bool Add(T component, IReadOnlyMetadataContext metadata);

        bool Remove(T component, IReadOnlyMetadataContext metadata);

        bool Clear(IReadOnlyMetadataContext metadata);

        T[] GetItems();
    }
}