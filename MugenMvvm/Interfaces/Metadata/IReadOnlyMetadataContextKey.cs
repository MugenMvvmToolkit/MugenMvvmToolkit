using System.Diagnostics.CodeAnalysis;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IReadOnlyMetadataContextKey<T> : IMetadataContextKey
    {
        T GetValue(IReadOnlyMetadataContext metadataContext, object? rawValue);

        T GetValue(IReadOnlyMetadataContext metadataContext, object? rawValue, T value);

        [return: NotNullIfNotNull("defaultValue")]
        T GetDefaultValue(IReadOnlyMetadataContext metadataContext, T? defaultValue);
    }
}