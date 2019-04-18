using System;
using System.IO;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializer //todo review, can serialize instance??
    {
        IComponentCollection<ISerializerHandler> Handlers { get; }

        [Pure]
        ISerializationContext GetSerializationContext(IServiceProvider? serviceProvider, IMetadataContext? metadata);

        [Pure]
        bool CanSerialize(Type type, IReadOnlyMetadataContext metadata);

        Stream Serialize(object item, ISerializationContext? serializationContext);

        object Deserialize(Stream stream, ISerializationContext? serializationContext);
    }
}