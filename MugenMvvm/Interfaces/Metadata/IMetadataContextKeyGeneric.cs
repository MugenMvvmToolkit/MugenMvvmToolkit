namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextKey<T> : IReadOnlyMetadataContextKey<T>
    {
        object? SetValue(IReadOnlyMetadataContext metadataContext, object? oldValue, T newValue);
    }
}