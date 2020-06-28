using System.Diagnostics.CodeAnalysis;
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
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;

        #endregion

        #region Constructors

        public Serializer(IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(componentCollectionProvider)
        {
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Implementation of interfaces

        public bool TrySerialize<TRequest>(Stream stream, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(stream, nameof(stream));
            using var ctx = GetComponents<ISerializationContextProviderComponent>().TryGetSerializationContext(this, request, metadata) ?? new SerializationContext(metadata, _metadataContextProvider);
            return GetComponents<ISerializerComponent>().TrySerialize(stream, request, ctx);
        }

        public bool TryDeserialize(Stream stream, IReadOnlyMetadataContext? metadata, out object? value)
        {
            Should.NotBeNull(stream, nameof(stream));
            using var ctx = GetComponents<ISerializationContextProviderComponent>().TryGetDeserializationContext(this, metadata) ?? new SerializationContext(metadata, _metadataContextProvider);
            return GetComponents<ISerializerComponent>().TryDeserialize(stream, ctx, out value);
        }

        #endregion
    }
}