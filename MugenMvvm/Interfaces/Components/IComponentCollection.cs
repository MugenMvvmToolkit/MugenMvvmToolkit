using JetBrains.Annotations;
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

        [Pure]
        TComponent[] Get<TComponent>(IReadOnlyMetadataContext? metadata = null) where TComponent : class;//todo review
    }
}