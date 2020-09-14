using System;
using System.IO;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;
using Should;

namespace MugenMvvm.UnitTests.Serialization.Internal
{
    public class TestSerializationContextProvider : ISerializationContextProviderComponent, IHasPriority
    {
        #region Fields

        private readonly ISerializer? _serializer;

        #endregion

        #region Constructors

        public TestSerializationContextProvider(ISerializer? serializer = null)
        {
            _serializer = serializer;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Func<Stream, object, IReadOnlyMetadataContext?, ISerializationContext?>? TryGetSerializationContext { get; set; }

        public Func<Stream, IReadOnlyMetadataContext?, ISerializationContext?>? TryGetDeserializationContext { get; set; }

        #endregion

        #region Implementation of interfaces

        ISerializationContext? ISerializationContextProviderComponent.TryGetSerializationContext(ISerializer serializer, Stream stream, object request, IReadOnlyMetadataContext? metadata)
        {
            _serializer?.ShouldEqual(serializer);
            return TryGetSerializationContext?.Invoke(stream, request, metadata);
        }

        ISerializationContext? ISerializationContextProviderComponent.TryGetDeserializationContext(ISerializer serializer, Stream stream, IReadOnlyMetadataContext? metadata)
        {
            _serializer?.ShouldEqual(serializer);
            return TryGetDeserializationContext?.Invoke(stream, metadata);
        }

        #endregion
    }
}