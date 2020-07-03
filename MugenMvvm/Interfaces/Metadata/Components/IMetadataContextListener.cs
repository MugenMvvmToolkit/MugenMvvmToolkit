using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Metadata.Components
{
    public interface IMetadataContextListener : IComponent<IMetadataContext>
    {
        void OnAdded(IMetadataContext context, IMetadataContextKey key, object? newValue);

        void OnChanged(IMetadataContext context, IMetadataContextKey key, object? oldValue, object? newValue);

        void OnRemoved(IMetadataContext context, IMetadataContextKey key, object? oldValue);
    }
}