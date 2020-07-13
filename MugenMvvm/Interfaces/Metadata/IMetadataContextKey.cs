using System;
using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextKey : IEquatable<IMetadataContextKey?>
    {
        IReadOnlyDictionary<string, object?> Metadata { get; }
    }

    public interface IMetadataContextKey<TGet, in TSet> : IReadOnlyMetadataContextKey<TGet>
    {
        object? SetValue(IReadOnlyMetadataContext metadataContext, object? oldValue, TSet newValue);
    }
}