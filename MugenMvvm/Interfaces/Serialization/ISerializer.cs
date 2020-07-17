using System.IO;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializer : IComponentOwner<ISerializer>
    {
        bool TrySerialize(Stream stream, object request, IReadOnlyMetadataContext? metadata = null);

        bool TryDeserialize(Stream stream, IReadOnlyMetadataContext? metadata, out object? value);
    }
}