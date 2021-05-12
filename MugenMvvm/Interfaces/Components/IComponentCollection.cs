using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollection : IComponentOwner<IComponentCollection>
    {
        object Owner { get; }

        int Count { get; }

        bool TryAdd(object component, IReadOnlyMetadataContext? metadata = null);

        bool Remove(object component, IReadOnlyMetadataContext? metadata = null);

        void Clear(IReadOnlyMetadataContext? metadata = null);

        void Invalidate(object component, IReadOnlyMetadataContext? metadata = null);

        ItemOrArray<T> Get<T>(IReadOnlyMetadataContext? metadata = null) where T : class;
    }
}