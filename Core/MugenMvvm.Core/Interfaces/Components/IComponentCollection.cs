using JetBrains.Annotations;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollection : IComponentOwner<IComponentCollection>
    {
        object Owner { get; }

        int Count { get; }

        bool Add(object component, IReadOnlyMetadataContext? metadata = null);

        bool Remove(object component, IReadOnlyMetadataContext? metadata = null);

        bool Clear(IReadOnlyMetadataContext? metadata = null);

        [Pure]
        TComponent[] Get<TComponent>(IReadOnlyMetadataContext? metadata = null) where TComponent : class;
    }
}