using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;

namespace MugenMvvm.Tests.Serialization
{
    public class TestSerializationManagerComponent : ISerializationManagerComponent, IHasPriority
    {
        public Func<ISerializer, ISerializationFormatBase, object?, IReadOnlyMetadataContext?, bool>? IsSupported { get; set; }

        public Func<ISerializer, object, ISerializationContext, object?>? TrySerialize { get; set; }

        public Func<ISerializer, object, ISerializationContext, object?>? TryDeserialize { get; set; }

        public int Priority { get; set; }

        bool ISerializationManagerComponent.IsSupported<TRequest, TResult>(ISerializer serializer, ISerializationFormatBase<TRequest, TResult> format, [AllowNull] TRequest request,
            IReadOnlyMetadataContext? metadata) =>
            IsSupported?.Invoke(serializer, format, request, metadata) ?? false;

        bool ISerializationManagerComponent.TrySerialize<TRequest, TResult>(ISerializer serializer, ISerializationFormat<TRequest, TResult> format, TRequest request,
            ISerializationContext serializationContext,
            [NotNullWhen(true)] [AllowNull] ref TResult result)
        {
            result = (TResult)TrySerialize?.Invoke(serializer, request!, serializationContext)!;
            return result != null;
        }

        bool ISerializationManagerComponent.TryDeserialize<TRequest, TResult>(ISerializer serializer, IDeserializationFormat<TRequest, TResult> format, TRequest request,
            ISerializationContext serializationContext,
            [NotNullWhen(true)] [AllowNull] ref TResult result)
        {
            result = (TResult)TryDeserialize?.Invoke(serializer, request!, serializationContext)!;
            return result != null;
        }
    }
}