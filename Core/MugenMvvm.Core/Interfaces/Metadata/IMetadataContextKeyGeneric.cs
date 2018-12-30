namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextKey<T> : IMetadataContextKey
    {
        void Validate(T item);

         T GetValue(IReadOnlyMetadataContext context, object? value);

        object? SetValue(IReadOnlyMetadataContext context, T value);

        T GetDefaultValue(IReadOnlyMetadataContext context, T defaultValue);
    }
}