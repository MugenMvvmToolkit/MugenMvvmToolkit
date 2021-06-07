using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;

namespace MugenMvvm.Tests.Serialization
{
    public class TestSerializationContextProvider : ISerializationContextProviderComponent, IHasPriority
    {
        public Func<ISerializer, ISerializationFormatBase, object, IReadOnlyMetadataContext?, ISerializationContext?>? TryGetSerializationContext { get; set; }

        public int Priority { get; set; }

        ISerializationContext? ISerializationContextProviderComponent.TryGetSerializationContext<TRequest, TResult>(ISerializer serializer,
            ISerializationFormatBase<TRequest, TResult> format, TRequest request,
            IReadOnlyMetadataContext? metadata) =>
            TryGetSerializationContext?.Invoke(serializer, format, request!, metadata);
    }
}