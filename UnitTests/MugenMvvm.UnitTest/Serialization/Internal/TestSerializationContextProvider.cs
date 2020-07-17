using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;
using Should;

namespace MugenMvvm.UnitTest.Serialization.Internal
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

        public Func<object, IReadOnlyMetadataContext?, ISerializationContext?>? TryGetSerializationContext { get; set; }

        public Func<IReadOnlyMetadataContext?, ISerializationContext?>? TryGetDeserializationContext { get; set; }

        #endregion

        #region Implementation of interfaces

        ISerializationContext? ISerializationContextProviderComponent.TryGetSerializationContext(ISerializer serializer, object request, IReadOnlyMetadataContext? metadata)
        {
            _serializer?.ShouldEqual(serializer);
            return TryGetSerializationContext?.Invoke(request!, metadata);
        }

        ISerializationContext? ISerializationContextProviderComponent.TryGetDeserializationContext(ISerializer serializer, IReadOnlyMetadataContext? metadata)
        {
            _serializer?.ShouldEqual(serializer);
            return TryGetDeserializationContext?.Invoke(metadata);
        }

        #endregion
    }
}