using System;
using System.IO;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializer : IComponentOwner<ISerializer>
    {
        bool IsOnSerializingSupported { get; }

        bool IsOnSerializedSupported { get; }

        bool IsOnDeserializingSupported { get; }

        bool IsOnDeserializedSupported { get; }

        [Pure]
        ISerializationContext GetSerializationContext(IServiceProvider? serviceProvider = null, IReadOnlyMetadataContext? metadata = null);

        [Pure]
        bool CanSerialize(Type type, IReadOnlyMetadataContext? metadata = null);

        Stream Serialize(object item, ISerializationContext? serializationContext = null);

        object Deserialize(Stream stream, ISerializationContext? serializationContext = null);
    }
}