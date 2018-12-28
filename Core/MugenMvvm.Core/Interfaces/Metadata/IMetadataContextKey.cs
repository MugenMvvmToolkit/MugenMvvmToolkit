using System;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextKey : IEquatable<IMetadataContextKey>, IHasMemento
    {
        string Key { get; }

        void Validate(object? item);

        bool CanSerialize(object? item, ISerializationContext context);

        object? GetDefaultValue(IReadOnlyMetadataContext context, object? defaultValue);
    }

    public interface IMetadataContextKey<T> : IMetadataContextKey
    {
        T GetDefaultValue(IReadOnlyMetadataContext context, T defaultValue);
    }
}