using System;
using System.IO;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializer : IComponentOwner<ISerializer>, IComponent<IMugenApplication>
    {
        bool IsOnSerializingSupported { get; }

        bool IsOnSerializedSupported { get; }

        bool IsOnDeserializingSupported { get; }

        bool IsOnDeserializedSupported { get; }

        [Pure]
        bool CanSerialize(Type type, IReadOnlyMetadataContext? metadata = null);

        Stream Serialize(object item, IReadOnlyMetadataContext? metadata = null);

        object Deserialize(Stream stream, IReadOnlyMetadataContext? metadata = null);
    }
}