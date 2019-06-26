using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwnerClearingCallback<in T> where T : class
    {
        bool OnComponentClearing(object collection, T[] items, IReadOnlyMetadataContext? metadata);
    }
}