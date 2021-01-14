using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;
using Should;

namespace MugenMvvm.UnitTests.Serialization.Internal
{
    public class TestDeserializerComponent<TRequest, TResult> : IDeserializerComponent<TRequest, TResult>, IHasPriority
    {
        private readonly ISerializer? _serializer;

        public TestDeserializerComponent(ISerializer? serializer = null)
        {
            _serializer = serializer;
        }

        public Func<ISerializationFormatBase, object?, IReadOnlyMetadataContext?, bool>? IsSupported { get; set; }

        public Func<object, ISerializationContext, object?>? TryDeserialize { get; set; }

        public int Priority { get; set; }

        bool IDeserializerComponent<TRequest, TResult>.IsSupported(ISerializer serializer, IDeserializationFormat<TRequest, TResult> format, TRequest? request,
            IReadOnlyMetadataContext? metadata)
        {
            _serializer?.ShouldEqual(serializer);
            return IsSupported?.Invoke(format, request!, metadata) ?? false;
        }

        bool IDeserializerComponent<TRequest, TResult>.TryDeserialize(ISerializer serializer, IDeserializationFormat<TRequest, TResult> format, TRequest request,
            ISerializationContext serializationContext,
            [NotNullWhen(true)] ref TResult? result)
        {
            _serializer?.ShouldEqual(serializer);
            result = (TResult) TryDeserialize?.Invoke(request!, serializationContext)!;
            return result != null;
        }
    }
}