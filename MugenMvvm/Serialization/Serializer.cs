using System.IO;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;

namespace MugenMvvm.Serialization
{
    public sealed class Serializer : ComponentOwnerBase<ISerializer>, ISerializer
    {
        #region Constructors

        public Serializer(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        #endregion

        #region Implementation of interfaces

        public bool TrySerialize(Stream stream, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(stream, nameof(stream));
            using var ctx = GetComponents<ISerializationContextProviderComponent>().TryGetSerializationContext(this, stream, request, metadata) ?? new SerializationContext(stream, true, metadata);
            return GetComponents<ISerializerComponent>().TrySerialize(this, request, ctx);
        }

        public bool TryDeserialize(Stream stream, IReadOnlyMetadataContext? metadata, out object? value)
        {
            Should.NotBeNull(stream, nameof(stream));
            using var ctx = GetComponents<ISerializationContextProviderComponent>().TryGetDeserializationContext(this, stream, metadata) ?? new SerializationContext(stream, false, metadata);
            return GetComponents<ISerializerComponent>().TryDeserialize(this, ctx, out value);
        }

        #endregion
    }
}