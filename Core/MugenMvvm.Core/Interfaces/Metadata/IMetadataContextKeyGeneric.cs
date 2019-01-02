namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextKey<T> : IMetadataContextKey
    {
        void Validate(T item);

         T GetValue(IReadOnlyMetadataContext metadataContext, object? value);

        object? SetValue(IReadOnlyMetadataContext metadataContext, T value);

        T GetDefaultValue(IReadOnlyMetadataContext metadataContext, T defaultValue);
    }
}