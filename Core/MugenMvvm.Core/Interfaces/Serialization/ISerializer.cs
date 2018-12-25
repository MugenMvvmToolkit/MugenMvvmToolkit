using System;
using System.IO;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializer
    {
        ISerializationContext GetContext(IServiceProvider? serviceProvider, IMetadataContext? metadata);

        [Pure]
        bool CanSerialize(Type type);

        Stream Serialize(object item, ISerializationContext? context);

        object Deserialize(Stream stream, ISerializationContext? context);
    }
}