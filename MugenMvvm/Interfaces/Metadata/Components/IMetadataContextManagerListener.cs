using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Metadata.Components
{
    public interface IMetadataContextManagerListener : IComponent<IMetadataContextManager>
    {
        void OnReadOnlyContextCreated(IMetadataContextManager metadataContextManager, IReadOnlyMetadataContext metadataContext, object? target);

        void OnContextCreated(IMetadataContextManager metadataContextManager, IMetadataContext metadataContext, object? target);
    }
}