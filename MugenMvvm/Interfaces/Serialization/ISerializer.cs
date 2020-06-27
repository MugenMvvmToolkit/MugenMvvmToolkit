using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializer : IComponentOwner<ISerializer>, IComponent<IMugenApplication>
    {
        [Pure]
        bool CanSerialize<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null);

        string? TrySerialize<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null);

        bool TryDeserialize(string data, IReadOnlyMetadataContext? metadata, out object? value);
    }
}