using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;

namespace MugenMvvm.Tests.Serialization
{
    public class TestDeserializerComponent<TRequest, TResult> : IDeserializerComponent<TRequest, TResult>, IHasPriority
    {
        public Func<ISerializer, IDeserializationFormat<TRequest, TResult>, TRequest?, IReadOnlyMetadataContext?, bool>? IsSupported { get; set; }

        public Func<ISerializer, IDeserializationFormat<TRequest, TResult>, TRequest, TResult, ISerializationContext, object?>? TryDeserialize { get; set; }

        public int Priority { get; set; }

        bool IDeserializerComponent<TRequest, TResult>.IsSupported(ISerializer serializer, IDeserializationFormat<TRequest, TResult> format, TRequest? request,
            IReadOnlyMetadataContext? metadata) =>
            IsSupported?.Invoke(serializer, format, request, metadata) ?? false;

        bool IDeserializerComponent<TRequest, TResult>.TryDeserialize(ISerializer serializer, IDeserializationFormat<TRequest, TResult> format, TRequest request,
            ISerializationContext serializationContext,
            [NotNullWhen(true)] ref TResult? result)
        {
            result = (TResult)TryDeserialize?.Invoke(serializer, format, request!, result!, serializationContext)!;
            return result != null;
        }
    }
}