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

        public bool CanSerialize<TTarget>([DisallowNull]in TTarget target, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<ISerializerComponent>().CanSerialize(target, metadata);
        }

        public Stream Serialize<TTarget>([DisallowNull]in TTarget target, IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<ISerializerComponent>().TrySerialize(target, metadata);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return result;
        }

        public object Deserialize(Stream stream, IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<ISerializerComponent>().TryDeserialize(stream, metadata);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return result;
        }

        #endregion
    }
}