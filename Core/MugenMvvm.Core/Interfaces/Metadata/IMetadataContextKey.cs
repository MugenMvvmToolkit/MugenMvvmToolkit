using System;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextKey : IEquatable<IMetadataContextKey>//todo add metadata dictionary
    {
    }

    public interface IMetadataContextKey<TGet, in TSet> : IReadOnlyMetadataContextKey<TGet>
    {
        object? SetValue(IReadOnlyMetadataContext metadataContext, object? oldValue, TSet newValue);
    }
}