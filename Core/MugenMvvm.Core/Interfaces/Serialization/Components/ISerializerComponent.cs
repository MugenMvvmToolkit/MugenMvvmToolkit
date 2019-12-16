using System;
using System.IO;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISerializerComponent : IComponent<ISerializer>
    {
        bool CanSerialize(Type targetType, IReadOnlyMetadataContext? metadata);

        Stream? TrySerialize(object target, IReadOnlyMetadataContext? metadata);

        object? TryDeserialize(Stream stream, IReadOnlyMetadataContext? metadata);
    }
}