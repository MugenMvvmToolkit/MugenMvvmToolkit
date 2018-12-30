using System;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextKey : IEquatable<IMetadataContextKey>, IHasMemento
    {
        string Key { get; }

        bool CanSerialize(object? item, ISerializationContext context);
    }
}