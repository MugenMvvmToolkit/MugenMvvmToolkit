using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;
using Should;

namespace MugenMvvm.UnitTests.Serialization.Internal
{
    public class TestSerializerComponent<TRequest, TResult> : ISerializerComponent<TRequest, TResult>, IHasPriority
    {
        private readonly ISerializer? _serializer;

        public TestSerializerComponent(ISerializer? serializer = null)
        {
            _serializer = serializer;
        }

        public Func<ISerializationFormatBase, object?, IReadOnlyMetadataContext?, bool>? IsSupported { get; set; }

        public Func<object, ISerializationContext, object?>? TrySerialize { get; set; }

        public int Priority { get; set; }

        bool ISerializerComponent<TRequest, TResult>.IsSupported(ISerializer serializer, ISerializationFormat<TRequest, TResult> format, [AllowNull] TRequest request,
            IReadOnlyMetadataContext? metadata)
        {
            _serializer?.ShouldEqual(serializer);
            return IsSupported?.Invoke(format, request, metadata) ?? false;
        }

        bool ISerializerComponent<TRequest, TResult>.TrySerialize(ISerializer serializer, ISerializationFormat<TRequest, TResult> format, TRequest request,
            ISerializationContext serializationContext,
            [NotNullWhen(true)] [AllowNull] ref TResult result)
        {
            _serializer?.ShouldEqual(serializer);
            result = (TResult) TrySerialize?.Invoke(request!, serializationContext)!;
            return result != null;
        }
    }
}