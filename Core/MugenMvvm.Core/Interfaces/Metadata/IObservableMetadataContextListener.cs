namespace MugenMvvm.Interfaces.Metadata
{
    public interface IObservableMetadataContextListener
    {
        void OnAdded(IObservableMetadataContext metadataContext, IMetadataContextKey key, object? newValue);

        void OnChanged(IObservableMetadataContext metadataContext, IMetadataContextKey key, object? oldValue, object? newValue);

        void OnRemoved(IObservableMetadataContext metadataContext, IMetadataContextKey key, object? oldValue);
    }
}