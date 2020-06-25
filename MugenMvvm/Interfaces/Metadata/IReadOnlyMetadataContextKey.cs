namespace MugenMvvm.Interfaces.Metadata
{
    public interface IReadOnlyMetadataContextKey<TGet> : IMetadataContextKey
    {
        TGet GetValue(IReadOnlyMetadataContext metadataContext, object? value);

        TGet GetDefaultValue(IReadOnlyMetadataContext metadataContext, TGet defaultValue);
    }
}