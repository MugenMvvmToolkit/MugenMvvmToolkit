using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerClearingCallback<T> where T : class
    {
        bool OnComponentClearing(IComponentCollection<T> collection, T[] items, IReadOnlyMetadataContext? metadata);
    }
}