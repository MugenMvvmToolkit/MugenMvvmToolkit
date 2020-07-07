using System.Diagnostics.CodeAnalysis;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IReadOnlyMetadataContextKey<TGet> : IMetadataContextKey
    {
        TGet GetValue(IReadOnlyMetadataContext metadataContext, object? value);

        [return: NotNullIfNotNull("defaultValue")]
        TGet GetDefaultValue(IReadOnlyMetadataContext metadataContext, [AllowNull] TGet defaultValue);
    }
}