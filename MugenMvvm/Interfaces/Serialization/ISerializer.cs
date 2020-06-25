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

        Stream? TrySerialize<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null);

        bool TryDeserialize(Stream stream, IReadOnlyMetadataContext? metadata, out object? value);
    }
}