using System;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextKey : IEquatable<IMetadataContextKey>
    {
    }

    public interface IMetadataContextKey<TGet, in TSet> : IReadOnlyMetadataContextKey<TGet>
    {
        object? SetValue(IReadOnlyMetadataContext metadataContext, object? oldValue, TSet newValue);
    }
}