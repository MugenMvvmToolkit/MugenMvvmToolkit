using JetBrains.Annotations;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollection : IComponentOwner<IComponentCollection> //todo remove binding components!
    {
        object Owner { get; }

        int Count { get; }

        bool Add(object component, IReadOnlyMetadataContext? metadata = null);

        bool Remove(object component, IReadOnlyMetadataContext? metadata = null);

        bool Clear(IReadOnlyMetadataContext? metadata = null);//todo review metadata

        [Pure]
        TComponent[] GetComponents<TComponent>(IReadOnlyMetadataContext? metadata) where TComponent : class; //todo update metadata
    }
}