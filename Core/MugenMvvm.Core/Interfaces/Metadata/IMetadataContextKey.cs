﻿using System;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextKey : IEquatable<IMetadataContextKey>, IHasMemento
    {
        string Key { get; }

        object? ToSerializableValue(object? item, ISerializationContext serializationContext);

        bool CanSerializeValue(object? item, ISerializationContext serializationContext);
    }
}