using System.Diagnostics.CodeAnalysis;
using System.IO;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISerializerComponent : IComponent<ISerializer>
    {
        bool CanSerialize<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata);

        Stream? TrySerialize<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata);

        bool TryDeserialize(Stream stream, IReadOnlyMetadataContext? metadata, out object? value);
    }
}