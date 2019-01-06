namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextKey<T> : IMetadataContextKey
    {
        T GetValue(IReadOnlyMetadataContext metadataContext, object? value);

        object? SetValue(IReadOnlyMetadataContext metadataContext, object? oldValue, T newValue);

        T GetDefaultValue(IReadOnlyMetadataContext metadataContext, T defaultValue);
    }
}