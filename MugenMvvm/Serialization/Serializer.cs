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
        #region Constructors

        public Serializer(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public bool CanSerialize<TRequest>([DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<ISerializerComponent>().CanSerialize(request, metadata);
        }

        public Stream? TrySerialize<TRequest>([DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<ISerializerComponent>().TrySerialize(request, metadata);
        }

        public bool TryDeserialize(Stream stream, IReadOnlyMetadataContext? metadata, out object? value)
        {
            return GetComponents<ISerializerComponent>().TryDeserialize(stream, metadata, out value);
        }

        #endregion
    }
}