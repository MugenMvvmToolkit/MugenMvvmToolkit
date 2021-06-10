using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;

namespace MugenMvvm.Tests.Serialization
{
    public class TestSerializerComponent<TRequest, TResult> : ISerializerComponent<TRequest, TResult>, IHasPriority
    {
        public Func<ISerializer, ISerializationFormat<TRequest, TResult>, TRequest?, IReadOnlyMetadataContext?, bool>? IsSupported { get; set; }

        public Func<ISerializer, ISerializationFormat<TRequest, TResult>, TRequest, TResult, ISerializationContext, object?>? TrySerialize { get; set; }

        public int Priority { get; set; }

        bool ISerializerComponent<TRequest, TResult>.IsSupported(ISerializer serializer, ISerializationFormat<TRequest, TResult> format, [AllowNull] TRequest request,
            IReadOnlyMetadataContext? metadata) =>
            IsSupported?.Invoke(serializer, format, request, metadata) ?? false;

        bool ISerializerComponent<TRequest, TResult>.TrySerialize(ISerializer serializer, ISerializationFormat<TRequest, TResult> format, TRequest request,
            ISerializationContext serializationContext,
            [NotNullWhen(true)] [AllowNull] ref TResult result)
        {
            result = (TResult)TrySerialize?.Invoke(serializer, format, request!, result!, serializationContext)!;
            return result != null;
        }
    }
}