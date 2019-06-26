using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollection<T> : IComponentOwner<IComponentCollection<T>> where T : class
    {
        object Owner { get; }

        bool HasItems { get; }

        bool Add(T component, IReadOnlyMetadataContext? metadata = null);

        bool Remove(T component, IReadOnlyMetadataContext? metadata = null);

        bool Clear(IReadOnlyMetadataContext? metadata = null);

        T[] GetItems();
    }
}