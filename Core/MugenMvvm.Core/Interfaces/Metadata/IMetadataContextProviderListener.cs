using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextProviderListener : IListener
    {
        void OnReadOnlyContextCreated(IMetadataContextProvider provider, IReadOnlyMetadataContext metadataContext, object? target);

        void OnContextCreated(IMetadataContextProvider provider, IMetadataContext metadataContext, object? target);

        void OnObservableContextCreated(IMetadataContextProvider provider, IObservableMetadataContext metadataContext, object? target);
    }
}