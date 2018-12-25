namespace MugenMvvm.Interfaces.Metadata
{
    public interface IObservableMetadataContextListener
    {
        void OnContextChanged(IObservableMetadataContext metadataContext, IMetadataContextKey? key);
    }
}