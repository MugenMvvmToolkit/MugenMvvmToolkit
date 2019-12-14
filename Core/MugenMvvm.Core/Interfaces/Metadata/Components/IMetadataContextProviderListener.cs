using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Metadata.Components
{
    public interface IMetadataContextProviderListener : IComponent<IMetadataContextProvider>
    {
        void OnReadOnlyContextCreated(IMetadataContextProvider metadataContextProvider, IReadOnlyMetadataContext metadataContext, object? target);

        void OnContextCreated(IMetadataContextProvider metadataContextProvider, IMetadataContext metadataContext, object? target);
    }
}