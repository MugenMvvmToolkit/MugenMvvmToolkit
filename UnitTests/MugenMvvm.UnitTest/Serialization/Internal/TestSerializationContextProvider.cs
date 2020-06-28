using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;

namespace MugenMvvm.UnitTest.Serialization.Internal
{
    public class TestSerializationContextProvider : ISerializationContextProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<ISerializer, object, Type, IReadOnlyMetadataContext?, ISerializationContext?>? TryGetSerializationContext { get; set; }

        public Func<ISerializer, IReadOnlyMetadataContext?, ISerializationContext?>? TryGetDeserializationContext { get; set; }

        #endregion

        #region Implementation of interfaces

        ISerializationContext? ISerializationContextProviderComponent.TryGetSerializationContext<TRequest>(ISerializer serializer, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryGetSerializationContext?.Invoke(serializer, request!, typeof(TRequest), metadata);
        }

        ISerializationContext? ISerializationContextProviderComponent.TryGetDeserializationContext(ISerializer serializer, IReadOnlyMetadataContext? metadata)
        {
            return TryGetDeserializationContext?.Invoke(serializer, metadata);
        }

        #endregion
    }
}