using System;
using System.IO;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization.Components;

namespace MugenMvvm.UnitTest.Serialization
{
    public class TestSerializerComponent : ISerializerComponent
    {
        #region Properties

        public Func<Type, IReadOnlyMetadataContext?, bool>? CanSerialize { get; set; }

        public Func<object, IReadOnlyMetadataContext?, Stream>? TrySerialize { get; set; }

        public Func<Stream, IReadOnlyMetadataContext?, object?>? TryDeserialize { get; set; }

        #endregion

        #region Implementation of interfaces

        bool ISerializerComponent.CanSerialize(Type targetType, IReadOnlyMetadataContext? metadata)
        {
            throw new NotImplementedException();
        }

        Stream? ISerializerComponent.TrySerialize(object target, IReadOnlyMetadataContext? metadata)
        {
            throw new NotImplementedException();
        }

        object? ISerializerComponent.TryDeserialize(Stream stream, IReadOnlyMetadataContext? metadata)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}