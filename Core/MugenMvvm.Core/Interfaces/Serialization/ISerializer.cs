using System;
using System.IO;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializer
    {
        IComponentCollection<ISerializerHandler> Handlers { get; }

        bool IsOnSerializingSupported { get; }

        bool IsOnSerializedSupported { get; }

        bool IsOnDeserializingSupported { get; }

        bool IsOnDeserializedSupported { get; }

        [Pure]
        ISerializationContext GetSerializationContext(IServiceProvider? serviceProvider, IMetadataContext? metadata);

        [Pure]
        bool CanSerialize(Type type, IReadOnlyMetadataContext? metadata);

        Stream Serialize(object item, ISerializationContext? serializationContext);

        object Deserialize(Stream stream, ISerializationContext? serializationContext);
    }
}