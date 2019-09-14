using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerClearedCallback<T> where T : class
    {
        void OnComponentCleared(IComponentCollection<T> collection, ItemOrList<T?, T[]> oldItems, IReadOnlyMetadataContext? metadata);
    }
}