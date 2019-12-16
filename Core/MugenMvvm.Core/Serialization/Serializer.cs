using System;
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

        public bool CanSerialize(Type targetType, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<ISerializerComponent>().CanSerialize(targetType, metadata);
        }

        public Stream Serialize(object target, IReadOnlyMetadataContext? metadata = null)
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