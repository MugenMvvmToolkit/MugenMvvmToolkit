using System.Diagnostics.CodeAnalysis;
using System.IO;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISerializerComponent : IComponent<ISerializer>
    {
        bool CanSerialize<TTarget>([DisallowNull] in TTarget target, IReadOnlyMetadataContext? metadata);

        Stream? TrySerialize<TTarget>([DisallowNull] in TTarget target, IReadOnlyMetadataContext? metadata);

        object? TryDeserialize(Stream stream, IReadOnlyMetadataContext? metadata);
    }
}