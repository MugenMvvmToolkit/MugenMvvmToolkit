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
        bool CanSerialize<TTarget>([DisallowNull] in TTarget target, IReadOnlyMetadataContext? metadata = null);

        Stream Serialize<TTarget>([DisallowNull] in TTarget target, IReadOnlyMetadataContext? metadata = null);

        object Deserialize(Stream stream, IReadOnlyMetadataContext? metadata = null);
    }
}