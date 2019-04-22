using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextProviderListener : IListener
    {
        void OnReadOnlyContextCreated(IMetadataContextProvider provider, object? target, IReadOnlyMetadataContext metadataContext);

        void OnContextCreated(IMetadataContextProvider provider, object? target, IMetadataContext metadataContext);

        void OnObservableContextCreated(IMetadataContextProvider provider, object? target, IObservableMetadataContext metadataContext);
    }
}