using System;
using System.IO;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization.Components;

namespace MugenMvvm.UnitTest.Serialization.Internal
{
    public class TestSerializerComponent : ISerializerComponent, IHasPriority
    {
        #region Properties

        public Func<Type, IReadOnlyMetadataContext?, bool>? CanSerialize { get; set; }

        public Func<object, IReadOnlyMetadataContext?, Stream?>? TrySerialize { get; set; }

        public Func<Stream, IReadOnlyMetadataContext?, object?>? TryDeserialize { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool ISerializerComponent.CanSerialize(Type targetType, IReadOnlyMetadataContext? metadata)
        {
            return CanSerialize?.Invoke(targetType, metadata) ?? false;
        }

        Stream? ISerializerComponent.TrySerialize(object target, IReadOnlyMetadataContext? metadata)
        {
            return TrySerialize?.Invoke(target, metadata);
        }

        object? ISerializerComponent.TryDeserialize(Stream stream, IReadOnlyMetadataContext? metadata)
        {
            return TryDeserialize?.Invoke(stream, metadata);
        }

        #endregion
    }
}