using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Metadata.Components
{
    public interface IMetadataContextListener : IComponent<IMetadataContext>
    {
        void OnAdded(IMetadataContext metadataContext, IMetadataContextKey key, object? newValue);

        void OnChanged(IMetadataContext metadataContext, IMetadataContextKey key, object? oldValue, object? newValue);

        void OnRemoved(IMetadataContext metadataContext, IMetadataContextKey key, object? oldValue);
    }
}