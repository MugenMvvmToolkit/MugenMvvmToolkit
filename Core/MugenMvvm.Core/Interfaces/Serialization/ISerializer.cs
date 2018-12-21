using System;
using System.IO;
using JetBrains.Annotations;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializer
    {
        Stream Serialize(object item, ISerializationContext? context);

        object Deserialize(Stream stream, ISerializationContext? context);

        [Pure]
        bool IsSerializable(Type type);
    }
}