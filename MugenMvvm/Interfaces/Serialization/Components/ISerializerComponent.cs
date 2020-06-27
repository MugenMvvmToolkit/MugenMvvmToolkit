using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISerializerComponent : IComponent<ISerializer>
    {
        bool CanSerialize<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata);

        string? TrySerialize<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata);

        bool TryDeserialize(string data, IReadOnlyMetadataContext? metadata, out object? value);
    }
}