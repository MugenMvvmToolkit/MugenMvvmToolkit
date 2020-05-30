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

        Stream Serialize<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null);

        object Deserialize(Stream stream, IReadOnlyMetadataContext? metadata = null);
    }
}